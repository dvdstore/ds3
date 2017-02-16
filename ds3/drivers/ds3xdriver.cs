/*
 * Generalized DVD Store 3 Driver Program - ds3xdriver.cs
 *
 * Copyright (C) 2005 Dell, Inc. <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
 *
 * Generates orders against DVD Store Database 3 through web interface or directly against database
 * Simulates users logging in to store or creating new customer data; browsing for DVDs by title, actor or 
 * category; creating new product reviews, browsing product reviews, and purchasing selected DVDs
 *
 * To see syntax: ds3xdriver   where x= web, mysql, sqlserver or oracle
 *
 * Compile with appropriate functions file to generate driver for web, SQL Server, MySQL, Oracle or PostgreSQL target:
 *  csc /out:ds3webdriver.exe       ds3xdriver.cs ds3webfns.cs       /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS
 *  csc /out:ds3sqlserverdriver.exe ds3xdriver.cs ds3sqlserverfns.cs /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS
 *  csc /out:ds3mysqldriver.exe     ds3xdriver.cs ds3mysqlfns.cs     /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  /r:<path>MySql.Data.dll
 *  csc /out:ds3oracledriver.exe    ds3xdriver.cs ds3oraclefns.cs    /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  /r:<path>Oracle.DataAccess.dll
 *  csc /out:ds3pgsqldriver.exe     ds3xdriver.cs ds3pgsqlfns.cs     /d:USE_WIN32_TIMER /d:GENPERF_CTRS   /r:<path>Npgsql.dll
 *
 *  USE_WIN32_TIMER: if defined, program will use high resolution WIN32 timers
 *  GEN_PERF_CTRS: if defined, program will generate Windows Perfmon performance counters
 *
 *  csc is installed with Microsoft.NET   Typical location: C:\WINNT\Microsoft.NET\Framework\v2.0.50727
 *
 * Updated 6/14/2010 by GSK(girish.khadke@gmail.com)
 * Updated 5/12/11 by DJ (cleaned up output; minor fixes)
 * Last updated 5/15/15 by TM - updated for DS3 from DS2- support for new order process with reviews and membership.
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA  */



using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Text;   //Added by GSK

namespace ds2xdriver
  {
  /// <summary>
  /// ds2xdriver: drives DVD Store 2 Database through web interface or directly against database
  /// </summary>

  public class GlobalConstants
    {
    public const int MAX_USERS = 1000;
    public const int MAX_CATEGORY = 16;
    public const int MAX_ROWS = 100;
    public const int LAST_N = 100;
    public const int MAX_FAILURES = 10;
    }

  //
  //-------------------------------------------------------------------------------------------------
  //
  class Controller
    {
    // If compile option /d:USE_WIN32_TIMER is specified will use 64b QueryPerformance counter from Win32
    // Else will use .NET DateTime class      
#if (USE_WIN32_TIMER)
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceCounter(ref long x);
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceFrequency(ref long x);   
#endif

    // Variables needed by User objects 
    public static string target , windows_perf_host = null;
    public static string outfilename;
    public static string ds2_mode_string;
    System.IO.StreamWriter outfile;
    
    public static string[] target_servers;                   //Added by GSK (for single instance of driver program driving multiple database servers)
    public static string[] windows_perf_host_servers;       //Added by GSK
    public static int n_target_servers = 1;                 //Added by GSK to keep track of number of Servers/DB instances on which threads spawned
    public static object UpdateLock = 1;
    public static int n_threads , n_threads_running = 0 , n_threads_connected = 0;
    public static int n_overall = 0 , n_login_overall = 0 , n_newcust_overall = 0 , n_browse_overall = 0 ,
      n_purchase_overall = 0 , n_rollbacks_overall = 0 , n_rollbacks_from_start = 0 , n_purchase_from_start = 0 , n_cpu_pct_samples = 0;
    public static int n_reviewbrowse_overall = 0, n_newreview_overall = 0, n_newhelpfulness_overall = 0, n_newmember_overall = 0;
    public static double pct_rollbacks;

    //Added by GSK
    public static int[] arr_n_login_overall;
    public static double[] arr_rt_login_overall;
    public static int[] arr_n_newcust_overall;
    public static double[] arr_rt_newcust_overall;
    public static int[] arr_n_browse_overall;
    public static double[] arr_rt_browse_overall;
    public static int[] arr_n_purchase_overall;
    public static double[] arr_rt_purchase_overall;
    public static int[] arr_n_rollbacks_overall;
    public static int[] arr_n_overall;
    public static double[] arr_rt_tot_overall;
    public static int[] arr_n_purchase_from_start;
    public static int[] arr_n_rollbacks_from_start;
    public static double[,] arr_rt_tot_lastn;
    public static double[] arr_cpu_pct_tot;
    public static int[] arr_n_cpu_pct_samples;
    // Added by TM 3/17/2015
    public static int[] arr_n_reviewbrowse_overall;
    public static double[] arr_rt_reviewbrowse_overall;
    public static int[] arr_n_newreview_overall;
    public static double[] arr_rt_newreview_overall;
    public static int[] arr_n_newhelpfulness_overall;
    public static double[] arr_rt_newhelpfulness_overall;
    public static int[] arr_n_newmember_overall;
    public static double[] arr_rt_newmember_overall;

    public static int pct_newcustomers = 0 , n_searches , search_batch_size , n_line_items , ramp_rate;
    public static int pct_newreviews = 0, n_reviews;
    public static int pct_newhelpfulness = 0;
    public static int pct_newmember = 0;
    public static double think_time , rt_tot_overall = 0.0 , rt_login_overall = 0.0 , rt_newcust_overall = 0.0 ,
      rt_browse_overall = 0.0 , rt_purchase_overall = 0.0 , cpu_pct_tot = 0.0;
    public static double rt_reviewbrowse_overall = 0.0, rt_newreview_overall = 0.0, rt_newhelpfulness_overall = 0.0,
        rt_newmember_overall = 0.0;
    public static double[] rt_tot_lastn = new double[GlobalConstants.LAST_N];
    public static bool Start = false , End = false;
    public static int[] MAX_CUSTOMER = new int[] { 20000 , 2000000 , 200000000 };
    public static int[] MAX_PRODUCT = new int[] { 10000 , 100000 , 1000000 };
    public static int max_customer , max_product , prod_array_size, max_review;
    //public static int[] prod_array = new int[1100000];
    //Changed by GSK (size of this array will depend on number of rows in product table)
    public static int[] prod_array;
    public static string virt_dir = "ds3" , page_type = "php";

    //Added new parameter database_custom_size and new variables by GSK 
    //Note that order_rows are per month
    public static int customer_rows , order_rows , product_rows;
    public static string db_size = "10MB";

    //Added by GSK (New parameter to Print detailed or aggregate output  Values = "Y" or "N" Default value = "N"
    public static string detailed_view = "N";
    public static bool is_detailed_view = true;

    //Added by GSK( New parameter to print Linux CPU utilization statistics)
    public static string linux_perf_host = null;
    public static string[] linux_perf_host_servers;
    public static string[] linux_unames;
    public static string[] linux_passwd;
    public static double[] arr_linux_cpu_utilization;       //Used for book keeping purposes
    //Keep track of number of windows and linux VM's on which to drive workload on 
    public static int n_windows_servers = 0;
    public static int n_linux_servers = 0;
    //Boolean values to check if there are linux and windows target VM's
    public static bool is_Lin_VM = false;
    public static bool is_Win_VM = false;
    //Boolean value to simulate DS2 version of driver on DS3 database
    public static bool ds2_mode = false;

    // Variables needed within Controller class
    // Added new Parameter db_size by GSK
    // db_size will indicate actual database size (e.g. Values for this parameter can be like 10MB or 150GB) 
    //db_size_str parameter is removed since it would not be used in code anywhere
    //Instead at same place we need db_size parameter
    //Added new parameter detailed_view by GSK default value = N
    //Added new parameter linux_perf_host by GSK 
    static string[] input_parm_names = new string[] {"config_file", "target", "n_threads", "ramp_rate",
      "run_time", "db_size", "warmup_time", "think_time", "pct_newcustomers", "pct_newmember", "n_searches",
      "search_batch_size", "n_reviews", "pct_newreviews", "pct_newhelpfulness", "n_line_items", "virt_dir", 
      "page_type", "windows_perf_host", "linux_perf_host", "detailed_view", "out_filename", "ds2_mode"};
    static string[] input_parm_desc = new string[] {"config file path", 
      "database/web server hostname or IP address", "number of driver threads", "startup rate (users/sec)",
      "run time (min) - 0 is infinite", "S | M | L or database size (e.g. 30MB, 80GB)", "warmup_time (min)", "think time (sec)", 
      "percent of customers that are new customers", "percent of orders that will have a customer upgrade their membership",
      "average number of searches per order", "average number of items returned in each search", 
      "average number of product review searches per order", "percent of orders where customer will create a new review",
      "percent of orders where customer will rate an existing review for its helpfulness", "average number of items per order",
      "virtual directory (for web driver)", "web page type (for web driver)", "target hostname for Perfmon CPU% display (Windows only)",
      "username:password:target hostname/IP Address for Linux CPU% display (Linux Only)",
      "Detailed statistics View (Y / N)", "output results to specified file in csv format", "run driver in ds2 mode to mimic previous version"};
    static string[] input_parm_values = new string[] {"none", "localhost", "1", "10", "0", "10MB", "1", "0",
      "20", "1", "3", "5", "3", "5", "10", "5", "ds3", "php", "","","N","","N"};

    int server_id = 0;          //Added by GSK
    
    //
    //-------------------------------------------------------------------------------------------------
    //    
    [STAThread]
    static void Main ( string[] args )
      {
      new Controller ( args );
      }
    //
    //-------------------------------------------------------------------------------------------------
    //
    //Added by GSK to register RSA fingerprint / host key in registry before using plink to get CPU data
    bool RegisterRSAHostKey ( string machine_name , string user , string passwd )
      {
      try
        {
        Process p = new Process ( );
        //These arguments will ensure than yes = y will automatically be answered
        // -l root -pw password 11.22.33.44 exit
        string p_args = " -l " + user + " -pw " + passwd + " " + machine_name + " exit";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = false;
        //We need to set environment variable PLINK_PATH to give full path of plink.exe on machine on which driver program is executing
        p.StartInfo.FileName = System.Environment.GetEnvironmentVariable ( "PLINK_PATH");
        p.StartInfo.Arguments = p_args;
        //Run plink to register Host key in registry
        p.Start ( );
        StreamWriter strm_Writer = p.StandardInput;
        strm_Writer.AutoFlush = true;
        strm_Writer.Write ( "y" );      //This will automatically give answer as y when yes/no question is asked to add host key
        strm_Writer.Write ( "\n" );     //Simulate pressing enter key
        p.WaitForExit ( );              //Wait till process finishes
        }
      catch(System.Exception e)
        {
        //In case of any exception like error in connection to target linux host, directly throw exception to caller of this function
        throw e;
        }
      return true;
      }

    //
    //-------------------------------------------------------------------------------------------------
    //      

    //Run BackGround Mpstat to target machine
    void RunBackGroundmpStat ( string machine_name , string user , string passwd )
      {
      try
        {
        String s_retValue = "";                    
        Process p = new Process ( );
        //These arguments will ensure than yes = y will automatically be answered
        // -l root -pw password 11.22.33.44 exit
        //Submit background task to write mpstat output for 10 seconds to a file
        string p_args = " -l " + user + " -pw " + passwd + " " + machine_name + " cd /; nohup mpstat 1 10 > /cpuutil.txt 2> /cpuutil.err < /dev/null &";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.RedirectStandardOutput = true;
        //We need to set environment variable PLINK_PATH to give full path of plink.exe on machine on which driver program is executing
        p.StartInfo.FileName = System.Environment.GetEnvironmentVariable ( "PLINK_PATH");
        p.StartInfo.Arguments = p_args;
        //Run plink to get CPU utilization by running bash script on remote shell
        p.Start ( );
        StreamReader strm_Reader = p.StandardOutput;
        s_retValue = strm_Reader.ReadToEnd ( );
        p.WaitForExit ( );              //Wait till process finishes
        }
      catch ( System.Exception e )
        {
        //In case of exception throw exception directly to caller of this function
        throw e;
        }                
      }

    //Read remove text file to get CPUutilization
    double ReadRemoteTextFile ( string machine_name , string user , string passwd )
      {
      double cpuutilizn = 0.0;
      try
        {
        String s_retValue;                    
        Process p = new Process ( );
        //These arguments will ensure than yes = y will automatically be answered
        // -l root -pw password 11.22.33.44 exit
        //Submit background task to write mpstat output for 10 seconds to a file
        string p_args = " -l " + user + " -pw " + passwd + " " + machine_name + " cd /; grep 'Average:' /cpuutil.txt ; exit;";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.RedirectStandardOutput = true;
        //We need to set environment variable PLINK_PATH to give full path of plink.exe on machine on which driver program is executing
        p.StartInfo.FileName = System.Environment.GetEnvironmentVariable ( "PLINK_PATH");
        p.StartInfo.Arguments = p_args;
        //Run plink to get CPU utilization by running bash script on remote shell
        p.Start ( );
        StreamReader strm_Reader = p.StandardOutput;
        s_retValue = strm_Reader.ReadToEnd ( );
        p.WaitForExit ( );              //Wait till process finishes

        if(s_retValue == "")
          {
          throw new System.Exception("No value returned after reading file!! Check whether file created on target system or not!!");
          }

        //Remove all extra white spaces and have only one whitespace
        //String before:Average:     all   13.56    0.00    1.16    4.50    0.06    0.16    0.00    0.00   80.55
        //String after: Average: all 13.56 0.00 1.16 4.50 0.06 0.16 0.00 0.00 80.55
        s_retValue = System.Text.RegularExpressions.Regex.Replace(s_retValue,@"\s{2,}", " ");
        String[] arr_strSplit = s_retValue.Split(' ');
                    
        //Get User, Nice, System values from string and add to get CPU utilization
        cpuutilizn = Convert.ToDouble(arr_strSplit[2]) + Convert.ToDouble(arr_strSplit[3]) + Convert.ToDouble(arr_strSplit[4]);                    
        }
      catch ( System.Exception e )
        {
        //In case of exception throw exception directly to caller of this function
        throw e;
        }    
      return cpuutilizn;
      }

    //-------------------------------------------------------------------------------------------------
    //    
    //Function written by GSK to calculate number of Rows in tables of database according to database size
    void CalculateNumberOfRows ( string str_db_size )
      {
      string db_custom_size = str_db_size;
      int i_db_custom_size = 10;          //Default 10mb
      string str_is_mb_gb = "mb";
      db_custom_size = db_custom_size.ToLower ( );  //For case insensitivity
      if ( db_custom_size.IndexOf ( "mb" ) != -1 )
        {
        str_is_mb_gb = db_custom_size.Substring ( db_custom_size.IndexOf ( "mb" ) , 2 );
        try
          {
          i_db_custom_size = Convert.ToInt32 ( db_custom_size.Substring ( 0 , db_custom_size.IndexOf ( "mb" ) ) );
          if ( i_db_custom_size <= 0 )
            {
            throw new System.Exception ( "db_size must be greater than 0!!" );
            }
          }
        catch ( System.Exception e )
          {
          throw e;
          }
        }
      else if ( db_custom_size.IndexOf ( "gb" ) != -1 )
        {
        str_is_mb_gb = db_custom_size.Substring ( db_custom_size.IndexOf ( "gb" ) , 2 );
        try
          {
          i_db_custom_size = Convert.ToInt32 ( db_custom_size.Substring ( 0 , db_custom_size.IndexOf ( "gb" ) ) );
          if ( i_db_custom_size <= 0 )
            {
            throw new System.Exception ( "db_size must be greater than 0!!" );
            }
          }
        catch ( System.Exception e )
          {
          throw e;
          }
        }
      else
        {
        //Wrong parameter specified
        throw new Exception ( "Wrong value for parameter db_size specified!!" );
        }

      //Everything is OK in parameter, so now calculate number of rows in each of customers, orders and products tables
      //Note that order_rows are per month
      int mult_cust_rows = 0 , mult_ord_rows = 0 , mult_prod_rows = 0;
      double ratio = 0;
      //Size is in MB  (Database can be only in range 1 mb to 1024 mb - Small instance S)
      if ( String.Compare ( str_is_mb_gb , "mb" ) == 0 )
        {
        ratio = ( double ) ( i_db_custom_size / 10.0 );
        mult_cust_rows = 20000;
        mult_ord_rows = 1000;
        mult_prod_rows = 10000;
        }
      else if ( String.Compare ( str_is_mb_gb , "gb" ) == 0 ) //Size is in GB (database can be 1 GB (Medium instance M) or > 1 GB (Larger instance L)
        {
        if ( i_db_custom_size == 1 )  //Medium M size 1 GB database
          {
          ratio = ( double ) ( i_db_custom_size / 1.0 );
          mult_cust_rows = 2000000;
          mult_ord_rows = 100000;
          mult_prod_rows = 100000;
          }
        else  //Size > 1 GB Large L size database
          {
          ratio = ( double ) ( i_db_custom_size / 100.0 );
          mult_cust_rows = 200000000;
          mult_ord_rows = 10000000;
          mult_prod_rows = 1000000;
          }
        }

      //Initialize number of rows in table according to ratio calculated for custom database size
      customer_rows = ( int ) ( ratio * mult_cust_rows );
      order_rows = ( int ) ( ratio * mult_ord_rows );
      product_rows = ( int ) ( ratio * mult_prod_rows );

      }

    //    
    //-------------------------------------------------------------------------------------------------
    //   
    Controller ( string[] argarray )
      {
      //Console.WriteLine("Controller constructor: " + argarray.Length + " args");

      int i;
            int z;
      int i_sec , run_time = 0 , warmup_time = 1;
      //Changed by GSK
      //int db_size=0;
      //string db_size_str, errmsg=null;
      string errmsg = null;
      double et;
      int opm , rt_login_avg_msec , rt_newcust_avg_msec , rt_browse_avg_msec , rt_purchase_avg_msec ,
        rt_tot_lastn_max_msec , rt_tot_avg_msec;
      int rt_reviewbrowse_avg_msec, rt_newreview_avg_msec, rt_newhelpfulness_avg_msec, rt_newmember_avg_msec;
      double rt_tot_lastn_max;

      //Added by GSK
      int old_n_overall = 0;
      int[] arr_old_n_overall;
      int diff_n_overall = 0;
      int[] arr_diff_n_overall;
      double old_rt_tot_overall = 0.0;
      double[] arr_old_rt_tot_overall;
      double diff_rt_tot_overall = 0.0;
      double[] arr_diff_rt_tot_overall;
      int[] arr_rt_tot_sampled;
      int rt_tot_sampled = 0;

      //Added by GSK
      int[] arr_opm;
      int[] arr_rt_login_avg_msec;
      int[] arr_rt_newcust_avg_msec;
      int[] arr_rt_browse_avg_msec;
      int[] arr_rt_reviewbrowse_avg_msec;
      int[] arr_rt_newreview_avg_msec;
      int[] arr_rt_newhelpfulness_avg_msec;
      int[] arr_rt_newmember_avg_msec;
      int[] arr_rt_purchase_avg_msec;
      int[] arr_rt_tot_lastn_max_msec;
      int[] arr_rt_tot_avg_msec;
      double arr_rt_tot_lastn_max;

      //Added by GSK (Keeps track of total and utilizn of bunch of linux and windows VM's)
      double total_cpu_utilzn = 0.0;
      double total_win_cpu_utilzn = 0.0;
      double total_lin_cpu_utilzn = 0.0;

    
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan ( );
      DateTime DT0;
#endif

      User[] users = new User[GlobalConstants.MAX_USERS];
      Thread[] threads = new Thread[GlobalConstants.MAX_USERS];

      if ( argarray.Length == 0 )
        {
        // display input parameter info
        Console.WriteLine ( "\nEnter parameters with format --parm_name=parm_value" );
        Console.WriteLine ( "And/or use a config file with argument --config_file=(config file path)" );
        Console.WriteLine ( "Parms will be evaluated left to right" );
        Console.WriteLine ( "\n{0,-20}{1,-52}{2}\n" , "Parameter Name" , "Description" , "Default Value" );
        for ( i = 0 ; i < input_parm_names.Length ; i++ )
          {
          Console.WriteLine ( "{0,-20}{1,-52}{2}" , input_parm_names[i] , input_parm_desc[i] , input_parm_values[i] );
          }
        return;
        }

      // send args to parse_args, return 0 or # of parms set, error_message if any
      // parsed values are in array input_parm_values
      i = parse_args ( argarray , ref errmsg );
      if ( i != 0 ) { }//Console.WriteLine("{0} parameters parsed", i);
      else
        {
        Console.WriteLine ( errmsg );
        return;
        }

      // Set parameters from input_parm_values 
      //target = input_parm_values[Array.IndexOf ( input_parm_names , "target" )];

      //Added try catch block by GSK
      try
        {

        target = input_parm_values[Array.IndexOf ( input_parm_names , "target" )];                
        target_servers = target.Split ( ';' );
        n_target_servers = target_servers.Length;   //Added by GSK to keep track of number of Target Servers
        //Added by GSK
        //Dynamically allocate memory Initialize arrays for book keeping for individual Servers on which test runs
        arr_n_login_overall = new int[n_target_servers];
        arr_rt_login_overall = new double[n_target_servers];
        arr_n_newcust_overall = new int[n_target_servers];
        arr_rt_newcust_overall = new double[n_target_servers];
        arr_n_browse_overall = new int[n_target_servers];
        arr_rt_browse_overall = new double[n_target_servers];
        arr_n_reviewbrowse_overall = new int[n_target_servers];
        arr_rt_reviewbrowse_overall = new double[n_target_servers];
        arr_n_newreview_overall = new int[n_target_servers];
        arr_rt_newreview_overall = new double[n_target_servers];
        arr_n_newhelpfulness_overall = new int[n_target_servers];
        arr_rt_newhelpfulness_overall = new double[n_target_servers];
        arr_n_newmember_overall = new int[n_target_servers];
        arr_rt_newmember_overall = new double[n_target_servers];
        arr_n_purchase_overall = new int[n_target_servers];
        arr_rt_purchase_overall = new double[n_target_servers];
        arr_n_rollbacks_overall = new int[n_target_servers];
        arr_n_overall = new int[n_target_servers];
        arr_rt_tot_overall = new double[n_target_servers];
        arr_n_purchase_from_start = new int[n_target_servers];
        arr_n_rollbacks_from_start = new int[n_target_servers];
        arr_rt_tot_lastn = new double[n_target_servers,GlobalConstants.LAST_N];

        arr_opm = new int[n_target_servers];
        arr_rt_login_avg_msec = new int[n_target_servers];
        arr_rt_newcust_avg_msec = new int[n_target_servers];
        arr_rt_browse_avg_msec = new int[n_target_servers];
        arr_rt_reviewbrowse_avg_msec = new int[n_target_servers];
        arr_rt_newreview_avg_msec = new int[n_target_servers];
        arr_rt_newhelpfulness_avg_msec = new int[n_target_servers];
        arr_rt_newmember_avg_msec = new int[n_target_servers];
        arr_rt_purchase_avg_msec = new int[n_target_servers];
        arr_rt_tot_lastn_max_msec = new int[n_target_servers];
        arr_rt_tot_avg_msec = new int[n_target_servers];

        arr_rt_tot_lastn_max = 0.0;

        old_n_overall = 0;
        diff_n_overall = 0;
        old_rt_tot_overall = 0.0;
        diff_rt_tot_overall = 0.0;

        //Added on 8/8/2010
        arr_old_n_overall = new int[n_target_servers];
        arr_diff_n_overall = new int[n_target_servers];
        arr_old_rt_tot_overall = new double[n_target_servers];
        arr_diff_rt_tot_overall = new double[n_target_servers];
        arr_rt_tot_sampled = new int[n_target_servers];

        for ( i = 0 ; i < n_target_servers ; i++ )
          {
          arr_n_login_overall[i] = 0;
          arr_rt_login_overall[i] = 0.0;
          arr_n_newcust_overall[i] = 0;
          arr_rt_newcust_overall[i] = 0.0;
          arr_n_newmember_overall[i] = 0;
          arr_rt_newmember_overall[i] = 0.0;
          arr_n_browse_overall[i] = 0;
          arr_rt_browse_overall[i] = 0.0;
          arr_n_reviewbrowse_overall[i] = 0;
          arr_rt_reviewbrowse_overall[i] = 0.0;
          arr_n_newreview_overall[i] = 0;
          arr_rt_newreview_overall[i] = 0.0;
          arr_n_newhelpfulness_overall[i] = 0;
          arr_rt_newhelpfulness_overall[i] = 0.0;
          arr_n_purchase_overall[i] = 0;
          arr_rt_purchase_overall[i] = 0.0;
          arr_n_rollbacks_overall[i] = 0;
          arr_n_overall[i] = 0;
          arr_rt_tot_overall[i] = 0.0;
          arr_n_purchase_from_start[i] = 0;
          arr_n_rollbacks_from_start[i] = 0;

          arr_opm[i] = 0;
          arr_rt_login_avg_msec[i] = 0;
          arr_rt_newcust_avg_msec[i] = 0;
          arr_rt_newmember_avg_msec[i] = 0;
          arr_rt_browse_avg_msec[i] = 0;
          arr_rt_reviewbrowse_avg_msec[i] = 0;
          arr_rt_newreview_avg_msec[i] = 0;
          arr_rt_newhelpfulness_avg_msec[i] = 0;
          arr_rt_purchase_avg_msec[i] = 0;
          arr_rt_tot_lastn_max_msec[i] = 0;
          arr_rt_tot_avg_msec[i] = 0;

          //Added on 8/8/2010
          arr_old_n_overall[i] = 0;
          arr_diff_n_overall[i] = 0;
          arr_old_rt_tot_overall[i] = 0.0;
          arr_diff_rt_tot_overall[i] = 0.0;
          arr_rt_tot_sampled[i] = 0;

          for ( int l = 0 ; l < GlobalConstants.LAST_N ; l++ )
            {
            arr_rt_tot_lastn[i,l] = 0.0;
            }
          }                
        }
        
      catch(System.Exception e)
        {
        Console.WriteLine ( "Error in converting parameter target: {0}" , e.Message );
        return;
        }

      try
        {
        n_threads = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "n_threads" )] );                
                //Changed by GSK -- n_threads represents threads spawned per DB/Web Server
                //Hence total number of threads spawned by Controller Driver Program = no of threads per Server * number of servers to Drive Workload on
                n_threads = n_threads * n_target_servers;
                Console.WriteLine ( "Total number of Threads to be Spawned across multiple servers are n_threads: {0}" , n_threads );
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter n_threads: {0}" , e.Message );
        return;
        }
      try
        {
        ramp_rate = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "ramp_rate" )] );
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter ramp_rate: {0}" , e.Message );
        return;
        }
      try
        {
        run_time = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "run_time" )] );
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter run_time: {0}" , e.Message );
        return;
        }


      //db_size_str = input_parm_values[Array.IndexOf(input_parm_names, "db_size_str")];

            //Changed by GSK
      //This parameter db_size_str will not be used in case of Custom database size since CalculateNumberOfRows() calculates rows in tables 
      //on the fly according to database size passed as parameter            
      //string sizes= "SML";
      //if ((db_size = sizes.IndexOf(db_size_str.ToUpper())) < 0)
      //  {
      //      Console.WriteLine("Error: db_size_str must be one of S, M or L");
      //      return;
      //  }

      //Code for new parameter and new function to initialize number of rows 
      //Added by GSK
      db_size = input_parm_values[Array.IndexOf ( input_parm_names , "db_size" )];
      if ( db_size == "" )
        {
        Console.WriteLine ( "Error: Wrong db_size parameter value specified" );
        return;
        }
      try
        {
        if ( db_size.ToUpper ( ) == "S" ) db_size = "10MB";        //These if and else if's are to ensure code works with older S | M | L parameters too
        else if ( db_size.ToUpper ( ) == "M" ) db_size = "1GB";
        else if ( db_size.ToUpper ( ) == "L" ) db_size = "100GB";
        CalculateNumberOfRows ( db_size );
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in Calculating number of rows in table according to db_size parameter: {0}" , e.Message );
        return;
        }

      try
        {
        warmup_time = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "warmup_time" )] );
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter warmup_time: {0}" , e.Message );
        return;
        }
      try
        {
        think_time = Convert.ToDouble ( input_parm_values[Array.IndexOf ( input_parm_names , "think_time" )] );
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter think_time: {0}" , e.Message );
        return;
        }
      try
        {
        pct_newcustomers =
          Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "pct_newcustomers" )] );
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter pct_newcustomers: {0}" , e.Message );
        return;
        }
      try
        {
        n_searches = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "n_searches" )] );
        if ( n_searches <= 0 )
          {
          Console.WriteLine ( "n_searches must be greater than 0" );
          return;
          }
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter n_searches: {0}" , e.Message );
        return;
        }
      try
        {
        search_batch_size = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names ,
          "search_batch_size" )] );
        if ( search_batch_size <= 0 )
          {
          Console.WriteLine ( "search_batch_size must be greater than 0" );
          return;
          }
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter search_batch_size: {0}" , e.Message );
        return;
        }
      try
        {
        n_line_items = Convert.ToInt32 ( input_parm_values[Array.IndexOf ( input_parm_names , "n_line_items" )] );
        if ( n_line_items <= 0 )
          {
          Console.WriteLine ( "n_line_items must be greater than 0" );
          return;
          }
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter n_line_items: {0}" , e.Message );
        return;
        }

      virt_dir = input_parm_values[Array.IndexOf ( input_parm_names , "virt_dir" )];
      page_type = input_parm_values[Array.IndexOf ( input_parm_names , "page_type" )];
           
      //windows_perf_host = input_parm_values[Array.IndexOf ( input_parm_names , "windows_perf_host" )];
      //if ( windows_perf_host == "" ) windows_perf_host = null;

      //Added by GSK
      try
        {
        windows_perf_host = input_parm_values[Array.IndexOf ( input_parm_names , "windows_perf_host" )];
        if ( windows_perf_host == "" )
          {
          windows_perf_host = null;
          windows_perf_host_servers = null;
          n_windows_servers = 0;
          }
        else
          {
          windows_perf_host_servers = windows_perf_host.Split ( ';' );
          n_windows_servers = windows_perf_host_servers.Length;
          is_Win_VM = true;

          //Allocate memory and initialize
          arr_cpu_pct_tot = new double[n_windows_servers];
          arr_n_cpu_pct_samples = new int[n_windows_servers];
          for ( i = 0 ; i < n_windows_servers ; i++ )
            {
            arr_cpu_pct_tot[i] = 0.0;
            arr_n_cpu_pct_samples[i] = 0;
            }
          }
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter windows_perf_host: {0}" , e.Message );
        return;
        }

      //Added by GSK for new parameter linux_perf_host only in case of linux
      try
        {
        linux_perf_host = input_parm_values[Array.IndexOf ( input_parm_names , "linux_perf_host" )];
        if ( linux_perf_host == "" )
          {
          linux_perf_host = null;
          linux_perf_host_servers = null;
          n_linux_servers = 0;
          arr_linux_cpu_utilization = null;
          }
        else
          {
          string []str_SplitSemiColons;

          str_SplitSemiColons = linux_perf_host.Split ( ';' );
                    
          n_linux_servers = str_SplitSemiColons.Length;

          linux_unames = new String[n_linux_servers];
          linux_passwd = new String[n_linux_servers];
          linux_perf_host_servers = new String[n_linux_servers];

          i = 0;
          foreach (string splitline in str_SplitSemiColons)
            {
            string []str_SplitColon = new String[3];
            str_SplitColon = splitline.Split ( ':' );
            linux_unames[i] = str_SplitColon[0];
            linux_passwd[i] = str_SplitColon[1];
            linux_perf_host_servers[i] = str_SplitColon[2];
            i++;
            }

          is_Lin_VM = true;
          arr_linux_cpu_utilization = new double[n_linux_servers];        //Used to store CPU utilizations for book keeping

          for ( i = 0 ; i < n_linux_servers ; i++ )
            {
            arr_linux_cpu_utilization[i] = 0.0;
            }
          }
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter linux_perf_host: {0}" , e.Message );
        return;
        }

      //Added by GSK
      try
        {
        detailed_view = input_parm_values[Array.IndexOf ( input_parm_names , "detailed_view" )];
        if ( detailed_view.ToUpper ( ) == "Y" )
            is_detailed_view = true;
        else if ( detailed_view.ToUpper ( ) == "N" )
            is_detailed_view = false;
        else
            throw new System.Exception ( "Wrong value of parameter detailed_view specified!!" );
        }
      catch ( System.Exception e )
        {
        Console.WriteLine ( "Error in converting parameter detailed_view: {0}" , e.Message );
        return;
        }
      try
      {
          pct_newreviews =
            Convert.ToInt32(input_parm_values[Array.IndexOf(input_parm_names, "pct_newreviews")]);
      }
      catch (System.Exception e)
      {
          Console.WriteLine("Error in converting parameter pct_newreviews: {0}", e.Message);
          return;
      }
      try
      {
          n_reviews = Convert.ToInt32(input_parm_values[Array.IndexOf(input_parm_names, "n_reviews")]);
          if (n_reviews <= 0)
          {
              Console.WriteLine("n_reviews must be greater than 0");
              return;
          }
      }
      catch (System.Exception e)
      {
          Console.WriteLine("Error in converting parameter n_reviews: {0}", e.Message);
          return;
      }
      try
      {
          pct_newhelpfulness = Convert.ToInt32(input_parm_values[Array.IndexOf(input_parm_names, "pct_newhelpfulness")]);
      }
      catch (System.Exception e)
      {
          Console.WriteLine("Error in converting parameter pct_newhelpfulness: {0}", e.Message);
          return;
      } 
      try
      {
          pct_newmember = Convert.ToInt32(input_parm_values[Array.IndexOf(input_parm_names, "pct_newmember")]);
      }
      catch (System.Exception e)
      {
          Console.WriteLine("Error in converting parameter pct_newmember: {0}", e.Message);
          return;
      }
      try 
      {
          outfilename = input_parm_values[Array.IndexOf(input_parm_names, "out_filename")];
          if (outfilename == "") 
            { 
              outfilename = null;
            }
          else 
           {
          outfile = new System.IO.StreamWriter(outfilename);
          outfile.WriteLine("et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec, rt_tot_sampled," +
            " n_rollbacks_overall, (100.0 * n_rollbacks_overall) / n_overall" );
           }
      }
        catch (System.Exception e)
      {
          Console.WriteLine("Error in filename given for out_filename: {0}", e.Message);
          return;
      }
      try
      {
          ds2_mode_string = input_parm_values[Array.IndexOf(input_parm_names, "ds2_mode")];
          if (ds2_mode_string == "Y")
          {
              ds2_mode = true;
          }          
      }
      catch (System.Exception e)
      {
          Console.WriteLine("Error in parsing ds2_mode parameter: {0}", e.Message);
          return;
      }
      
      Console.WriteLine ( "target= {0}  n_threads= {1}  ramp_rate= {2}  run_time= {3}  db_size= {4}" +
        "  warmup_time= {5}  think_time= {6} pct_newcustomers= {7} pct_newmembers= {8}  n_searches= {9}  search_batch_size= {10}" +
        "  n_reviews={11} pct_newreviews={12} pct_newhelpfulness={13} n_line_items{14} virt_dir= {15}" +
        "  page_type= {16}  windows_perf_host= {17} detailed_view= {18} linux_perf_host= {19} output_file= {20} ds2_mode= {21}" ,
        target , n_threads , ramp_rate , run_time , db_size , warmup_time , think_time , pct_newcustomers ,
            pct_newmember, n_searches , search_batch_size , n_reviews, pct_newreviews, pct_newhelpfulness,
            n_line_items , virt_dir , page_type , windows_perf_host , detailed_view , linux_perf_host, outfilename, ds2_mode_string );

#if (USE_WIN32_TIMER)
      Console.WriteLine("\nUsing WIN32 QueryPerformanceCounters for measuring response time\n");
#else
      Console.WriteLine ( "\nUsing .NET DateTime for measuring response time\n" );
#endif

      //Changed by GSK
      //max_customer = MAX_CUSTOMER[db_size];
      //max_product = MAX_PRODUCT[db_size];

      max_customer = customer_rows;
      max_product = product_rows;
      max_review = product_rows * 20;

      //Changed by GSK (size of array prod_array = number of rows in product table + (10000 * 10)
      //Reason : Every 10000th product wil be popular and will have 10 entries in list
      //Set up array to choose product ids from, weighted with more entries for popular products
      //Popular products (in this case every 10,000th) will have 10 entries in list, others just 1
      int prod_arr_size = product_rows + 100000;
      prod_array = new int[prod_arr_size];
      i = 0;
      for ( int j = 1 ; j <= max_product ; j++ )
        {
        if ( ( j % 10000 ) == 0 ) for ( int k = 0 ; k < 10 ; k++ ) prod_array[i++] = j;
        else prod_array[i++] = j;
        }
      prod_array_size = i;
      //Console.WriteLine("{0} products in array", prod_array_size);

      for ( i = 0 ; i < GlobalConstants.LAST_N ; i++ ) { rt_tot_lastn[i] = 0.0; }

#if (GEN_PERF_CTRS)      
      if (!PerformanceCounterCategory.Exists("Test")) // Create Performance Counter object if necessary
        {
        CounterCreationDataCollection CCDC = new CounterCreationDataCollection();
        CounterCreationData MaxRT = new CounterCreationData();
        MaxRT.CounterType = PerformanceCounterType.NumberOfItems32;
        MaxRT.CounterName = "MaxRT";
        CCDC.Add(MaxRT);
        CounterCreationData OPM = new CounterCreationData();
        OPM.CounterType = PerformanceCounterType.NumberOfItems32;
        OPM.CounterName = "OPM";
        CCDC.Add(OPM);       
    // For Visual Studio 2003: PerformanceCounterCategory.Create("Test", "DB Stress Data", CCDC);
        PerformanceCounterCategory.Create("Test", "DB Stress Data", PerformanceCounterCategoryType.SingleInstance, CCDC);
        Console.WriteLine("Performance Counter Category Test and Counters MaxRT and OPM created");
        }          
      else
        {
        if ( !( PerformanceCounterCategory.CounterExists("MaxRT", "Test") && 
          PerformanceCounterCategory.CounterExists("OPM", "Test")) )
          { 
          PerformanceCounterCategory.Delete("Test");
          CounterCreationDataCollection CCDC = new CounterCreationDataCollection();
          CounterCreationData MaxRT = new CounterCreationData();
          MaxRT.CounterType = PerformanceCounterType.NumberOfItems32;
          MaxRT.CounterName = "MaxRT";
          CCDC.Add(MaxRT);
          CounterCreationData OPM = new CounterCreationData();
          OPM.CounterType = PerformanceCounterType.NumberOfItems32;
          OPM.CounterName = "OPM";
          CCDC.Add(OPM);       
      // For Visual Studio 2003: PerformanceCounterCategory.Create("Test", "DB Stress Data", CCDC);
          PerformanceCounterCategory.Create("Test", "DB Stress Data", PerformanceCounterCategoryType.SingleInstance, CCDC); 
          Console.WriteLine
            ("Performance Counter Category Test deleted; Category Test and Counters MaxRT/OPM created");
          }
        else
          {
          Console.WriteLine("Performance Counter Category Test and Counter MaxRT exist");
          }
        }
      PerformanceCounter MaxRTC = new PerformanceCounter("Test", "MaxRT", false); // Max response time
      PerformanceCounter OPMC = new PerformanceCounter("Test", "OPM", false); // Orders per minute
      
      // Read CPU Utilization % of target host (if Windows)
      //PerformanceCounter CPU_PCT = null;
      //if (windows_perf_host != null)
        //CPU_PCT = new PerformanceCounter("Processor", "% Processor Time", "_Total", windows_perf_host);
      
      //Changed by GSK
      //Need an array of PerfCounter Class objects to capture Processor Time for each Machine

      PerformanceCounter[] CPU_PCT = new PerformanceCounter[n_windows_servers];
      if (windows_perf_host != null)
        {           
        //Create PerfMon counter on Each target machine

        for ( i = 0 ; i < n_windows_servers ; i++)
          {
          CPU_PCT[i] = new PerformanceCounter("Processor", "% Processor Time", "_Total", windows_perf_host_servers[i]);
          }            
        }
        
      
#else
            Console.WriteLine ( "Not generating Windows Performance Monitor Counters" );
#endif

      //for ( i = 0 ; i < n_threads ; i++ ) // Create User objects; associate each with new Thread running Emulate method
      //    {
      //    users[i] = new User ( i );
      //    threads[i] = new Thread ( new ThreadStart ( users[i].Emulate ) );
      //    }

           
                   
      for ( i = 0 , server_id = 0 ; i < n_threads ; i++ ) // Create User objects; associate each with new Thread running Emulate method
        {
        if ( server_id < n_target_servers )
          {
          users[i] = new User ( i , server_id );
          threads[i] = new Thread ( new ThreadStart ( users[i].Emulate ) );
          server_id++;
          }
        else if ( server_id == n_target_servers )
          {
          server_id = 0;
          users[i] = new User ( i , server_id );
          threads[i] = new Thread ( new ThreadStart ( users[i].Emulate ) );
          server_id++;
          }
        }

      //Added by GSK
      //Before each thread will try to connect to remote systems and then running the loop to start the warmup and then actual run
      //We will plink all linux targets if there are any
      //this will ensure each target is registered in registry of machine on which driver program runs
      //This will avoid giving any add RSA fingerprint message when actual run stats are getting printed out
      // 
      if (linux_perf_host != null)     //Added by GSK for getting Linux CPU Utilization
        {
        for (i = 0; i < n_linux_servers; i++)
          {
          try
            {
            RegisterRSAHostKey(linux_perf_host_servers[i].ToString(), linux_unames[i].ToString(), linux_passwd[i].ToString());
            }
          catch (System.Exception e)
            {
            Console.WriteLine("Error in adding RSA fingerprint for target linux host: {0}: {1}", linux_perf_host_servers[i].ToString(), e.Message);
            return;
            }
          }
        Console.WriteLine(" ");
        }


      for ( i = 0 ; i < n_threads ; i++ ) // Start threads
        {
        threads[i].Start ( );
        }

      while ( n_threads_running < n_threads ) // Wait for all threads to start
        {
        //Console.WriteLine("Controller: n_threads_running = {0}", n_threads_running);
        //Console.WriteLine("Controller: Thread status:");
        //for (i=0; i<n_threads; i++) Console.WriteLine("  Thread {0}: {1}", i, threads[i].ThreadState);
        Thread.Sleep ( 1000 );
        }
      Console.WriteLine ( "Controller ({0}): all threads running" , DateTime.Now );
      //for (i=0; i<n_threads; i++) Console.WriteLine("  Thread {0}: {1}", i, threads[i].ThreadState);   

      int ConnectTimeout = 60;  // Used to limit the amount of time that driver program will try to get all threads conencted
      while ( (n_threads_connected < n_threads) && (ConnectTimeout > 0) )   
        {
        for ( int j = 0 ; j < n_threads ; j++ )  // If one of the threads has stopped quit
          if ( threads[j].ThreadState == System.Threading.ThreadState.Stopped ) return;
        Console.WriteLine ( "Controller: n_threads_connected = {0} : ConnectionTimeOut remaining {1}" , n_threads_connected,ConnectTimeout );
        Thread.Sleep ( 1000 );
        --ConnectTimeout;
        }
      
      if (n_threads_connected < n_threads)   // If all threads are not connected, then timeout was exceeded
        { 
        Console.WriteLine ( "Controller: ConnectTimeout reached : could not connect all threads, Aborting...");
        Thread.Sleep ( 500 );
        for ( i = 0 ; i < n_threads ; i++ )
          {
              threads[i].Abort();
          }
        return;
        }
      
      Console.WriteLine ( "Controller ({0}): all threads connected - issuing Start" , DateTime.Now );
      Start = true;

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif

      if ( run_time == 0 ) run_time = 1000000;  // test run time in minutes, 0 => forever
      run_time += warmup_time;  // Add warmup time for total run time
            
      for ( i_sec = 1 ; i_sec <= run_time * 60 ; i_sec++ ) // run for run_time*60 seconds
        {          
        //Call plink to execute mpstat on remote linux machine to store CPU data in File on remote system
        if (i_sec % 10 == 1)  //At start of every 10 second interval, start background process for mpstat CPU monitoring on each linux machine
          {
          if (linux_perf_host != null)     //Added by GSK for getting Linux CPU Utilization
            {
            for (i = 0; i < n_linux_servers; i++)
              {
              try
                {
                RunBackGroundmpStat(linux_perf_host_servers[i].ToString(), linux_unames[i].ToString(), linux_passwd[i].ToString());
                }
              catch (System.Exception e)
                {
                Console.WriteLine("Error in getting CPU Utilization for host: {0}: {1}", linux_perf_host_servers[i].ToString(), e.Message);
                return;
                }
              }
            }
          }

        Thread.Sleep ( 1000 );     // Update perfmon stats about every second
        Monitor.Enter ( UpdateLock );  // Block User threads from accessing code to update these values (below)       
#if (USE_WIN32_TIMER)
          QueryPerformanceCounter(ref ctr);
          et = (ctr-ctr0)/(double) freq;   
#else
        TS = DateTime.Now - DT0;
        et = TS.TotalSeconds;
#endif          
      
        //opm, rt_tot_lastn_max_msec will maintain overall runtime stats for all threads that connect to DB Servers on multiple VM's
        opm = ( int ) Math.Floor ( 60.0 * n_overall / et );
        rt_tot_lastn_max = 0.0;
        for ( int j = 0 ; j < GlobalConstants.LAST_N ; j++ )
            rt_tot_lastn_max = ( rt_tot_lastn[j] > rt_tot_lastn_max ) ? rt_tot_lastn[j] : rt_tot_lastn_max;
        rt_tot_lastn_max_msec = ( int ) Math.Floor ( 1000 * rt_tot_lastn_max );
           
        //Following code will maintain runtime stats for threads that connect to DB Servers on individual VM's
        for ( i = 0 ; i < n_target_servers ; i++ )
          {
          arr_opm[i] = ( int ) Math.Floor ( 60.0 * arr_n_overall[i] / et );
          arr_rt_tot_lastn_max = 0.0;
          for ( int m = 0 ; m < GlobalConstants.LAST_N ; m++ )
            {
            arr_rt_tot_lastn_max = ( arr_rt_tot_lastn[i , m] > arr_rt_tot_lastn_max ) ? arr_rt_tot_lastn[i , m] : arr_rt_tot_lastn_max;
            }
          arr_rt_tot_lastn_max_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_tot_lastn_max );
          }
                
                
#if (GEN_PERF_CTRS)  
          MaxRTC.RawValue = rt_tot_lastn_max_msec;
          OPMC.RawValue = opm;
          //Changed by GSK
          if (windows_perf_host != null)
            {
            //cpu_pct_tot += CPU_PCT.NextValue();
            //++n_cpu_pct_samples;

                for ( i = 0 ; i < n_windows_servers ; i++ )
                {
                    arr_cpu_pct_tot[i] += CPU_PCT[i].NextValue();
                    ++arr_n_cpu_pct_samples[i];
                }
            }
#endif               
                

        if ( i_sec % 10 == 0 ) // print out stats every 10 seconds
          {
          //rt_login_avg_msec = (int) Math.Floor(1000*rt_login_overall/n_login_overall);
          //rt_newcust_avg_msec = (int) Math.Floor(1000*rt_newcust_overall/n_newcust_overall);
          //rt_browse_avg_msec = (int) Math.Floor(1000*rt_browse_overall/n_browse_overall);
          //rt_purchase_avg_msec = (int) Math.Floor(1000*rt_purchase_overall/n_purchase_overall);
          // rt_tot_avg_msec = ( int ) Math.Floor ( 1000 * rt_tot_overall / n_overall );

          // Modified by DJ 11/28/2016 to handle n_overall = 0
    	  if (n_overall > 0)
            {
            rt_tot_avg_msec = ( int ) Math.Floor ( 1000 * rt_tot_overall / n_overall );
	        }
          else
            {
            rt_tot_avg_msec = 0;
	        }


          //Added on 8/8/2010
          diff_n_overall = Math.Abs(n_overall - old_n_overall);
          old_n_overall = n_overall;
          diff_rt_tot_overall = Math.Abs(rt_tot_overall - old_rt_tot_overall);                    
          old_rt_tot_overall = rt_tot_overall;

          if (diff_n_overall > 0)
            {
            rt_tot_sampled = (int) Math.Floor(1000 * diff_rt_tot_overall / diff_n_overall);
    	    }
          else
            {
            rt_tot_sampled = 0;
	        }
 
	      if (n_overall > 0)
            {
            pct_rollbacks = (100.0 * n_rollbacks_overall) / n_overall;
	        }
          else
            {
            pct_rollbacks = 0.0;
	        }

          //Console.Error.Write ( "\n" );      
          //Console.WriteLine("et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max_msec={3} rt_tot_avg_msec={4} " +
          //  "rollbacks: n={5} %={6,5:F1}  ", et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec, n_rollbacks_overall,
          //  (100.0 * n_rollbacks_overall) / n_overall);
          //Changed on 8/8/2010
          Console.WriteLine("et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max_msec={3} rt_tot_avg_msec={4} " +
            "rt_tot_sampled={5} " +
            "rollbacks: n={6} %={7,5:F1} ", et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec,
            rt_tot_sampled,
            n_rollbacks_overall,pct_rollbacks                      
            );
            if (outfilename != null)
              {
               outfile.WriteLine("{0,7:F1},{1},{2},{3},{4},{5},{6},{7,5:F1}", et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec,
              rt_tot_sampled, n_rollbacks_overall, pct_rollbacks); 
              }

          total_cpu_utilzn = 0.0;
          total_lin_cpu_utilzn = 0.0;
          total_win_cpu_utilzn = 0.0;
          if ( windows_perf_host != null )
            {
            //Changed by GSK to get total average cpu utilization                                                
            for ( i = 0 ; i < n_windows_servers ; i++ )
              {
              total_win_cpu_utilzn += ( arr_cpu_pct_tot[i] / arr_n_cpu_pct_samples[i] );
              }                        
            }                     
          if ( linux_perf_host != null )     //Added by GSK for getting Linux CPU Utilization
            {                        
            for ( i = 0 ; i < n_linux_servers ; i++ )
              {
              try
                {
                //Call plink to Read mpstat data in a text file on remote linux machine to give CPU data
                //Store CPU utilization for each linux target for bookkeeping                    
                arr_linux_cpu_utilization[i] = ReadRemoteTextFile(linux_perf_host_servers[i].ToString(), linux_unames[i].ToString(), linux_passwd[i].ToString());
                total_lin_cpu_utilzn += arr_linux_cpu_utilization[i];
                }
              catch(System.Exception e)
                {
                Console.WriteLine ( "Error in getting CPU Utilization for host: {0}: {1}" , linux_perf_host_servers[i].ToString ( ) , e.Message);
                return;
                }
              }                        
            }

          if ( is_Win_VM == true && is_Lin_VM == true )       //Get perf stats from both linux and windows machines                        
            {
            total_cpu_utilzn = total_win_cpu_utilzn + total_lin_cpu_utilzn;
            //Instead of getting Sum of cpu utilization of all machines, we take average of total since it is good indication of utilization of Physical Processor
            total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
            StringBuilder sb_linux = new StringBuilder();
            for ( z= 0 ; z < n_linux_servers ; z++ )
              {
              sb_linux.Append ( linux_perf_host_servers[z] ).Append ( ";" );
              }
            Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host + ";" + sb_linux.ToString() , total_cpu_utilzn );
            }
          else if ( is_Win_VM == true && is_Lin_VM == false )  //Get perf stats from windows machines                        
            {
            total_cpu_utilzn = total_win_cpu_utilzn;
                        
            total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
            Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host  , total_cpu_utilzn );
            }
          else if ( is_Lin_VM == true && is_Win_VM == false )  //Get perf stats from linux machines                        
            {
            total_cpu_utilzn = total_lin_cpu_utilzn;
                        
            total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
            StringBuilder sb_linux = new StringBuilder ( );
            for ( z = 0 ; z < n_linux_servers ; z++ )
              {
              sb_linux.Append ( linux_perf_host_servers[z] ).Append ( ";" );
              }
            Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , sb_linux.ToString() , total_cpu_utilzn );
            }
          else
            {
                // Console.Error.Write ( "\n" );
            }
                  

          //Added by GSK
          //Call Write individual stats only when detailed_view parameter is YES and more than one target servers                   
          if ( is_detailed_view == true && n_target_servers > 1)
            {
            Console.WriteLine ( "\nIndividual Stats for each DB / Web Server: " );
            for ( i = 0 ; i < n_target_servers ; i++ )
              {
              arr_rt_tot_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_tot_overall[i] / arr_n_overall[i] );

              //Added on 8/8/2010
              arr_diff_n_overall[i] = Math.Abs(arr_n_overall[i] - arr_old_n_overall[i]);
              arr_old_n_overall[i] = arr_n_overall[i];
              arr_diff_rt_tot_overall[i] = Math.Abs(arr_rt_tot_overall[i] - arr_old_rt_tot_overall[i]);
              arr_old_rt_tot_overall[i] = arr_rt_tot_overall[i];
              arr_rt_tot_sampled[i] = (int)Math.Floor(1000 * arr_diff_rt_tot_overall[i] / arr_diff_n_overall[i]);

              //Console.WriteLine("et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max_msec={3} rt_tot_avg_msec={4} " +
              //  "rollbacks: n={5} %={6,5:F1}  ",
              //  et, arr_n_overall[i], arr_opm[i], arr_rt_tot_lastn_max_msec[i], arr_rt_tot_avg_msec[i], arr_n_rollbacks_overall[i],
              //  (100.0 * arr_n_rollbacks_overall[i]) / arr_n_overall[i]);
              //Changed on 8/8/2010
              Console.WriteLine("et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max_msec={3} rt_tot_avg_msec={4} " +
                "rt_tot_sampled={5} " +
                "rollbacks: n={6} %={7,5:F1} ",
                et, arr_n_overall[i], arr_opm[i], arr_rt_tot_lastn_max_msec[i], arr_rt_tot_avg_msec[i], arr_rt_tot_sampled[i],
                arr_n_rollbacks_overall[i],(100.0 * arr_n_rollbacks_overall[i]) / arr_n_overall[i]                              
                );
                            

              //Added by GSK
              //Following condition i < n_windows_servers ensure that stats for windows VM's will be outputted first and then linux VM's
              //For this to work, target parameter should always specify all windows targets first followed by linux targets (all targets selerated by semi colon ;)
              if ( windows_perf_host != null && i < n_windows_servers )
                  {
                  //Need individual CPU utilization of Virtual Machines on which DB / Web Servers are running
                  Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host_servers[i] , arr_cpu_pct_tot[i] / arr_n_cpu_pct_samples[i] );
                  }
              if(linux_perf_host != null && i >= n_windows_servers)
                {
                try
                  {
                  //We only get CPU Utilization data which is book keeped in array arr_linux_cpu_utilization above
                  Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , linux_perf_host_servers[i - n_windows_servers] ,
                      arr_linux_cpu_utilization[i - n_windows_servers] );
                  }
                catch ( System.Exception e )
                  {
                  Console.WriteLine ( "Error in getting CPU Utilization for host: {0}: {1}" , linux_perf_host_servers[i - n_windows_servers].ToString ( ) , e.Message );
                  return;
                  }
                }
              else Console.Error.Write ( "\n" );
              }                        
            }
      //Till this point Added by GSK                    

          for ( int j = 0 ; j < n_threads ; j++ )
            {
            if ( threads[j].ThreadState == System.Threading.ThreadState.Stopped )
              {
              Console.WriteLine ( "threads[{0}].ThreadState= {1}" , j , threads[j].ThreadState );
              }
            }

          }

        Monitor.Exit ( UpdateLock );

        if ( i_sec == 60 * warmup_time ) // reset params after specified warmump
          {
          n_overall = 0; n_login_overall = 0; n_newcust_overall = 0; n_browse_overall = 0; n_purchase_overall = 0;
          n_rollbacks_overall = 0;
          rt_tot_overall = 0.0; rt_login_overall = 0.0; rt_newcust_overall = 0.0; rt_browse_overall = 0.0;
          rt_purchase_overall = 0.0;
          for ( int j = 0 ; j < GlobalConstants.LAST_N ; j++ ) rt_tot_lastn[j] = 0.0;
          cpu_pct_tot = 0.0;
          n_cpu_pct_samples = 0;

          //Added on 8/8/2010
          old_n_overall= 0;
          diff_n_overall= 0;
          old_rt_tot_overall= 0.0;
          diff_rt_tot_overall= 0.0;
          rt_tot_sampled = 0;

          //Added by GSK
          for ( i = 0 ; i < n_target_servers ; i++ )
            {
            arr_n_overall[i] = 0; 
            arr_n_login_overall[i] = 0; 
            arr_n_newcust_overall[i] = 0;
            arr_n_browse_overall[i] = 0; 
            arr_n_purchase_overall[i] = 0;
            arr_n_rollbacks_overall[i] = 0;
            arr_rt_tot_overall[i] = 0.0; 
            arr_rt_login_overall[i] = 0.0; 
            arr_rt_newcust_overall[i] = 0.0; 
            arr_rt_browse_overall[i] = 0.0;
            arr_rt_purchase_overall[i] = 0.0;

            //Added on 3/17/2015
            arr_n_reviewbrowse_overall[i] = 0;
            arr_n_newreview_overall[i] = 0;
            arr_n_newhelpfulness_overall[i] = 0;
            arr_n_newmember_overall[i] = 0;
            arr_rt_reviewbrowse_overall[i] = 0.0;
            arr_rt_newreview_overall[i] = 0.0;
            arr_rt_newhelpfulness_overall[i] = 0.0;
            arr_rt_newmember_overall[i] = 0.0;


            //Added on 8/8/2010
            arr_old_n_overall[i] = 0;
            arr_diff_n_overall[i] = 0;
            arr_old_rt_tot_overall[i] = 0.0;
            arr_diff_rt_tot_overall[i] = 0.0;
            arr_rt_tot_sampled[i] = 0;

            for ( int n = 0 ; n < GlobalConstants.LAST_N ; n++ ) arr_rt_tot_lastn[i,n] = 0.0;
            
            cpu_pct_tot = 0.0;
            n_cpu_pct_samples = 0;
            }

          for ( i = 0 ; i < n_windows_servers ; i++ )
            {
            arr_n_cpu_pct_samples[i] = 0;
            arr_cpu_pct_tot[i] = 0.0;
            }

          for ( i = 0 ; i < n_linux_servers ; i++ )
            {
            arr_linux_cpu_utilization[i] = 0.0;
            }
          //Till this point Added by GSK

#if (USE_WIN32_TIMER)
          QueryPerformanceCounter(ref ctr0);   
#else
          DT0 = DateTime.Now;
#endif

          Console.WriteLine ( "Stats reset" );
          }
        } // End for i_sec<run_time

      Monitor.Enter ( UpdateLock );  // Block User threads from accessing code to update these values (below)
#if (USE_WIN32_TIMER)
        QueryPerformanceCounter(ref ctr);
        et = (ctr-ctr0)/(double) freq;   
#else
      TS = DateTime.Now - DT0;
      et = TS.TotalSeconds;
#endif
            
      //Variables below will maintain Aggregate Final stats data for all DB servers running on all VM's
      opm = ( int ) Math.Floor ( 60.0 * n_overall / et );
      rt_login_avg_msec = ( int ) Math.Floor ( 1000 * rt_login_overall / n_login_overall );
      rt_newcust_avg_msec = ( int ) Math.Floor ( 1000 * rt_newcust_overall / n_newcust_overall );
      rt_browse_avg_msec = ( int ) Math.Floor ( 1000 * rt_browse_overall / n_browse_overall );
      rt_reviewbrowse_avg_msec = ( int ) Math.Floor ( 1000 * rt_reviewbrowse_overall / n_reviewbrowse_overall );
      rt_newreview_avg_msec = ( int ) Math.Floor ( 1000 * rt_newreview_overall / n_newreview_overall );
      rt_newhelpfulness_avg_msec = ( int ) Math.Floor ( 1000 * rt_newhelpfulness_overall / n_newhelpfulness_overall );
      rt_newmember_avg_msec = (int)Math.Floor(1000 * rt_newmember_overall / n_newmember_overall);
      rt_purchase_avg_msec = ( int ) Math.Floor ( 1000 * rt_purchase_overall / n_purchase_overall );
      rt_tot_lastn_max = 0.0;
      for ( int j = 0 ; j < GlobalConstants.LAST_N ; j++ )
        rt_tot_lastn_max = ( rt_tot_lastn[j] > rt_tot_lastn_max ) ? rt_tot_lastn[j] : rt_tot_lastn_max;
      rt_tot_lastn_max_msec = ( int ) Math.Floor ( 1000 * rt_tot_lastn_max );
      rt_tot_avg_msec = ( int ) Math.Floor ( 1000 * rt_tot_overall / n_overall );

      //Added by GSK
      //Variables/ Arrays below will maintain individual stats data for each DB Server running on each VM
      for ( i = 0 ; i < n_target_servers ; i++ )
        {
        arr_opm[i] = ( int ) Math.Floor ( 60.0 * arr_n_overall[i] / et );
        arr_rt_login_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_login_overall[i] / arr_n_login_overall[i] );
        arr_rt_newcust_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_newcust_overall[i] / arr_n_newcust_overall[i] );
        arr_rt_browse_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_browse_overall[i] / arr_n_browse_overall[i] );
        arr_rt_reviewbrowse_avg_msec[i] = (int)Math.Floor(1000 * arr_rt_reviewbrowse_overall[i] / arr_n_reviewbrowse_overall[i]);
        arr_rt_newreview_avg_msec[i] = (int)Math.Floor(1000 * arr_rt_newreview_overall[i] / arr_n_newreview_overall[i]);
        arr_rt_newhelpfulness_avg_msec[i] = (int)Math.Floor(1000 * arr_rt_newhelpfulness_overall[i] / arr_n_newhelpfulness_overall[i]);
        arr_rt_newmember_avg_msec[i] = (int)Math.Floor(1000 * arr_rt_newmember_overall[i] / arr_n_newmember_overall[i]);
        arr_rt_purchase_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_purchase_overall[i] / arr_n_purchase_overall[i] );
        arr_rt_tot_lastn_max = 0.0;
        for ( int p = 0 ; p < GlobalConstants.LAST_N ; p++ )
          arr_rt_tot_lastn_max = ( arr_rt_tot_lastn[i , p] > arr_rt_tot_lastn_max ) ? arr_rt_tot_lastn[i , p] : arr_rt_tot_lastn_max;
        arr_rt_tot_lastn_max_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_tot_lastn_max );
        arr_rt_tot_avg_msec[i] = ( int ) Math.Floor ( 1000 * arr_rt_tot_overall[i] / arr_n_overall[i] );
        }
      //Till this point Added by GSK

#if (GEN_PERF_CTRS)  
        MaxRTC.RawValue = rt_tot_lastn_max_msec;
        OPMC.RawValue = opm;
#endif
      //Console.WriteLine("\nFinal: et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max={3} rt_tot_avg={4} " +
      //  "n_login_overall={5} n_newcust_overall={6} n_browse_overall={7} n_purchase_overall={8} " +
      //  "rt_login_avg_msec={9} rt_newcust_avg_msec={10} rt_browse_avg_msec={11} rt_purchase_avg_msec={12} " +
      //  "n_rollbacks_overall={13} rollback_rate = {14,5:F1}%  ",
      //  et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec, n_login_overall, n_newcust_overall,
      //  n_browse_overall, n_purchase_overall, rt_login_avg_msec, rt_newcust_avg_msec, rt_browse_avg_msec,
      //  rt_purchase_avg_msec, n_rollbacks_overall, (100.0 * n_rollbacks_overall) / n_overall);
      //Changed on 8/8/2010
      // Changed again on 3/17/2015
      Console.WriteLine("\nFinal ({0}): et={1,7:F1} n_overall={2} opm={3} rt_tot_lastn_max={4} rt_tot_avg={5} " +
        "n_login_overall={6} n_newcust_overall={7} n_newmember_overall={8} n_browse_overall={9} " +
        "n_reviewbrowse={10} n_newreviews={11} n_newhelpfulness={12} n_purchase_overall={13} " +
        "rt_login_avg_msec={14} rt_newcust_avg_msec={15} rt_rewmember_avg_msec={16} rt_browse_avg_msec={17} " +
        "rt_reviewbrowse_avg_msec={18} rt_newreview_avg_msec={19} rt_newhelpfulness={20} rt_purchase_avg_msec={21} " +
        "rt_tot_sampled={22} n_rollbacks_overall={23} rollback_rate = {24,5:F1}%",
        DateTime.Now, et, n_overall, opm, rt_tot_lastn_max_msec, rt_tot_avg_msec, n_login_overall, n_newcust_overall, n_newmember_overall,
        n_browse_overall, n_reviewbrowse_overall, n_newreview_overall, n_newhelpfulness_overall,  n_purchase_overall, 
        rt_login_avg_msec, rt_newcust_avg_msec, rt_newmember_avg_msec, rt_browse_avg_msec, rt_reviewbrowse_avg_msec, rt_newreview_avg_msec, 
        rt_newhelpfulness_avg_msec, rt_purchase_avg_msec, rt_tot_sampled, n_rollbacks_overall, (100.0 * n_rollbacks_overall) / n_overall);

      if (outfilename != null)
          outfile.Close();

      total_cpu_utilzn = 0.0;
      total_win_cpu_utilzn = 0.0;
      total_lin_cpu_utilzn = 0.0;

      if ( windows_perf_host != null )
        {
        //Changed by GSK to get total average cpu utilization                                        
        for ( i = 0 ; i < n_windows_servers ; i++ )
          {
          total_win_cpu_utilzn += ( arr_cpu_pct_tot[i] / arr_n_cpu_pct_samples[i] );
          }                
        }            
      if ( linux_perf_host != null )     //Added by GSK for getting Linux CPU Utilization
        {                
        for ( i = 0 ; i < n_linux_servers ; i++ )
          {
          try
              {
              //Use bookkeeped CPU utilization 
              total_lin_cpu_utilzn += arr_linux_cpu_utilization[i];
              }
          catch ( System.Exception e )
              {
              Console.WriteLine ( "Error in getting CPU Utilization for host: {0}: {1}" , linux_perf_host_servers[i].ToString ( ) , e.Message );
              return;
              }
            }                
          }

      if ( is_Win_VM == true && is_Lin_VM == true )       //Get perf stats from both linux and windows machines                        
        {
        total_cpu_utilzn = total_win_cpu_utilzn + total_lin_cpu_utilzn;
        //Instead of getting Sum of cpu utilization of all machines, we take average of total since it is good indication of utilization of Physical Processor
        total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
        StringBuilder sb_linux = new StringBuilder ( );
        for ( z = 0 ; z < n_linux_servers ; z++ )
          {
          sb_linux.Append ( linux_perf_host_servers[z] ).Append ( ";" );
          }
        Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host + ";" + sb_linux.ToString() , total_cpu_utilzn );
        }
      else if ( is_Win_VM == true && is_Lin_VM == false )  //Get perf stats from windows machines                        
        {
        total_cpu_utilzn = total_win_cpu_utilzn;
                
        total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
        Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host , total_cpu_utilzn );
        }
      else if ( is_Lin_VM == true && is_Win_VM == false )  //Get perf stats from linux machines                        
        {
        total_cpu_utilzn = total_lin_cpu_utilzn;
                
        total_cpu_utilzn = total_cpu_utilzn / n_target_servers;
        StringBuilder sb_linux = new StringBuilder ( );
        for ( z = 0 ; z < n_linux_servers ; z++ )
          {
          sb_linux.Append ( linux_perf_host_servers[z] ).Append ( ";" );
          }
        Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , sb_linux.ToString() , total_cpu_utilzn );
        }
      else
        {
        Console.Error.Write ( "\n" );
        }           

      //Added by GSK
      //Call Write individual stats only when there are more than one target servers                   
      if ( n_target_servers > 1 )
        {
        Console.WriteLine ( "\nIndividual Stats for each DB / Web Server: " );
        for ( i = 0 ; i < n_target_servers ; i++ )
          {
          //Console.Error.Write ( "\nFinal: et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max={3} rt_tot_avg={4} " +
          //  "n_login_overall={5} n_newcust_overall={6} n_browse_overall={7} n_purchase_overall={8} " +
          //  "rt_login_avg_msec={9} rt_newcust_avg_msec={10} rt_browse_avg_msec={11} rt_purchase_avg_msec={12} " +
          //  "n_rollbacks_overall={13} rollback_rate = {14,5:F1}% " ,
          //  et , arr_n_overall[i] , arr_opm[i] , arr_rt_tot_lastn_max_msec[i] , arr_rt_tot_avg_msec[i] , arr_n_login_overall[i] , arr_n_newcust_overall[i] ,
          //  arr_n_browse_overall[i] , arr_n_purchase_overall[i] , arr_rt_login_avg_msec[i] , arr_rt_newcust_avg_msec[i] , arr_rt_browse_avg_msec[i] ,
          //  arr_rt_purchase_avg_msec[i] , arr_n_rollbacks_overall[i] , ( 100.0 * arr_n_rollbacks_overall[i] ) / arr_n_overall[i]);
          //Changed on 8/8/2010
          // Changed again on 3/17/2015
          Console.WriteLine("Final: et={0,7:F1} n_overall={1} opm={2} rt_tot_lastn_max={3} rt_tot_avg={4} " +
            "n_login_overall={5} n_newcust_overall={6} n_newmember_overall={7} n_browse_overall={8} " +
            "n_reviewbrowse_overall={9} n_newreview_overall={10} n_newhelpfulnes_overall={11} n_purchase_overall={12} " +
            "rt_login_avg_msec={13} rt_newcust_avg_msec={14} rt_newmember_avg_msec={15} rt_browse_avg_msec={16} " +
            "rt_reviewbrowse_avg_msec={17} rt_newreview_avg_msec={18} rt_newhelpfulness_avg_msec={19} rt_purchase_avg_msec={20} " +
            "rt_tot_sampled={21} n_rollbacks_overall={22} rollback_rate = {23,5:F1}%  ",
            et, arr_n_overall[i], arr_opm[i], arr_rt_tot_lastn_max_msec[i], arr_rt_tot_avg_msec[i], arr_n_login_overall[i], arr_n_newcust_overall[i],
            arr_n_newmember_overall[i], arr_n_browse_overall[i], arr_n_reviewbrowse_overall[i], arr_n_newreview_overall[i], arr_n_newhelpfulness_overall[i], 
            arr_n_purchase_overall[i], arr_rt_login_avg_msec[i], arr_rt_newcust_avg_msec[i], arr_rt_newmember_avg_msec[i], arr_rt_browse_avg_msec[i],
            arr_rt_reviewbrowse_avg_msec[i], arr_rt_newreview_avg_msec[i], arr_rt_newhelpfulness_avg_msec[i], arr_rt_purchase_avg_msec[i], arr_rt_tot_sampled[i], 
            arr_n_rollbacks_overall[i], (100.0 * arr_n_rollbacks_overall[i]) / arr_n_overall[i]                      
            );

          //Added by GSK
          //Following condition i < n_windows_servers ensure that stats for windows VM's will be outputted first and then linux VM's
          //For this to work, target parameter should always specify all windows targets first followed by linux targets (all targets selerated by semi colon ;)
          if ( windows_perf_host != null && i < n_windows_servers )
            {
            //Need individual CPU utilization for Virtual Machines on which DB/ Web Servers are running
            Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , windows_perf_host_servers[i] , arr_cpu_pct_tot[i] / arr_n_cpu_pct_samples[i] );
            }
          else if ( linux_perf_host != null && i >= n_windows_servers )     //Added by GSK for getting CPU Utilization of Linux Systems
            {          
            try
              {
              //We only get CPU Utilization data which is book keeped in array arr_linux_cpu_utilization above
              Console.WriteLine ( "host {0} CPU%= {1,5:F1}" , linux_perf_host_servers[i - n_windows_servers] ,
                  arr_linux_cpu_utilization[i - n_windows_servers]);
              }
            catch ( System.Exception e )
              {
              Console.WriteLine ( "Error in getting CPU Utilization for host: {0}: {1}" , linux_perf_host_servers[i - n_windows_servers].ToString ( ) , e.Message );
              return;
              }
            }
          else Console.Error.Write ( "\n" );
          }
        }                      
      //Till this point Added by GSK

      Monitor.Exit ( UpdateLock );

      // Signal threads to end, wait for 'em to stop
      End = true;
      bool all_stopped;
      do
        {
        Thread.Sleep ( 500 );
        all_stopped = true;
        for ( i = 0 ; i < n_threads ; i++ ) all_stopped &= ( threads[i].ThreadState == System.Threading.ThreadState.Stopped );
        }
      while ( !all_stopped );
      Console.WriteLine ( "Controller ({0}): all threads stopped, exiting", DateTime.Now);
      Console.WriteLine ( "n_purchase_from_start= {0} n_rollbacks_from_start= {1}" , n_purchase_from_start , n_rollbacks_from_start );

      //Added by GSK
      //Call Write individual stats only when there are more than one target servers                   
      if ( n_target_servers > 1 )
        {
        Console.WriteLine ( "\nIndividual Stats for each DB / Web Server: " );
        for ( i = 0 ; i < n_target_servers ; i++ )
          Console.WriteLine ( "n_purchase_from_start= {0} n_rollbacks_from_start= {1}", 
            arr_n_purchase_from_start[i] , arr_n_rollbacks_from_start[i] );
        }            
      //Till this point Added by GSK

  Console.WriteLine ( "Run over" );
#if (GEN_PERF_CTRS)  
      MaxRTC.RawValue = 0;
      OPMC.RawValue = 0;
#endif
      } // End of Controller() Constructor
    //
    //-------------------------------------------------------------------------------------------------
    //      
    static int parse_args ( string[] argstring , ref string errmsg )
      {
      int parm_idx = -1 , parm_count = 0;
      string[] split = null;
      string config_fname = null , parmline = null;
      char[] delimeter = { '=' };

      for ( int i = 0 ; i < argstring.Length ; i++ )
        {
        //Console.WriteLine(argstring[i]);
        if ( ( argstring[i].StartsWith ( "--" ) ) && ( argstring[i].IndexOf ( '=' ) > 2 ) )
          {
          split = argstring[i].Substring ( 2 ).Split ( delimeter );
          if ( split[0] == "config_file" )
            {
            config_fname = split[1];
            if ( File.Exists ( config_fname ) )
              {
              StreamReader sr = new StreamReader ( config_fname );
              while ( ( parmline = sr.ReadLine ( ) ) != null )
                {
                //Console.WriteLine(parmline);            
                if ( parmline.IndexOf ( '=' ) > 0 )
                  {
                  split = parmline.Split ( delimeter );
                  parm_idx = Array.IndexOf ( input_parm_names , split[0] );
                  if ( parm_idx > -1 )
                    {
                    //Console.WriteLine("Parameter {0} parsed; was {1}, now {2}", 
                    //  split[0], input_parm_values[parm_idx], split[1]);
                    input_parm_values[parm_idx] = split[1];
                    ++parm_count;
                    }
                  else
                    {
                    errmsg = "Parameter " + split[0] + " doesn't exist";
                    return ( 0 );
                    }
                  }
                else
                  {
                  errmsg = "Incorrect format in parameter: " + argstring[i];
                  return ( 0 );
                  }
                } // End while((parmline = sr.ReadLine()) != null)
              sr.Close ( );
              }
            else
              {
              errmsg = "File " + split[1] + " doesn't exist";
              return ( 0 );
              }
            }  // End if (split[0] == "config_file")
          else  // Param is not a config file name
            {
            parm_idx = Array.IndexOf ( input_parm_names , split[0] );
            if ( parm_idx > -1 )
              {
              //Console.WriteLine("Parameter {0} parsed; was {1}, now {2}", 
              //  split[0], input_parm_values[parm_idx], split[1]);
              input_parm_values[parm_idx] = split[1];
              ++parm_count;
              }
            else
              {
              errmsg = "Parameter " + split[0] + " doesn't exist";
              return ( 0 );
              }
            } // End else Param is not a config file name       
          } // End if ((argstring[i].StartsWith("--") ...
        else
          {
          errmsg = "Incorrect format in parameter: " + argstring[i];
          return ( 0 );
          }
        } // End for (int i=0; i<argstring.Length; i++)
      return ( parm_count );
      } // End of parse_args

    } // End of class Controller
  //
  //-------------------------------------------------------------------------------------------------
  //    
  class User
    {
    // If compile option /d:USE_WIN32_TIMER is specified will use 64b QueryPerformance counter from Win32
    // Else will use .NET DateTime class      
#if (USE_WIN32_TIMER)
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceCounter(ref long x);
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceFrequency(ref long x);  
#endif

    int Userid;
    ds2Interface[] ds2interfaces = new ds2Interface[GlobalConstants.MAX_USERS];
    Random r;
    string username_in , password_in , firstname_in , lastname_in , address1_in , address2_in , city_in , state_in;
    string zip_in , country_in , email_in , phone_in , creditcard_in , gender_in;
    int creditcardtype_in , ccexpmon_in , ccexpyr_in , income_in , age_in;
    int customerid_in, membershiplevel_in, reviewid_in, reviewhelpfulness_in;
    string actor_in , title_in;
    string[] actornames_in, titlenames_in;
    string new_review_summary_in, new_review_text_in;
    int new_review_stars_in, new_review_prod_id_in;
    string[] review_data_terms;

    public int target_server_id = 0;   //Added by GSK (Need this public since it is used by Controller to find out which thread belongs to which DB/Web Server)

    public User ( int userid )
      {
      Userid = userid;
      //Console.WriteLine("user {0} created", userid);
      }

    //Added by GSK Overloaded constructor which will take care of Single instance of Driver Program driving multiple servers on ESX Host(s)
    public User ( int userid , int server_id)
      {
      Userid = userid;
      target_server_id = server_id;
      //Console.WriteLine("user {0} created", userid);
      }
    //
    //-------------------------------------------------------------------------------------------------
    //
    public void Emulate ( )
      {
      int i , customerid_out = 0 , neworderid_out = 0 , rows_returned = 0, reviewhelpfulnessid_out = 0, newreviewid_out = 0, failures;
      bool IsLogin , IsRollback, IsNewMember, IsNewReview, IsNewHelpfulness;
      double rt = 0 , rt_tot , rt_login , rt_newcust , rt_browse , rt_purchase;
      double rt_newmember, rt_reviewbrowse, rt_newreview, rt_newhelpfulness;

      string[] title_out = new string[GlobalConstants.MAX_ROWS];         // Login, Browse
      string[] actor_out = new string[GlobalConstants.MAX_ROWS];         // Login, Browse
      string[] related_title_out = new string[GlobalConstants.MAX_ROWS]; // Login
      int[] prod_id_out = new int[GlobalConstants.MAX_ROWS];             // Browse
      decimal[] price_out = new decimal[GlobalConstants.MAX_ROWS];       // Browse
      int[] special_out = new int[GlobalConstants.MAX_ROWS];             // Browse
      int[] common_prod_id_out = new int[GlobalConstants.MAX_ROWS];      // Browse
      int[] prod_id_in = new int[GlobalConstants.MAX_ROWS];              // Purchase
      int[] qty_in = new int[GlobalConstants.MAX_ROWS];                  // Purchase
      int[] review_id_out = new int[GlobalConstants.MAX_ROWS];           // Browse Reviews, Get Reviews
      string[] review_date_out = new string[GlobalConstants.MAX_ROWS];   // Browse Reviews, Get Reviews
      int[] review_stars_out = new int[GlobalConstants.MAX_ROWS];        // Browse Reviews, Get Reviews
      int[] review_customerid_out = new int[GlobalConstants.MAX_ROWS];   // Browse Reviews, Get Reviews
      string[] review_summary_out = new string[GlobalConstants.MAX_ROWS];// Browse Reviews, Get Reviews
      string[] review_text_out = new string[GlobalConstants.MAX_ROWS];   // Browse Reviews, Get Reviews
      int[] review_helpfulness_sum_out = new int[GlobalConstants.MAX_ROWS]; // Browse Reviews, Get Reviews
      int get_review_stars_in;                                           // Browse Reviews, Get Reviews
      int n_reviewbrowse = 0;
      int n_getreviewbrowse = 0;
                          // New Review
      //string new_review_summary_in = new string[GlobalConstants.MAX_ROWS]; // New Review
      //string new_review_text_in = new string[1000];                        // New Review
      
         


      Thread.CurrentThread.Name = Userid.ToString ( );
      Console.WriteLine ( "Thread {0}: created for User {1}" , Thread.CurrentThread.Name , Userid );

      lock ( typeof ( User ) )  // Only allow one instance of User to access this code at a time
        {
        ++Controller.n_threads_running;
        }

      // Create random stream r with very randomized seed
      Random rtemp = new Random ( Userid * 1000 ); // Temporary seed
      // For multi-thread runs sleep between 0 - 10 second to spread out Ticks (100 nsecs)
      if ( Controller.n_threads > 1 ) Thread.Sleep ( rtemp.Next ( 10000 ) );
      long DTNT = DateTime.Now.Ticks;
      uint lowDTNT = ( uint ) ( 0x00000000ffffffff & DTNT );
      uint rev_lowDTNT = 0;  // take low 32 bits of Tick counter and reverse them
      for ( i = 0 ; i < 32 ; i++ ) rev_lowDTNT = rev_lowDTNT | ( ( 0x1 & ( lowDTNT >> i ) ) << ( 31 - i ) );
      //Console.WriteLine("DTNT= 0x{0:x16}  lowDTNT= 0x{1:x8}  rev_lowDTNT= 0x{2:x8}", DTNT, lowDTNT, rev_lowDTNT);
      r = new Random ( ( int ) rev_lowDTNT );

      //ds2interfaces[Userid] = new ds2Interface ( Userid );
      //Changed by GSK
      ds2interfaces[Userid] = new ds2Interface ( Userid , Controller.target_servers[target_server_id].ToString() );

      if ( !ds2interfaces[Userid].ds2initialize ( ) )
        {
        //Console.WriteLine ( "Can't initialize " + Controller.target + "; exiting" );
        //Changed by GSK
        Console.WriteLine ( "Can't initialize " + Controller.target_servers[target_server_id].ToString ( ) + "; exiting" );
        return;
        }

      // Users randomly start connecting over a (#users/ramp_rate) sec period
      Thread.Sleep ( r.Next ( ( int ) Math.Floor ( 1000.0 * Controller.n_threads / ( double ) Controller.ramp_rate ) ) );

      if ( !ds2interfaces[Userid].ds2connect ( ) )
        {
        //Console.WriteLine ( "Thread {0}: can't connect to {1}; exiting" , Thread.CurrentThread.Name ,
        //  Controller.target );
        //Changed by GSK
        Console.WriteLine ( "Thread {0}: can't connect to {1}; exiting" , Thread.CurrentThread.Name ,
          Controller.target_servers[target_server_id].ToString ( ) );
        return;
        }

      //Console.WriteLine ( "Thread {0}: connected to {1}" , Thread.CurrentThread.Name , Controller.target );
      //Changed by GSK
      Console.WriteLine ( "Thread {0}: connected to {1}" , Thread.CurrentThread.Name , Controller.target_servers[target_server_id].ToString ( ) );

      lock ( typeof ( User ) )  // Only allow one instance of User to access this code at a time
        {
        ++Controller.n_threads_connected;
        }

      // Wait for all threads to connect
      while ( !Controller.Start ) Thread.Sleep ( 100 );

      // Thread emulation loop - execute until Controller signals END      
      do
        {
        //Console.WriteLine ( "Thread {0}: Running for User {1}" , Thread.CurrentThread.Name , Userid );
        // Initialize response time accumulators and other variables
        rt_tot = 0.0;  //  total response time for all phases of this emulation loop order
        rt_login = 0.0;  //  response time for login in this emulation loop
        rt_newcust = 0.0;  //  response time for new cust registration in this emulation loop
        rt_newmember = 0.0;  // response time for customer joining membership program in this emulation loop
        rt_browse = 0.0;  //  total response time for browses in this emulation loop
        rt_reviewbrowse = 0.0;  // total response time for review browses in this emulation loop
        rt_newreview = 0.0;  // total response time for new reviews created in this emulation loop
        rt_newhelpfulness = 0.0;  // total response time for new helpfulness ratings of reviews in this emulation loop
        rt_purchase = 0.0;  //  response time for purchase in this emulation loop       

        IsLogin = false;
        IsRollback = false;
        IsNewMember = false;
        IsNewReview = false;
        IsNewHelpfulness = false;

        // Login/New Customer Phase

        double user_type = r.NextDouble ( );

        if ( user_type >= Controller.pct_newcustomers / 100.0 ) // If this is true we have a returning customer 
          {
          IsLogin = true;
          //Returning user with randomized username
          int i_user = 1 + r.Next ( Controller.max_customer );
          username_in = "user" + i_user;
          password_in = "password";
          rows_returned = 0;

          // Modified by DJ 11/20/2016 to allow failures before dropping thread
          failures = 0;
          while ( !ds2interfaces[Userid].ds2login ( username_in , password_in , ref customerid_out , ref rows_returned ,
            ref title_out , ref actor_out , ref related_title_out , ref rt ) )
            {
            if (++failures < GlobalConstants.MAX_FAILURES)
              {
              Console.WriteLine ( "Thread {0}: Error in Login for User {1}, failure {2}, retrying" ,
                Thread.CurrentThread.Name , username_in, failures);
    	      }
	        else 
	          {
              Console.WriteLine ( "Thread {0}: Error in Login for User {1}, failure {2}, exiting" ,
                Thread.CurrentThread.Name , username_in, failures);
              return;
	          }
            }

          if ( customerid_out == 0 )
            {
            Console.WriteLine ( "Thread {0}: User {1} not found, thread exiting" ,
              Thread.CurrentThread.Name , username_in );
            return;
            }

          //        Console.WriteLine("Thread {0}: User {1} logged in, customerid= {2}, previous DVDs ordered= {3}, " +
          //          "RT= {4,10:F3}", Thread.CurrentThread.Name, username_in, customerid_out, rows_returned, rt);  
          //        for (i=0; i<rows_returned; i++)
          //          Console.WriteLine("Thread {0}: title= {1} actor= {2} related_title= {3}", 
          //            Thread.CurrentThread.Name, title_out[i], actor_out[i], related_title_out[i]);

          rt_login = rt;
          rt_tot += rt;
          }  // end returning customer

        // New Customer with randomized username

        else   // New user
          {
          CreateUserData ( );
          do  // Try newcustomer until find a userid that doesn't exist
            {
            int i_user = 1 + r.Next ( Controller.max_customer );
            username_in = "newuser" + i_user;
            password_in = "password";

            failures = 0;
            while ( !ds2interfaces[Userid].ds2newcustomer ( username_in , password_in , firstname_in , lastname_in ,
              address1_in , address2_in , city_in , state_in , zip_in , country_in , email_in , phone_in ,
              creditcardtype_in , creditcard_in , ccexpmon_in , ccexpyr_in , age_in , income_in , gender_in ,
              ref customerid_out , ref rt ) )
              {
              if (++failures < GlobalConstants.MAX_FAILURES)
                {
                  Console.WriteLine ( "Thread {0}: Error in New Customer for User {1}, failure {2}, retrying" ,
                  Thread.CurrentThread.Name , username_in, failures);
	            }
	          else 
	            {
                  Console.WriteLine ( "Thread {0}: Error in New Customer for User {1}, failure {2}, exiting" ,
                  Thread.CurrentThread.Name , username_in, failures);
                  return;
	            }
              }
              

            if (customerid_out == 0)
            {
                Console.WriteLine("User name {0} already exists", username_in);
            }
            if (customerid_out == -1)
            {
                Console.WriteLine("New Customer - DB didn't return value for new customerid, retrying... ");
            }
            } while ( customerid_out < 1 ); // end of do/while try newcustomer

//        Console.WriteLine("Thread {0}: New user {1} logged in, customerid = {2}, RT= {3,10:F3}", 
//           Thread.CurrentThread.Name, username_in, customerid_out, rt);  

          rt_newcust = rt;  // Just count last iteration if had to retry username
          rt_tot += rt;

          } //End of Else (new user)

        // End of Login/New Customer Phase

          // Begin New Member Phase 

        if ( ( user_type <= Controller.pct_newmember / 100.0 ) && (!Controller.ds2_mode)) // If this is true we have a customer that wants to join membership program
        {
            IsNewMember = true;
            do  // Try newmember until find a userid that doesn't exist
            {
            customerid_in = 1 + r.Next ( Controller.max_customer );
            membershiplevel_in = 1 + r.Next(3);

            failures = 0;
            while ( !ds2interfaces[Userid].ds2newmember ( customerid_in , membershiplevel_in , ref customerid_out , ref rt ) )
              {
              if (++failures < GlobalConstants.MAX_FAILURES)
                {
                  Console.WriteLine ( "Thread {0}: Error in New Member for User {1}, failure {2}, retrying" ,
                    Thread.CurrentThread.Name , username_in, failures);
    	        }
  	          else 
  	            {
                  Console.WriteLine ( "Thread {0}: Error in New Member for User {1}, failure {2}, exiting" ,
                    Thread.CurrentThread.Name , username_in, failures);
                  return;
  	            }
              }

            if ( customerid_out == 0 ) Console.WriteLine ( "Customer {0} is already a member" , customerid_in );
            } while ( customerid_out == 0 ); // end of do/while try newcustomer

//        Console.WriteLine("Thread {0}: New user {1} logged in, customerid = {2}, RT= {3,10:F3}", 
//           Thread.CurrentThread.Name, username_in, customerid_out, rt);  

          rt_newmember = rt;  // Just count last iteration if had to retry username
          rt_tot += rt;

          } //End of IF 
        
          // End of New Member Phase

        // Browse Phase

        // Search Product table different ways:
        // Browse by Category: with category randomized between 1 and MAX_CATEGORY (and SPECIAL=1)
        // Browse by Actor:  with first and last names selected randomly from list of names
        // Browse by Title:  with first and last words in title selected randomly from list of title words

        string browse_type_in = "" , browse_category_in = "" , browse_actor_in = "" , browse_title_in = "";
        string browse_criteria = "";
        int batch_size_in;

        int n_browse = 1 + r.Next ( 2 * Controller.n_searches - 1 );   // Perform average of n_searches searches
        for ( int ib = 0 ; ib < n_browse ; ib++ )
          {
          batch_size_in = 1 + r.Next ( 2 * Controller.search_batch_size - 1 ); // request avg of search_batch_size lines
          int search_type = r.Next ( 3 ); // randomly select search type
          switch ( search_type )
            {
            case 0:  // Search by Category
              browse_type_in = "category";
              browse_category_in = ( 1 + r.Next ( GlobalConstants.MAX_CATEGORY ) ).ToString ( );
              browse_actor_in = "";
              browse_title_in = "";
              browse_criteria = browse_category_in;
              break;
            case 1:  // Search by Actor 
              browse_type_in = "actor";
              browse_category_in = "";
              CreateActor ( );
              browse_actor_in = actor_in;
              browse_title_in = "";
              browse_criteria = browse_actor_in;
              break;
            case 2:  // Search by Title
              browse_type_in = "title";
              browse_category_in = "";
              browse_actor_in = "";
              CreateTitle ( );
              browse_title_in = title_in;
              browse_criteria = browse_title_in;
              break;
            }

          failures = 0;
            while ( !ds2interfaces[Userid].ds2browse ( browse_type_in , browse_category_in , browse_actor_in ,
            browse_title_in , batch_size_in , customerid_out , ref rows_returned , ref prod_id_out , ref title_out ,
            ref actor_out , ref price_out , ref special_out , ref common_prod_id_out , ref rt ) )
            {
            if (++failures < GlobalConstants.MAX_FAILURES)
              {
               Console.WriteLine ( "Thread {0}: Error in simple product Browse for User {1}, failure {2}, retrying" ,
                Thread.CurrentThread.Name , username_in, failures);
    	      }
	        else 
	          {
               Console.WriteLine ( "Thread {0}: Error in simple product Browse for User {1}, failure {2}, exiting" ,
                Thread.CurrentThread.Name , username_in, failures);
               return;
	          }
            }

//        Console.WriteLine("Thread {0}: Search by {1}={2} returned {3} DVDs ({4} requested), RT= {5,10:F3}", 
//        Thread.CurrentThread.Name, browse_type_in, browse_criteria, rows_returned, batch_size_in,rt);
//        for (i=0; i<rows_returned; i++)
//          Console.WriteLine("  Thread {0}: prod_id= {1} title= {2} actor= {3} price= {4} special= {5}" + 
//            " common_prod_id= {6}", 
//            Thread.CurrentThread.Name, prod_id_out[i], title_out[i], actor_out[i],
//            price_out[i], special_out[i], common_prod_id_out[i]);

          rt_browse += rt;
          }  // End of for ib=0 to n_browse

        rt_tot += rt_browse;

        // End of Browse Phase

        // Browse Reviews Phase

        // Search for reviews by Actor name or title 
        // 
        // GET_PROD_REVIEWS_BY_ACTOR - Search by actor name for product reviews
        // GET_PROD_REVIEWS_BY_TITLE - Search by title name for product reviews

        if (!Controller.ds2_mode)    // if ds2_mode is set to true, then don't do get reviews or helpfulness
        {
            string get_review_type_in = "", get_review_category_in = "", get_review_actor_in = "", get_review_title_in = "";
            int get_review_prod_in;
            string get_review_criteria = "";
            // int batch_size_in;

            n_reviewbrowse = 1 + r.Next(2 * Controller.n_reviews - 1);   // Perform average of n_reviews searches
            for (int ib = 0; ib < n_reviewbrowse; ib++)
            {
                batch_size_in = 1 + r.Next(2 * Controller.search_batch_size - 1); // request avg of search_batch_size lines
                int search_type = r.Next(2); // randomly select search type
                switch (search_type)
                {
                    case 0:  // Get Reviews by Actor 
                        get_review_type_in = "actor";
                        get_review_prod_in = 0;
                        CreateActor();
                        actornames_in = actor_in.Split(' ');     // Get just one name for searching
                        get_review_actor_in = actornames_in[1];
                        get_review_title_in = "";
                        get_review_criteria = get_review_actor_in;
                        break;
                    case 1:  // Get Reviews by Title
                        get_review_type_in = "title";
                        get_review_category_in = "";
                        get_review_actor_in = "";
                        CreateTitle();
                        titlenames_in = title_in.Split(' ');       // Get just one word for title search
                        get_review_title_in = titlenames_in[1];
                        get_review_criteria = get_review_title_in;
                        break;
                }
                failures = 0;
                while (!ds2interfaces[Userid].ds2browsereview(get_review_type_in, get_review_category_in, get_review_actor_in,
                  get_review_title_in, batch_size_in, customerid_out, ref rows_returned, ref prod_id_out, ref title_out,
                  ref actor_out, ref review_id_out, ref review_date_out, ref review_stars_out, ref review_customerid_out,
                  ref review_summary_out, ref review_text_out, ref review_helpfulness_sum_out, ref rt))
                  {
                   if (++failures < GlobalConstants.MAX_FAILURES)
                    {
                      Console.WriteLine ( "Thread {0}: Error in browse reviews for User {1}, failure {2}, retrying" ,
                        Thread.CurrentThread.Name , username_in, failures);
	                }
	                else 
	                {
                      Console.WriteLine ( "Thread {0}: Error in browse reviews for User {1}, failure {2}, exiting" ,
                        Thread.CurrentThread.Name , username_in, failures);
                      return;
                    }          
                  }
                rt_reviewbrowse += rt;
            }  // End of for ib=0 to n_browse

            rt_tot += rt_reviewbrowse;


            // End of Browse Reviews Phase

            // Get Reviews Phase

            // GET_PROD_REVIEWS - Get product reviews for a specific product
            // GET_PROD_REVIEWS_BY_DATE - Get product reviews for a specific product sorted by date
            // GET_PROD_REVIEWS_BY_STARS - Get product reviews for a specific product at a specific "stars" level

            get_review_type_in = "";
            get_review_stars_in = 1 + r.Next(5);    //Randomly select the star level to search for
            get_review_prod_in = 0;
            //string get_review_criteria = "";
            // int batch_size_in;

            n_getreviewbrowse = 1 + r.Next(2 * Controller.n_reviews - 1);   // Perform average of n_searches searches
            for (int ib = 0; ib < n_getreviewbrowse; ib++)
            {
                batch_size_in = 1 + r.Next(2 * Controller.search_batch_size - 1); // request avg of search_batch_size lines
                int search_type = r.Next(3); // randomly select search type
                switch (search_type)
                {
                    case 0:  // Get Reviews with no order 
                        get_review_type_in = "noorder";
                        // assign get_review_prod_in to be a random product id number
                        get_review_prod_in = Controller.prod_array[r.Next(Controller.prod_array_size)];
                        break;
                    case 1:  // Get Reviews by Star ranking 
                        get_review_type_in = "star";
                        get_review_prod_in = Controller.prod_array[r.Next(Controller.prod_array_size)];
                        break;
                    case 2:  // Get Reviews by date
                        get_review_type_in = "date";
                        get_review_prod_in = Controller.prod_array[r.Next(Controller.prod_array_size)];
                        break;
                }
                failures = 0;
                while (!ds2interfaces[Userid].ds2getreview(get_review_type_in, get_review_prod_in, get_review_stars_in, customerid_out, batch_size_in, ref rows_returned, ref prod_id_out,
                   ref review_id_out, ref review_date_out, ref review_stars_out, ref review_customerid_out,
                   ref review_summary_out, ref review_text_out, ref review_helpfulness_sum_out, ref rt))
                   {
                   if (++failures < GlobalConstants.MAX_FAILURES)
                     {
                       Console.WriteLine ( "Thread {0}: Error in get review for User {1}, failure {2}, retrying" ,
                         Thread.CurrentThread.Name , username_in, failures);
	                 }
	              else 
	                 {
                       Console.WriteLine ( "Thread {0}: Error in get review for User {1}, failure {2}, exiting" ,
                         Thread.CurrentThread.Name , username_in, failures);
                       return;
                     }
                   }
                rt_reviewbrowse += rt;
            }  // End of for ib=0 to n_browse

            rt_tot += rt_reviewbrowse;

            // End of Get Reviews Phase

            // Begin New Review Phase
            if (user_type <= Controller.pct_newreviews / 100.0) // If this is true we have a customer that wants to submit a new review
            {
                IsNewReview = true;
                review_data_terms = InitReviewDataTerms();
                new_review_summary_in = CreateReviewData(ref review_data_terms, 3);
                new_review_text_in = CreateReviewData(ref review_data_terms, 25);
                new_review_stars_in = 1 + r.Next(5);
                new_review_prod_id_in = 1 + r.Next(Controller.max_product);

                failures = 0;
                while (!ds2interfaces[Userid].ds2newreview(new_review_prod_id_in, new_review_stars_in, customerid_out,
                  new_review_summary_in, new_review_text_in, ref newreviewid_out, ref rt))
                {
                  if (++failures < GlobalConstants.MAX_FAILURES)
                    {
                      Console.WriteLine ( "Thread {0}: Error in new review for User {1}, failure {2}, retrying" ,
                        Thread.CurrentThread.Name , username_in, failures);
	                }
	              else 
	                {
                      Console.WriteLine ( "Thread {0}: Error in new review for User {1}, failure {2}, exiting" ,
                        Thread.CurrentThread.Name , username_in, failures);
                      return;
                    }
                }

                rt_newreview = rt;
                rt_tot += rt;
            }
            //End New Review Phase

            // Begin New Review Helpfulness Phase 

            if (user_type <= Controller.pct_newhelpfulness / 100.0) // If this is true we have a customer that wants to rate a reviews helpfulness
            {
                IsNewHelpfulness = true;
                reviewid_in = 1 + r.Next(Controller.max_review);
                reviewhelpfulness_in = 1 + r.Next(10);

                failures = 0;
                while (!ds2interfaces[Userid].ds2newreviewhelpfulness(reviewid_in, customerid_out, reviewhelpfulness_in, ref reviewhelpfulnessid_out, ref rt))
                {
                   if (++failures < GlobalConstants.MAX_FAILURES)
                     {
                       Console.WriteLine ( "Thread {0}: Error in new review helpfulness for User {1}, failure {2}, retrying" ,
                         Thread.CurrentThread.Name , username_in, failures);
	                 }
	               else 
	                 {
                       Console.WriteLine ( "Thread {0}: Error in new review helpfulness for User {1}, failure {2}, exiting" ,
                         Thread.CurrentThread.Name , username_in, failures);
                       return;
	                 }
                }

                rt_newhelpfulness = rt;  // Just count last iteration if had to retry username
                rt_tot += rt;

            } //End of IF 

            // End of New Helpfulness Phase

        } // end of if for ds2_mode to exclude reviews and helpfulness opreations
        // Purchase Phase

        for ( i = 0 ; i < GlobalConstants.MAX_ROWS ; i++ )
          {
          prod_id_in[i] = 0;
          qty_in[i] = 0;
          }

        // Randomize number of cart items with average n_line_items
        int cart_items = 1 + r.Next ( 2 * Controller.n_line_items - 1 );

        //For each cart item take product_id from search results or randomly select
        //for (i=0; i<cart_items; i++)
        //  {
        //  prod_id_in[i] = (rows_returned > i) ? prod_id_out[i] : (1 + r.Next(Controller.max_product));
        //  qty_in[i] = 1 + r.Next(3);  // qty (1, 2 or 3)
        //  }

        // For each cart item randomly select product_id using weighted prod_array
        for ( i = 0 ; i < cart_items ; i++ )
          {
          prod_id_in[i] = Controller.prod_array[r.Next ( Controller.prod_array_size )];
          qty_in[i] = 1 + r.Next ( 3 );  // qty (1, 2 or 3)
          //        Console.WriteLine("Thread {0}: Purchase prod_id_in[{1}] = {2}  qty_in[{1}]= {3}", 
          //          Thread.CurrentThread.Name, i, prod_id_in[i], qty_in[i]);
          }

        failures = 0;
        while ( !ds2interfaces[Userid].ds2purchase ( cart_items , prod_id_in , qty_in , customerid_out , ref neworderid_out ,
          ref IsRollback , ref rt ) )
          {
          if (++failures < GlobalConstants.MAX_FAILURES)
            {
            Console.WriteLine ( "Thread {0}: Error in Purchase for User {1}, failure {2}, retrying" ,
              Thread.CurrentThread.Name , username_in, failures);
    	    }
	      else 
	        {
            Console.WriteLine ( "Thread {0}: Error in Purchase for User {1}, failure {2}, exiting" ,
              Thread.CurrentThread.Name , username_in, failures);
            return;
	        }
          }

        //      Console.WriteLine("Thread {0}: Purchase completed successfully, neworderid = {1}, rollback= {2}, " +
        //        "RT= {3,10:F3}", Thread.CurrentThread.Name, neworderid_out, IsRollback, rt);

        rt_purchase = rt;
        rt_tot += rt;

        // End of Purchase Phase
        // End of Order sequence

        // Block other User threads or Controller from accessing this code while we update these values
        Monitor.Enter ( Controller.UpdateLock );
        if ( IsLogin )
          {
          ++Controller.n_login_overall;
          ++Controller.arr_n_login_overall[target_server_id];                 //Added by GSK (all Controller class members starting with arr_%)
          Controller.rt_login_overall += rt_login;
          Controller.arr_rt_login_overall[target_server_id] += rt_login;      
          }
        else
          {
          ++Controller.n_newcust_overall;
          ++Controller.arr_n_newcust_overall[target_server_id];               
          Controller.rt_newcust_overall += rt_newcust;
          Controller.arr_rt_newcust_overall[target_server_id] += rt_newcust;  
          }
        if ( IsNewMember )
          {
          ++Controller.n_newmember_overall;
          ++Controller.arr_n_newmember_overall[target_server_id];
          Controller.rt_newmember_overall += rt_newmember;
          Controller.arr_rt_newmember_overall[target_server_id] += rt_newmember;
          }
        Controller.n_browse_overall += n_browse;
        Controller.arr_n_browse_overall[target_server_id] += n_browse;          
        Controller.rt_browse_overall += rt_browse;
        Controller.arr_rt_browse_overall[target_server_id] += rt_browse;
        Controller.n_reviewbrowse_overall += (n_reviewbrowse + n_getreviewbrowse);
        Controller.arr_n_reviewbrowse_overall[target_server_id] += (n_reviewbrowse + n_getreviewbrowse);
        Controller.rt_reviewbrowse_overall += rt_reviewbrowse;
        Controller.arr_rt_reviewbrowse_overall[target_server_id] += rt_reviewbrowse;
        if (IsNewReview)
        {
            ++Controller.n_newreview_overall;
            ++Controller.arr_n_newreview_overall[target_server_id];
            Controller.rt_newreview_overall += rt_newmember;
            Controller.arr_rt_newreview_overall[target_server_id] += rt_newmember;
        }
        if (IsNewHelpfulness)
        {
            ++Controller.n_newhelpfulness_overall;
            ++Controller.arr_n_newhelpfulness_overall[target_server_id];
            Controller.rt_newhelpfulness_overall += rt_newhelpfulness;
            Controller.arr_rt_newhelpfulness_overall[target_server_id] += rt_newhelpfulness;
        }
        ++Controller.n_purchase_overall;
        ++Controller.arr_n_purchase_overall[target_server_id];                  
        ++Controller.n_purchase_from_start;                                     
        ++Controller.arr_n_purchase_from_start[target_server_id];               
        Controller.rt_purchase_overall += rt_purchase;
        Controller.arr_rt_purchase_overall[target_server_id] += rt_purchase;    
        
        if ( IsRollback )
          {
          ++Controller.n_rollbacks_overall;
          ++Controller.arr_n_rollbacks_overall[target_server_id];             
          ++Controller.n_rollbacks_from_start;                                
          ++Controller.arr_n_rollbacks_from_start[target_server_id];          
          }

        ++Controller.n_overall;
        ++Controller.arr_n_overall[target_server_id];                                           
        Controller.rt_tot_overall += rt_tot;
        Controller.arr_rt_tot_overall[target_server_id] += rt_tot;                              
        Controller.rt_tot_lastn[Controller.n_overall % GlobalConstants.LAST_N] = rt_tot;
                
        int arrIndex = Controller.arr_n_overall[target_server_id] % GlobalConstants.LAST_N;    
        Controller.arr_rt_tot_lastn[target_server_id,arrIndex] = rt_tot;                        
                                            
        Monitor.Exit ( Controller.UpdateLock );

        Thread.Sleep ( r.Next ( 2 * ( int ) Math.Floor ( 1000 * Controller.think_time ) ) ); // Delay think time seconds               

        } while ( !Controller.End ); // End of Thread Emulation loop

      ds2interfaces[Userid].ds2close ( );

      Console.WriteLine ( "Thread {0}: exiting" , Thread.CurrentThread.Name ); Console.Out.Flush ( );
      }  // End of Emulate()
    //
    //-------------------------------------------------------------------------------------------------
    //          
    void CreateUserData ( )
      {
      string[] states = new string[] {"AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "HI", "IA", 
                        "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MI", "MN", "MO", "MS", "MT", "NC", 
                        "ND", "NE", "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", 
                        "TX", "UT", "VA", "VT", "WA", "WI", "WV", "WY"};

      string[] countries = new string[] {"Australia", "Canada", "Chile", "China", "France", "Germany", "Japan", 
                           "Russia", "South Africa", "UK"};

      int j;
      firstname_in = ""; for ( j = 0 ; j < 6 ; j++ ) { firstname_in = firstname_in + ( char ) ( 65 + r.Next ( 26 ) ); }
      lastname_in = ""; for ( j = 0 ; j < 10 ; j++ ) { lastname_in = lastname_in + ( char ) ( 65 + r.Next ( 26 ) ); }
      city_in = ""; for ( j = 0 ; j < 7 ; j++ ) { city_in = city_in + ( char ) ( 65 + r.Next ( 26 ) ); }

      if ( r.Next ( 2 ) == 1 ) // Select region (US or ROW)
        { //ROW    
        zip_in = (r.Next(100000)).ToString();
        state_in = "";
        country_in = countries[r.Next ( 10 )];
        }
      else //US
        {
        zip_in = ( r.Next ( 100000 ) ).ToString ( );
        state_in = states[r.Next ( 50 )];
        country_in = "US";
        } //End Else

      phone_in = "" + r.Next ( 100 , 1000 ) + r.Next ( 10000000 );
      creditcardtype_in = 1 + r.Next ( 5 );
      creditcard_in = "" + r.Next ( 10000000 , 100000000 ) + r.Next ( 10000000 , 100000000 );
      ccexpmon_in = 1 + r.Next ( 12 );
      ccexpyr_in = 2008 + r.Next ( 5 );
      address1_in = phone_in + " Dell Way";
      address2_in = "";
      email_in = lastname_in + "@dell.com";
      age_in = r.Next ( 18 , 100 );
      income_in = 20000 * r.Next ( 1 , 6 ); // >$20,000, >$40,000, >$60,000, >$80,000, >$100,000
      gender_in = ( r.Next ( 2 ) == 1 ) ? "M" : "F";

      }  // End of CreateUserData

    //
    //-------------------------------------------------------------------------------------------------
    //      
    void CreateActor ( )
      {
      // Names compiled by Dara Jaffe

      // 200 actor/actress firstnames
      string[] actor_firstnames = new string[]
        {
        "ADAM", "ADRIEN", "AL", "ALAN", "ALBERT", "ALEC", "ALICIA", "ANDY", "ANGELA", "ANGELINA", "ANJELICA", 
        "ANNE", "ANNETTE", "ANTHONY", "AUDREY", "BELA", "BEN", "BETTE", "BOB", "BRAD", "BRUCE", "BURT", "CAMERON", 
        "CANDICE", "CARMEN", "CARRIE", "CARY", "CATE", "CHARLES", "CHARLIZE", "CHARLTON", "CHEVY", "CHRIS", 
        "CHRISTIAN", "CHRISTOPHER", "CLARK", "CLINT", "CUBA", "DAN", "DANIEL", "DARYL", "DEBBIE", "DENNIS", 
        "DENZEL", "DIANE", "DORIS", "DREW", "DUSTIN", "ED", "EDWARD", "ELIZABETH", "ELLEN", "ELVIS", "EMILY", 
        "ETHAN", "EWAN", "FARRAH", "FAY", "FRANCES", "FRANK", "FRED", "GARY", "GENE", "GEOFFREY", "GINA", "GLENN", 
        "GOLDIE", "GRACE", "GREG", "GREGORY", "GRETA", "GROUCHO", "GWYNETH", "HALLE", "HARRISON", "HARVEY", 
        "HELEN", "HENRY", "HILARY", "HUGH", "HUME", "HUMPHREY", "IAN", "INGRID", "JACK", "JADA", "JAMES", "JANE", 
        "JAYNE", "JEFF", "JENNIFER", "JEREMY", "JESSICA", "JIM", "JOAN", "JODIE", "JOE", "JOHN", "JOHNNY", "JON", 
        "JUDE", "JUDI", "JUDY", "JULIA", "JULIANNE", "JULIETTE", "KARL", "KATE", "KATHARINE", "KENNETH", "KEVIN", 
        "KIM", "KIRK", "KIRSTEN", "LANA", "LAURA", "LAUREN", "LAURENCE", "LEELEE", "LENA", "LEONARDO", "LIAM", 
        "LISA", "LIV", "LIZA", "LUCILLE", "MADELINE", "MAE", "MARILYN", "MARISA", "MARLENE", "MARLON", "MARY", 
        "MATT", "MATTHEW", "MEG", "MEL", "MENA", "MERYL", "MICHAEL", "MICHELLE", "MILLA", "MINNIE", "MIRA", 
        "MORGAN", "NATALIE", "NEVE", "NICK", "NICOLAS", "NICOLE", "OLYMPIA", "OPRAH", "ORLANDO", "PARKER", 
        "PAUL", "PEARL", "PENELOPE", "RALPH", "RAY", "REESE", "RENEE", "RICHARD", "RIP", "RITA", "RIVER", 
        "ROBERT", "ROBIN", "ROCK", "ROSIE", "RUBY", "RUSSELL", "SALLY", "SALMA", "SANDRA", "SCARLETT", "SEAN", 
        "SHIRLEY", "SIDNEY", "SIGOURNEY", "SISSY", "SOPHIA", "SPENCER", "STEVE", "SUSAN", "SYLVESTER", "THORA", 
        "TIM", "TOM", "UMA", "VAL", "VIVIEN", "WALTER", "WARREN", "WHOOPI", "WILL", "WILLEM", "WILLIAM", "WINONA", 
        "WOODY", "ZERO"
        };

      // 200 actor/actress lastnames  
      string[] actor_lastnames = new string[]
        {
        "AFFLECK", "AKROYD", "ALLEN", "ANISTON", "ASTAIRE", "BACALL", "BAILEY", "BALE", "BALL", "BARRYMORE", 
        "BASINGER", "BEATTY", "BENING", "BERGEN", "BERGMAN", "BERRY", "BIRCH", "BLANCHETT", "BLOOM", "BOGART", 
        "BOLGER", "BRANAGH", "BRANDO", "BRIDGES", "BRODY", "BULLOCK", "CAGE", "CAINE", "CAMPBELL", "CARREY", 
        "CHAPLIN", "CHASE", "CLOSE", "COOPER", "COSTNER", "CRAWFORD", "CRONYN", "CROWE", "CRUISE", "CRUZ", 
        "DAFOE", "DAMON", "DAVIS", "DAY", "DAY-LEWIS", "DEAN", "DEE", "DEGENERES", "DENCH", "DENIRO", 
        "DEPP", "DERN", "DIAZ", "DICAPRIO", "DIETRICH", "DOUGLAS", "DREYFUSS", "DRIVER", "DUKAKIS", "DUNST", 
        "EASTWOOD", "FAWCETT", "FIELD", "FIENNES", "FINNEY", "FISHER", "FONDA", "FORD", "FOSTER", "FREEMAN", 
        "GABLE", "GARBO", "GARCIA", "GARLAND", "GIBSON", "GOLDBERG", "GOODING", "GRANT", "GUINESS", "HACKMAN", 
        "HANNAH", "HARRIS", "HAWKE", "HAWN", "HAYEK", "HECHE", "HEPBURN", "HESTON", "HOFFMAN", "HOPE", 
        "HOPKINS", "HOPPER", "HORNE", "HUDSON", "HUNT", "HURT", "HUSTON", "IRONS", "JACKMAN", "JOHANSSON", 
        "JOLIE", "JOVOVICH", "KAHN", "KEATON", "KEITEL", "KELLY", "KIDMAN", "KILMER", "KINNEAR", "KUDROW", 
        "LANCASTER", "LANSBURY", "LAW", "LEIGH", "LEWIS", "LOLLOBRIGIDA", "LOREN", "LUGOSI", "MALDEN", "MANSFIELD", 
        "MARTIN", "MARX", "MATTHAU", "MCCONAUGHEY", "MCDORMAND", "MCGREGOR", "MCKELLEN", "MCQUEEN", "MINELLI", "MIRANDA",  
        "MONROE", "MOORE", "MOSTEL", "NEESON", "NEWMAN", "NICHOLSON", "NOLTE", "NORTON", "ODONNELL", "OLIVIER", 
        "PACINO", "PALTROW", "PECK", "PENN", "PESCI", "PFEIFFER", "PHOENIX", "PINKETT", "PITT", "POITIER", 
        "POSEY", "PRESLEY", "REYNOLDS", "RICKMAN", "ROBBINS", "ROBERTS", "RUSH", "RUSSELL", "RYAN", "RYDER", 
        "SANDLER", "SARANDON", "SILVERSTONE", "SINATRA", "SMITH", "SOBIESKI", "SORVINO", "SPACEK", "STALLONE", "STREEP", 
        "SUVARI", "SWANK", "TANDY", "TAUTOU", "TAYLOR", "TEMPLE", "THERON", "THURMAN", "TOMEI", "TORN", 
        "TRACY", "TURNER", "TYLER", "VOIGHT", "WAHLBERG", "WALKEN", "WASHINGTON", "WATSON", "WAYNE", "WEAVER", 
        "WEST", "WILLIAMS", "WILLIS", "WILSON", "WINFREY", "WINSLET", "WITHERSPOON", "WOOD", "WRAY", "ZELLWEGER"
        };

      actor_in = actor_firstnames[r.Next ( 200 )] + " " + actor_lastnames[r.Next ( 200 )];

      }  // End of CreateActor

    //
    //-------------------------------------------------------------------------------------------------
    //    
    void CreateTitle ( )
      {
      // Names compiled by Dara Jaffe

      // 1000 movie title words

      string[] movie_titles = new string[]
        {
        "ACADEMY", "ACE", "ADAPTATION", "AFFAIR", "AFRICAN", "AGENT", "AIRPLANE", "AIRPORT", "ALABAMA", "ALADDIN", 
        "ALAMO", "ALASKA", "ALI", "ALICE", "ALIEN", "ALLEY", "ALONE", "ALTER", "AMADEUS", "AMELIE", 
        "AMERICAN", "AMISTAD", "ANACONDA", "ANALYZE", "ANGELS", "ANNIE", "ANONYMOUS", "ANTHEM", "ANTITRUST", "ANYTHING", 
        "APACHE", "APOCALYPSE", "APOLLO", "ARABIA", "ARACHNOPHOBIA", "ARGONAUTS", "ARIZONA", "ARK", "ARMAGEDDON", "ARMY", 
        "ARSENIC", "ARTIST", "ATLANTIS", "ATTACKS", "ATTRACTION", "AUTUMN", "BABY", "BACKLASH", "BADMAN", "BAKED", 
        "BALLOON", "BALLROOM", "BANG", "BANGER", "BARBARELLA", "BAREFOOT", "BASIC", "BEACH", "BEAR", "BEAST", 
        "BEAUTY", "BED", "BEDAZZLED", "BEETHOVEN", "BEHAVIOR", "BENEATH", "BERETS", "BETRAYED", "BEVERLY", "BIKINI", 
        "BILKO", "BILL", "BINGO", "BIRCH", "BIRD", "BIRDCAGE", "BIRDS", "BLACKOUT", "BLADE", "BLANKET", 
        "BLINDNESS", "BLOOD", "BLUES", "BOILED", "BONNIE", "BOOGIE", "BOONDOCK", "BORN", "BORROWERS", "BOULEVARD", 
        "BOUND", "BOWFINGER", "BRANNIGAN", "BRAVEHEART", "BREAKFAST", "BREAKING", "BRIDE", "BRIGHT", "BRINGING", "BROOKLYN", 
        "BROTHERHOOD", "BUBBLE", "BUCKET", "BUGSY", "BULL", "BULWORTH", "BUNCH", "BUTCH", "BUTTERFLY", "CABIN", 
        "CADDYSHACK", "CALENDAR", "CALIFORNIA", "CAMELOT", "CAMPUS", "CANDIDATE", "CANDLES", "CANYON", "CAPER", "CARIBBEAN", 
        "CAROL", "CARRIE", "CASABLANCA", "CASPER", "CASSIDY", "CASUALTIES", "CAT", "CATCH", "CAUSE", "CELEBRITY", 
        "CENTER", "CHAINSAW", "CHAMBER", "CHAMPION", "CHANCE", "CHAPLIN", "CHARADE", "CHARIOTS", "CHASING", "CHEAPER", 
        "CHICAGO", "CHICKEN", "CHILL", "CHINATOWN", "CHISUM", "CHITTY", "CHOCOLAT", "CHOCOLATE", "CHRISTMAS", "CIDER", 
        "CINCINATTI", "CIRCUS", "CITIZEN", "CLASH", "CLEOPATRA", "CLERKS", "CLOCKWORK", "CLONES", "CLOSER", "CLUB", 
        "CLUE", "CLUELESS", "CLYDE", "COAST", "COLDBLOODED", "COLOR", "COMA", "COMANCHEROS", "COMFORTS", "COMMAND", 
        "COMMANDMENTS", "CONEHEADS", "CONFESSIONS", "CONFIDENTIAL", "CONFUSED", "CONGENIALITY", "CONNECTICUT", "CONNECTION", 
        "CONQUERER", "CONSPIRACY", "CONTACT", "CONTROL", "CONVERSATION", "CORE", "COWBOY", "CRAFT", "CRANES", "CRAZY", 
        "CREATURES", "CREEPERS", "CROOKED", "CROSSING", "CROSSROADS", "CROW", "CROWDS", "CRUELTY", "CRUSADE", "CRYSTAL", 
        "CUPBOARD", "CURTAIN", "CYCLONE", "DADDY", "DAISY", "DALMATIONS", "DANCES", "DANCING", "DANGEROUS", "DARES", 
        "DARKNESS", "DARKO", "DARLING", "DARN", "DATE", "DAUGHTER", "DAWN", "DAY", "DAZED", "DECEIVER", "DEEP", "DEER", 
        "DELIVERANCE", "DESERT", "DESIRE", "DESPERATE", "DESTINATION", "DESTINY", "DETAILS", "DETECTIVE", "DEVIL", "DIARY", 
        "DINOSAUR", "DIRTY", "DISCIPLE", "DISTURBING", "DIVIDE", "DIVINE", "DIVORCE", "DOCTOR", "DOGMA", "DOLLS", 
        "DONNIE", "DOOM", "DOORS", "DORADO", "DOUBLE", "DOUBTFIRE", "DOWNHILL", "DOZEN", "DRACULA", "DRAGON", 
        "DRAGONFLY", "DREAM", "DRIFTER", "DRIVER", "DRIVING", "DROP", "DRUMLINE", "DRUMS", "DUCK", "DUDE", 
        "DUFFEL", "DUMBO", "DURHAM", "DWARFS", "DYING", "DYNAMITE", "EAGLES", "EARLY", "EARRING", "EARTH", 
        "EASY", "EDGE", "EFFECT", "EGG", "EGYPT", "ELEMENT", "ELEPHANT", "ELF", "ELIZABETH", "EMPIRE", 
        "ENCINO", "ENCOUNTERS", "ENDING", "ENEMY", "ENGLISH", "ENOUGH", "ENTRAPMENT", "ESCAPE", "EVE", "EVERYONE", "EVOLUTION", 
        "EXCITEMENT", "EXORCIST", "EXPECATIONS", "EXPENDABLE", "EXPRESS", "EXTRAORDINARY", "EYES", "FACTORY", "FALCON", 
        "FAMILY", "FANTASIA", "FANTASY", "FARGO", "FATAL", "FEATHERS", "FELLOWSHIP", "FERRIS", "FEUD", "FEVER", 
        "FICTION", "FIDDLER", "FIDELITY", "FIGHT", "FINDING", "FIRE", "FIREBALL", "FIREHOUSE", "FISH", "FLAMINGOS", 
        "FLASH", "FLATLINERS", "FLIGHT", "FLINTSTONES", "FLOATS", "FLYING", "FOOL", "FOREVER", "FORREST", "FORRESTER", 
        "FORWARD", "FRANKENSTEIN", "FREAKY", "FREDDY", "FREEDOM", "FRENCH", "FRIDA", "FRISCO", "FROGMEN", "FRONTIER", 
        "FROST", "FUGITIVE", "FULL", "FURY", "GABLES", "GALAXY", "GAMES", "GANDHI", "GANGS", "GARDEN", 
        "GASLIGHT", "GATHERING", "GENTLEMEN", "GHOST", "GHOSTBUSTERS", "GIANT", "GILBERT", "GILMORE", "GLADIATOR", "GLASS", 
        "GLEAMING", "GLORY", "GO", "GODFATHER", "GOLD", "GOLDFINGER", "GOLDMINE", "GONE", "GOODFELLAS", "GORGEOUS", 
        "GOSFORD", "GRACELAND", "GRADUATE", "GRAFFITI", "GRAIL", "GRAPES", "GREASE", "GREATEST", "GREEDY", "GREEK", 
        "GRINCH", "GRIT", "GROOVE", "GROSSE", "GROUNDHOG", "GUMP", "GUN", "GUNFIGHT", "GUNFIGHTER", "GUYS", 
        "HALF", "HALL", "HALLOWEEN", "HAMLET", "HANDICAP", "HANGING", "HANKY", "HANOVER", "HAPPINESS", "HARDLY", 
        "HAROLD", "HARPER", "HARRY", "HATE", "HAUNTED", "HAUNTING", "HAWK", "HEAD", "HEARTBREAKERS", "HEAVEN", 
        "HEAVENLY", "HEAVYWEIGHTS", "HEDWIG", "HELLFIGHTERS", "HIGH", "HIGHBALL", "HILLS", "HOBBIT", "HOCUS", "HOLES", 
        "HOLIDAY", "HOLLOW", "HOLLYWOOD", "HOLOCAUST", "HOLY", "HOME", "HOMEWARD", "HOMICIDE", "HONEY", "HOOK", 
        "HOOSIERS", "HOPE", "HORN", "HORROR", "HOTEL", "HOURS", "HOUSE", "HUMAN", "HUNCHBACK", "HUNGER", 
        "HUNTER", "HUNTING", "HURRICANE", "HUSTLER", "HYDE", "HYSTERICAL", "ICE", "IDAHO", "IDENTITY", "IDOLS", 
        "IGBY", "ILLUSION", "IMAGE", "IMPACT", "IMPOSSIBLE", "INCH", "INDEPENDENCE", "INDIAN", "INFORMER", "INNOCENT", 
        "INSECTS", "INSIDER", "INSTINCT", "INTENTIONS", "INTERVIEW", "INTOLERABLE", "INTRIGUE", "INVASION", "IRON", "ISHTAR", 
        "ISLAND", "ITALIAN", "JACKET", "JADE", "JAPANESE", "JASON", "JAWBREAKER", "JAWS", "JEDI", "JEEPERS", 
        "JEKYLL", "JEOPARDY", "JERICHO", "JERK", "JERSEY", "JET", "JINGLE", "JOON", "JUGGLER", "JUMANJI", 
        "JUMPING", "JUNGLE", "KANE", "KARATE", "KENTUCKIAN", "KICK", "KILL", "KILLER", "KING", "KISS", 
        "KISSING", "KNOCK", "KRAMER", "KWAI", "LABYRINTH", "LADY", "LADYBUGS", "LAMBS", "LANGUAGE", "LAWLESS", 
        "LAWRENCE", "LEAGUE", "LEATHERNECKS", "LEBOWSKI", "LEGALLY", "LEGEND", "LESSON", "LIAISONS", "LIBERTY", "LICENSE", 
        "LIES", "LIFE", "LIGHTS", "LION", "LOATHING", "LOCK", "LOLA", "LOLITA", "LONELY", "LORD", 
        "LOSE", "LOSER", "LOST", "LOUISIANA", "LOVE", "LOVELY", "LOVER", "LOVERBOY", "LUCK", "LUCKY", 
        "LUKE", "LUST", "MADIGAN", "MADISON", "MADNESS", "MADRE", "MAGIC", "MAGNIFICENT", "MAGNOLIA", "MAGUIRE", 
        "MAIDEN", "MAJESTIC", "MAKER", "MALKOVICH", "MALLRATS", "MALTESE", "MANCHURIAN", "MANNEQUIN", "MARRIED", "MARS", 
        "MASK", "MASKED", "MASSACRE", "MASSAGE", "MATRIX", "MAUDE", "MEET", "MEMENTO", "MENAGERIE", "MERMAID", 
        "METAL", "METROPOLIS", "MICROCOSMOS", "MIDNIGHT", "MIDSUMMER", "MIGHTY", "MILE", "MILLION", "MINDS", "MINE", 
        "MINORITY", "MIRACLE", "MISSION", "MIXED", "MOB", "MOCKINGBIRD", "MOD", "MODEL", "MODERN", "MONEY", 
        "MONSOON", "MONSTER", "MONTEREY", "MONTEZUMA", "MOON", "MOONSHINE", "MOONWALKER", "MOSQUITO", "MOTHER", "MOTIONS", 
        "MOULIN", "MOURNING", "MOVIE", "MULAN", "MULHOLLAND", "MUMMY", "MUPPET", "MURDER", "MUSCLE", "MUSIC", 
        "MUSKETEERS", "MUSSOLINI", "MYSTIC", "NAME", "NASH", "NATIONAL", "NATURAL", "NECKLACE", "NEIGHBORS", "NEMO", 
        "NETWORK", "NEWSIES", "NEWTON", "NIGHTMARE", "NONE", "NOON", "NORTH", "NORTHWEST", "NOTORIOUS", "NOTTING", 
        "NOVOCAINE", "NUTS", "OCTOBER", "ODDS", "OKLAHOMA", "OLEANDER", "OPEN", "OPERATION", "OPPOSITE", "OPUS", 
        "ORANGE", "ORDER", "ORIENT", "OSCAR", "OTHERS", "OUTBREAK", "OUTFIELD", "OUTLAW", "OZ", "PACIFIC", 
        "PACKER", "PAJAMA", "PANIC", "PANKY", "PANTHER", "PAPI", "PARADISE", "PARIS", "PARK", "PARTY", 
        "PAST", "PATHS", "PATIENT", "PATRIOT", "PATTON", "PAYCHECK", "PEACH", "PEAK", "PEARL", "PELICAN", 
        "PERDITION", "PERFECT", "PERSONAL", "PET", "PHANTOM", "PHILADELPHIA", "PIANIST", "PICKUP", "PILOT", "PINOCCHIO", 
        "PIRATES", "PITTSBURGH", "PITY", "PIZZA", "PLATOON", "PLUTO", "POCUS", "POLISH", "POLLOCK", "POND", 
        "POSEIDON", "POTLUCK", "POTTER", "PREJUDICE", "PRESIDENT", "PRIDE", "PRIMARY", "PRINCESS", "PRIVATE", "PRIX", 
        "PSYCHO", "PULP", "PUNK", "PURE", "PURPLE", "QUEEN", "QUEST", "QUILLS", "RACER", "RAGE", 
        "RAGING", "RAIDERS", "RAINBOW", "RANDOM", "RANGE", "REAP", "REAR", "REBEL", "RECORDS", "REDEMPTION", 
        "REDS", "REEF", "REIGN", "REMEMBER", "REQUIEM", "RESERVOIR", "RESURRECTION", "REUNION", "RIDER", "RIDGEMONT", 
        "RIGHT", "RINGS", "RIVER", "ROAD", "ROBBERS", "ROBBERY", "ROCK", "ROCKETEER", "ROCKY", "ROLLERCOASTER", 
        "ROMAN", "ROOF", "ROOM", "ROOTS", "ROSES", "ROUGE", "ROXANNE", "RUGRATS", "RULES", "RUN", 
        "RUNAWAY", "RUNNER", "RUSH", "RUSHMORE", "SABRINA", "SADDLE", "SAGEBRUSH", "SAINTS", "SALUTE", "SAMURAI", 
        "SANTA", "SASSY", "SATISFACTION", "SATURDAY", "SATURN", "SAVANNAH", "SCALAWAG", "SCARFACE", "SCHOOL", "SCISSORHANDS", 
        "SCORPION", "SEA", "SEABISCUIT", "SEARCHERS", "SEATTLE", "SECRET", "SECRETARY", "SECRETS", "SENSE", "SENSIBILITY", 
        "SEVEN", "SHAKESPEARE", "SHANE", "SHANGHAI", "SHAWSHANK", "SHEPHERD", "SHINING", "SHIP", "SHOCK", "SHOOTIST", 
        "SHOW", "SHREK", "SHRUNK", "SIDE", "SIEGE", "SIERRA", "SILENCE", "SILVERADO", "SIMON", "SINNERS", 
        "SISTER", "SKY", "SLACKER", "SLEEPING", "SLEEPLESS", "SLEEPY", "SLEUTH", "SLING", "SLIPPER", "SLUMS", 
        "SMILE", "SMOKING", "SMOOCHY", "SNATCH", "SNATCHERS", "SNOWMAN", "SOLDIERS", "SOMETHING", "SONG", "SONS", 
        "SORORITY", "SOUP", "SOUTH", "SPARTACUS", "SPEAKEASY", "SPEED", "SPICE", "SPIKING", "SPINAL", "SPIRIT", 
        "SPIRITED", "SPLASH", "SPLENDOR", "SPOILERS", "SPY", "SQUAD", "STAGE", "STAGECOACH", "STALLION", "STAMPEDE", 
        "STAR", "STATE", "STEEL", "STEERS", "STEPMOM", "STING", "STOCK", "STONE", "STORM", "STORY", 
        "STRAIGHT", "STRANGELOVE", "STRANGER", "STRANGERS", "STREAK", "STREETCAR", "STRICTLY", "SUBMARINE", "SUGAR", "SUICIDES", 
        "SUIT", "SUMMER", "SUN", "SUNDANCE", "SUNRISE", "SUNSET", "SUPER", "SUPERFLY", "SUSPECTS", "SWARM", 
        "SWEDEN", "SWEET", "SWEETHEARTS", "TADPOLE", "TALENTED", "TARZAN", "TAXI", "TEEN", "TELEGRAPH", "TELEMARK", 
        "TEMPLE", "TENENBAUMS", "TEQUILA", "TERMINATOR", "TEXAS", "THEORY", "THIEF", "THIN", "TIES", "TIGHTS", 
        "TIMBERLAND", "TITANIC", "TITANS", "TOMATOES", "TOMORROW", "TOOTSIE", "TORQUE", "TOURIST", "TOWERS", "TOWN", 
        "TRACY", "TRADING", "TRAFFIC", "TRAIN", "TRAINSPOTTING", "TRAMP", "TRANSLATION", "TRAP", "TREASURE", "TREATMENT", 
        "TRIP", "TROJAN", "TROOPERS", "TROUBLE", "TRUMAN", "TURN", "TUXEDO", "TWISTED", "TYCOON", "UNBREAKABLE", 
        "UNCUT", "UNDEFEATED", "UNFAITHFUL", "UNFORGIVEN", "UNITED", "UNTOUCHABLES", "UPRISING", "UPTOWN", "USUAL", "VACATION", 
        "VALENTINE", "VALLEY", "VAMPIRE", "VANILLA", "VANISHED", "VANISHING", "VARSITY", "VELVET", "VERTIGO", "VICTORY", 
        "VIDEOTAPE", "VIETNAM", "VILLAIN", "VIRGIN", "VIRGINIAN", "VIRTUAL", "VISION", "VOICE", "VOLCANO", "VOLUME", 
        "VOYAGE", "WAGON", "WAIT", "WAKE", "WALLS", "WANDA", "WAR", "WARDROBE", "WARLOCK", "WARS", 
        "WASH", "WASTELAND", "WATCH", "WATERFRONT", "WATERSHIP", "WEDDING", "WEEKEND", "WEREWOLF", "WEST", "WESTWARD", 
        "WHALE", "WHISPERER", "WIFE", "WILD", "WILLOW", "WIND", "WINDOW", "WISDOM", "WITCHES", "WIZARD", 
        "WOLVES", "WOMEN", "WON", "WONDERFUL", "WONDERLAND", "WONKA", "WORDS", "WORKER", "WORKING", "WORLD", 
        "WORST", "WRATH", "WRONG", "WYOMING", "YENTL", "YOUNG", "YOUTH", "ZHIVAGO", "ZOOLANDER", "ZORRO", 
        };
      title_in = movie_titles[r.Next ( 1000 )] + " " + movie_titles[r.Next ( 1000 )];
      }  // End of CreateTitle   

      string[] InitReviewDataTerms()
    {
          string[] review_data_terms = new string[] 
          {"the","and","a","of","to","is","in","I","that","this","it","for","was","with","as","The","movie","on",
            "but","you","are","have","his","not","film","be","one","by","an","he","from","50","at","all","who",
            "has","like","they","This","so","about","just","or","my","more","out","very","her","some","good",
            "great","It","what","will","when","would","their","can","if","up","really","than","see","had",
            "only","which","its","were","get","been","into","00","story","A","much","there","first","even",
            "no","time","DVD","also","other","she","most","we","love","its","because","how","me","40","best",
            "do","movie","your","many","make","it","dont","well","could","watch","any","people","movies","think",
            "two","The","him","never","over","still","little","Its","then","made","But","being","does","way","them",
            "If","after","did","too","seen","know","where","these","And","better","movie","He","those","In","ever",
            "character","film","should","back","films","characters","find","new","say","scenes","makes","Im","off",
            "through","There","go","such","want","watching","life","11","few","film","original","while","bad","action",
            "going","own","old","same","didnt","real","i","every","years","scene","both","30","version","must","something",
            "lot","doesnt","show","am","plot","before","may","look","between","acting","got","worth","always","take",
            "actually","give","another","Great","cant","man","You","part","end","quite","thought","special","it","I",
            "Ive","our","us","bit","saw","pretty","THE","The","now","why","set","They","things","series","come","John",
            "feel","As","10","down","young","work","seems","without","long","since","each","cast","gets","music","One",
            "thing","around","though","found","fun","big","almost","watched","enough","whole","classic","family","last",
            "recommend","What","My","isnt","comes","right","Not","enjoy","here","actors","far","done","played","nothing",
            "different","video","fan","favorite","funny","loved","times","world","book","excellent","horror","probably",
            "plays","might","quality","When","2","performance","buy","put","true","takes","interesting","For","three",
            "making","especially","looking","role","She","time","job","01","enjoyed","20","We","shows","during","came",
            "thats","kind","wonderful","least","12","again","hard","fact","rather","yet","22","youre","read","trying",
            "away","anyone","This","director","having","sure","need","second","goes","believe","keep","hes","So","play",
            "sound","until","used","American","stars","anything","truly","effects","All","seeing","wasnt","point",
            "looks","along","beautiful","seem","Michael","liked","once","help","left","full","main","getting","perfect",
            "high","gives","picture","nice","use","everything","several","year","3","However","simply","ending",
            "comedy","become","definitely","That","place","top","TV","everyone","entire","said","kids","day","guy",
            "bought","His","fans","less","together","J","After","5","able","screen","sense","someone","highly","human",
            "minutes","wont","live","wanted","release","well","given","features","next","went","Good","While","Movie",
            "already","star","tell","against","understand","youll","based","Of","James","To","himself","and","let",
            "woman","Robert","reason","finally","theres","performances","amazing","time","home","start","girl","review",
            "couple","playing","doing","completely","New","rest","money","With","felt","David","season","small","one",
            "wife","Even","try","all","remember","men","DVD","Hollywood","is","absolutely","certainly","Best","dvd",
            "becomes","war","early","entertaining","story","them","short","actor","story","No","M","often","idea",
            "including","final","mind","half","hope","wants","itself","At","Just","disc","released","course","although",
            "movies","took","that","reviews","black","line","A","23","script","later","good","Id","Some","fine","Mr","me"
            ,"again","children","under","fight","instead","audience","couldnt","OF","friends","production","4","others",
            "me","behind","lost","gave","good","shot","him","turn","person","history","moments","son","Very","greatest",
            "dark","else","called","heard","humor","problem","seemed","order","happy","care","movies","comic","33","wish",
            "cut","past","An","works","the","finds","AND","beginning","commentary","documentary","turns","maybe","name",
            "Tom","father","life","Jack","expect","lives","face","viewer","heart","Christmas","case","death","throughout",
            "side","cannot","out","friend","course","this","parts","taken","films","totally","group","four","either","school",
            "however","daughter","episodes","type","1","important","close","George","starts","havent","brilliant","night",
            "NOT","well","written","Dont","sort","dialogue","huge","coming","women","known","guys","C","extras","musical",
            "complete","this","add","title","stop","all","strong","somewhat","wrong","modern","On","episode","wait","edition",
            "bring","knew","L","English","days","head","extremely","Peter","leave","worst","evil","IS","Theres","style",
            "whose","mother","D","tells","copy","told","great","perhaps","learn","poor","it","lead","Ill","wouldnt","soon",
            "experience","five","stories","taking","matter","Disney","camera","feeling","says","brings","From","supposed",
            "major","brought","looked","How","THIS","due","giving","save","songs","exactly","easy","relationship","number",
            "run","living","If","films","myself","previous","knows","romantic","piece","opening","02","tries","R","cool",
            "a","named","turned","sets","In","movie","mean","guess","hit","personal","upon","Lee","view","Richard","audio",
            "box","here","Although","voice","kill","change","Amazon","begins","hear","attention","across","Love","loves",
            "10","Now","directed","boy","white","too","late","within","Dr","fantastic","laugh","sometimes","Then","DVD",
            "despite","you","certain","battle","thinking","films","needs","particularly","These","price","Will","glad","S",
            "started","fact","serious","house","drama","child","score","car","way","Paul","extra","age","overall","that",
            "itbr","power","lines","theyre","kept","collection","enjoyable","characters","song","single","clear","lots",
            "nearly","one","violence","ends","deal","British","Is","again","themselves","actual","lack","supporting",
            "great","arent","older","yourself","way","working","husband","easily","town","hours","decent","13","youve",
            "Like","end","powerful","tale","using","keeps","Star","end","viewing","dead","happens","saying","art","shes",
            "wonder","William","stuff","Bond","except","is","Harry","awesome","out","simple","footage","TO","waste","usually",
            "events","among","feature","chance","life","call","Oscar","novel","eyes","moment","decided","better","running",
            "series","reading","soundtrack","transfer","Film","fast","scenes","feels","talking","King","him","It","emotional",
            "Why","visual","Its","and","fall","watch","happened","War","disappointed","move","mostly","flick","moviebr",
            "Thats","whether","beyond","killed","here","VHS","realize","E","As","buying","low","became","First","showing",
            "available","seen","interest","acting","stand","34","hilarious","near","hate","moving","Most","game","up",
            "interested","shown","light","animation","slow","difficult","hour","Man","third","message","writing","leaves",
            "filmbr","mention","local","follow","falls","characters","Bruce","surprised","appreciate","future","I","alone",
            "includes","direction","Excellent","body","sequel","funny","involved","added","attempt","Well","sex","which",
            "means","Smith","meets","famous","various","Her","team","character","included","kid","plenty","forward","superb",
            "them","ones","include","said","theme","appears","elements","quickly","Kevin","happen","years","typical","storyline",
            "clearly","44","expected","animated","Bluray","though","unique","hold","above","on","form","missing","act",
            "police","moves","theatrical","roles","A","her","received","There","French","Who","Hes","check","tried","purchased",
            "scary","Another","times","similar","incredible","talk","stay","Also","But","problems","IT","created","level","Japanese",
            "crew","memorable","more","times","obvious","boring","action","eventually","plot","purchase","Jason","waiting","parents",
            "theater","middle","Martin","sad","there","books","forget","miss","recent","filmed","product","Frank","hero","strange",
            "pay","words","obviously","historical","usual","word","portrayal","deep","blood","television","figure","By","perfectly",
            "genre","material","Classic","Bill","particular","minute","Season","Yes","meet","viewers","return","series","K","hand",
            "worked","track","Ray","doubt","basically","say","World","begin","sit","B","filled","create","female","open","20","or",
            "Jim","room","brother","band","solid","remake","expecting","leads","bonus","needed","reality","won","collection","longer",
            "political","Well","fairly","budget","book","Scott","gone","sounds","ago","cinematography","killer","funny","York","girls",
            "be","dance","amount","Maybe","24","whom","etc","fighting","manages","pick","question","none","Black","IN","uses","forced",
            "scenes","Big","made","agree","Also","realistic","example","large","de","presented","rent","up","work","Steve","shots","Chris",
            "suspense","version","stupid","show","stage","inside","career","period","you","Ben","former","towards","world","telling",
            "fit","killing","Mark","on","portrayed","contains","possible","ways","follows","murder","dramatic","music","terrible","present",
            "character","Jones","color","match","recently","best","younger","as","addition","versions","quotThe","sequences","outstanding",
            "thriller","deleted","And","scifi","Tim","nature","pure","crime","standard","familiar","actress","cover","deserves","adds",
            "popular","sequence","country","class","earlier","terrific","Sam","aspect","admit","whos","whats","slightly","sent","years",
            "romance","screen","Johnny","Joe","Van","reviewers","epic","consider","caught","free","Christopher","MOVIE","movies","God",
            "front","focus","science","background","nor","entertainment","changed","today","meant","singing","city","30","following",
            "who","hands","decides","scene","But","talent","however","amp","information","horrible","Both","beautifully","violent",
            "average","surprise","compared","book","DVDs","Mary","sees","in","subject","likely","missed","rich","points","there",
            "likes","write","details","possibly","space","Bob","choice","cast","exciting","Special","plus","incredibly","imagine",
            "rare","ultimately","ask","interviews","Charles","classic","leading","break","starring","silly","appear","adventure",
            "social","I","sexual","fantasy","provides","ability","herself","opinion","members","stars","biggest","fully","pThe",
            "language","fan","ten","remains","US","Its","T","45","effort","eye","Many","outside","depth","masterpiece","White",
            "Watch","concert","result","straight","spent","Two","offers","image","workout","widescreen","Batman","G","list",
            "military","P","cinematic","beauty","basic","BluRay","a","truth","alien","believable","together","spend","German",
            "escape","edge","setting","mystery","wrote","share","runs","much","gore","Though","Every","detail","Stephen","player",
            "Only","Thomas","drug","issues","win","company","total","bunch","done","bad","changes","somehow","considered","Do",
            "computer","fun","helps","cute","twist","55","man","chemistry","puts","people","adaptation","ended","zombie","version",
            "So","worse","considering","stunning","terms","Wayne","stands","secret","holds","decide","adult","Dead","produced","anime",
            "love","talented","comedy","family","crazy","now","dont","fun","effect","gift","hell","minor","effects","plot","intense",
            "Daniel","entirely","Director","ordered","Little","America","critics","public","Story","role","Day","people","sister","vs",
            "her","rock","Mike","showed","laughing","in","did","premise","6","business","work","questions","worthy","delivers","plan",
            "rate","complex","control","originally","casting","cause","please","Once","catch","funniest","not","GREAT","further","Their",
            "general","W","Too","writer","Perhaps","actors","twists","respect","thinks","studio","Christian","show","cop","fear","now",
            "images","Brian","Robin","screenplay","Tony","leaving","impressive","die","hoping","successful","pull","cinema","creepy",
            "married","provide","More","Steven","monster","YOU","alot","key","male","attempts","boys","jokes","clever","HD","ALL","success",
            "bad","suggest","brief","Henry","Edward","continue","do","negative","society","Unfortunately","finding","immediately","listen",
            "touch","LOVE","annoying","western","rating","day","Billy","becoming","knowing","common","ending","quick","Sean","performance","writers","Trek","Anthony","sweet","hot","impressed","too","unless","subtle","journey","Academy","unlike","money","concept","apparently","explain","hardly","fascinating","weak","ever","scene","Edition","notice","cult","Or","ride","digital","FOR","tough","Red","world","15","graphic","Night","35","CGI","note","development","whatever","played","off","dream","Jeff","disappointed","government","effective","era","onto","wanting","51","off","mentioned","bluray","aspects","about","intelligent","the","mysterious","werent","03","first","chase","over","involving","situation","Jane","fiction","directors","literally","mix","Other","value","vampire","though","plain","master","fell","comedy","see","Jennifer","martial","force","thrown","arrived","know","ready","speak","THAT","cast","Williams","14","Time","hasnt","meaning","tired","discover","H","Ryan","magic","current","Highly","disturbing","moral","cheap","excellent","led","Charlie","touching","Bad","City","trailer","music","adults","De","What","train","For","atmosphere","today","ultimate","months","helped","learned","A","to","was","odd","difference","Last","II","credits","state","mixed","super","away","Since","creative","barely","held","seriously","office","physical","equally","grew","So","credit","opinion","so","step","Captain","tension","day","willing","not","Superman","regular","All","humor","directors","continues","ship","were","was","acting","Still","college","in","original","baby","Wonderful","Because","finest","spirit","inspired","entertaining","gang","then","WAS","otherwise","Buy","Patrick","reasons","trouble","memories","trip","Jr","Get","normal","Despite","thoroughly","mood","lose","Moore","wonderfully","tape","generally","action","growing","comedic","pace","todays","natural","feelings","extended","superior","indeed","acted","Blu","paid","videos","print","lived","surprisingly","potential","before","search","tone","absolute","laughs","either","moved","rented","compelling","Matt","weird","died","impossible","drawn","girlfriend","cheesy","themes","VERY","original","six","interview","gay","recommended","Dark","dog","The","compare","Danny","race","Having","Still","presence","positive","love","ideas","motion","viewed","80s","Thank","thing","awful","lovely","3D","scenery","content","Western","pop","seemingly","job","One","tv","reviewer","Alan","thanks","putting","culture","walk","Jackson","air","itself","featuring","party","fresh","filmmakers","case","slowly","yes","example","emotions","subtitles","teen","store","dancing","opportunity","charming","pleased","understanding","but","support","directing","predictable","Eric","Max","Gary","suddenly","set","editing","rated","People","appeal","Roger","Those","humans","asked","family","gotten","discovers","offer","release","Eastwood","with","role","captured","proves","ago","attack","convincing","better","excited","fan","B","Oh","seasons","DVDs","stuck","captures","own","Be","gun","genius","before","allowed","producers","allow","Better","Your","keeping","million","other","history","smart","ridiculous","toward","appearance","Was","Where","8","56","Everyone","experience","Hitchcock","Youll","Here","water","ahead","impact","pass","hits","followed","Ed","sitting","pieces","Can","Each","for","soldiers","Old","Have","Ford","wondering","wife","thus","picked","revenge","self","returns","numerous","creating","soul","place","building","merely","best","constantly","twice","others","My","format","audiences","long","heavy","Joseph","amazing","lets","avoid","Everything","bringing","shame","issue","not","Howard","date","beat","interesting","ago","clean","US","gorgeous","personally","realized","double","much","Elizabeth","man","stick","youd","Nothing","allows","Potter","colors","capture","laughed","ray","innocent","aware","Fun","year","Clint","cold","House","ON","stars","news","later","fellow","apart","neither","students","Adam","effects","system","giant","right","childhood","Much","silent","managed","fair","tragic","High","West","wrong","falling","down","fire","Fox","struggle","quality","born","video","Stone","Don","week","process","red","hair","7","done","part","And","F","reminded","wild","South","arts","agent","prefer","Grant","relate","Overall","began","learns","fails","enough","Green","deeply","actors","See","poorly","lacks","spectacular","Warner","made","director","sorry","watch","kills","do","hand","drive","introduced","places","Gene","Matthew","reminds","multiple","flick","Mrs","serial","turning","accurate","approach","Russell","summer","Never","Burton","vision","religious","packed","discs","starting","ones","Now","Lord","limited","flying","brothers","comedies","timebr","Life","filming","be","Jerry","interesting","comments","Earth","memory","grow","cartoon","with","answer","Halloween","12","25","planet","carry","Sarah","individual","developed","rarely","source","Mel","mans","Awesome","walking","Evil","emotion","watching","prison","forces","afraid","performance","When","villain","treat","back","Nick","However","leader","Dennis","century","sick","condition","intended","ending","a","remarkable","yet","Russian","asks","himself","Part","Alex","throw","honest","failed","itself","shooting","script","visually","noticed","mainly","drama","expectations","United","bother","wide","alive","presentation","loving","thing","technical","Before","joy","winning","Queen","choose","build","member","hearing","street","Kate","BEST","loud","mission","food","genre","Rock","package","marriage","opens","steals","road","suppose","learning","bored","so","land","psychological","children","NO","element","test","Eddie","together","Miss","Allen","costumes","acts","Italian","energy","Horror","violence","justice","minutes","Andrew","66","Anne","Brad","appeared","service","portrays","folks","brutal","realizes","shouldnt","point","gonna","dealing","The","46","dreams","traditional","blue","met","strength","excellent","accept","pleasure","charm","prove","lets","Alien","purpose","classic","screen","ONE","Go","blu","mind","destroy","artistic","grown","faces","else","wellbr","more","describe","study","Loved","players","treatment","utterly","finish","sexy","honestly","teenage","Depp","surprising","Brown","Did","additional","Dolby","performances","la","are","Taylor","relationships","professional","actions","breaks","short","Instead","moments","exception","Shes","technology","beautiful","WITH","emotionally","release","necessary","engaging","deliver","entertaining","zombies","sharp","Worth","Kelly","days","home","Wilson","situations","Spanish","visuals","latest","design","N","About","seat","pulled","deserved","theatre","direct","lady","legendary","hidden","classics","creates","humor","sing","names","fans","hadnt","provided","hated","pictures","theaters","Being","Julie","wasted","Johnson","Anderson","shoot","Digital","detective","fights","delivered","student","bigger","focuses","loses","Me","BUT","first","loss","overly","kinda","spot","remain","visit","Chinese","HAVE","Yet","door","father","actor","While","nicely","9","Julia","camp","enjoying","earth","Three","treated","deals","Blue","performances","other","calls","sheer","Americans","Davis","talks","Definitely","capable","Video","SO","point","brilliantly","jump","Brothers","remembered","mean","knowledge","believes","presents","central","Arthur","delightful","heroes","Let","disappointing","comment","picture","program","Music","lies","40","Our","releases","Jimmy","Guy","item","law","mental","brain","around","extreme","minutes","desire","restored","Joan","Criterion","bloody","Paris","send","Jackie","insight","scared","aliens","mom","pain","combination","travel","Are","broken","decision","bizarre","weeks","sell","back","hopes","R","Watching","doctor","Ron","50","Funny","2nd","unusual","grand","deeper","finished","adding","dangerous","constant","like","tears","nominated","disk","Prince","event","perfect","Anyone","Tommy","Arnold","values","friends","heads","somewhere","lacking","regret","genuine","conflict","teacher","definately","Douglas","Jean","rescue","discovered","it","Miller","have","Dan","Really","fairy","kick","children","Stewart","prior","Jesus","job","others","himself","ancient","Walter","intriguing","featured","Cruise","explains","history","Family","dull","audience","training","army","quiet","impression","losing","dialogue","reach","essentially","ground","involves","flat","Al","film","Picture","cost","loose","hurt","everybody","amusing","Fred","Vincent","length","Alice","range","magnificent","Review","standing","complaint","plane","seven","trust","Without","greater","narrative","price","Young","enough","Wars","personality","Andy","dying","seen","survive","appropriate","Hanks","initial","Movies","tend","100","Overall","thank","generation","Rob","MY","perspective","pulls","to","weve","relatively","67","dialog","closer","thats","Rocky","son","witty","meeting","unexpected","introduction","protect","producer","bits","dumb","driving","island","higher","lover","built","13","kids","edited","North","magical","set","photography","recommended","taste","foreign","Ms","Im","tour","bottom","cry","old","friend","favor","money","flaws","characters","surely","confused","thembr","plans","mad","faith","author","blown","us","death","You","aside","results","critical","dark","through","forgotten","fabulous","Oh","wearing","Lost","night","Would","flicks","who","passed","location","dad","unable","roles","majority","friendship","happening","Whats","frame","London","creature","alternate","previously","humanity","Thanks","old","MUST","night","Cant","fits","creatures","Stanley","dvd","portraying","Beautiful","skip","lesson","Ever","if","Michelle","machine","Sound","CD","gem","boss","Lets","Harris","Lewis","trailers","Amazing","Lady","with","Universal","heart","passion","mind","responsible","sat","project","Larry","shock","asking","wrong","surround","bright","lovers","Back","entertainment","Oliver","boring","forever","Fantastic","sequels","sides","GOOD","section","listening","great","animals","apparent","shocking","over","voices","steal","15","condition","artist","Cameron","True","away","serves","conclusion","mess","slasher","speaking","packaging","right","numbers","Sure","opera","inner","friends","balance","Clark","radio","Pretty","kids","till","unknown","twenty","Texas","things","Indian","haunting","record","holding","Bourne","mistake","favorites","Hugh","manage","manner","Lawrence","lame","girl","moments","ok","Morgan","center","Barbara","described","war","Kim","fake","significant","connection","part","unfortunately","officer","tragedy","novel","for","strongly","saved","fill","rise","performed","placed","media","badly","timeless","Linda","bar","Anyway","theyve","Willis","deserve","down","Hard","satisfying","suspect","home","Which","regarding","humorous","efforts","awesome","Kubrick","andor","Spielberg","although","Unlike","affair","San","driven","cuts","warm","features","Dean","Jonathan","fans","pair","noir","quality","the","Rachel","chose","join","wouldve","experienced","Long","cameo","see","Instead","phone","everyone","today","Death","mine","Vietnam","draw","extras","boyfriend","contrast","Los","greatly","Does","Series","70s","nobody","whenever","area","Jessica","Award","proper","comparison","20th","largely","enjoy","season","wonderful","Susan","Must","victim","faithful","alltime","eat","carries","Lots","Please","imagination","features","Over","makers","especially","reveal","sense","damn","spoil","Washington","storybr","skills","video","Helen","Any","Tarantino","fate","virtually","recognize","via","criminal","genuinely","death","masterpiece","disappointment","portray","mother","determined","offered","okay","like","beloved","powers","04","words","games","war","supposedly","36","opinion","Along","History","Woody","teach","holiday","Girl","franchise","though","long","latter","are","facts","Action","Terry","REALLY","around","Overall","Rick","General","Nazi","correct","but","extras","didnt","reveals","types","Roman","develop","focused","evidence","climax","twisted","FBI","separate","line","delivery","Alexander","award","Nice","2","stuff","actor","actresses","ARE","ghost","confusing","horse","Civil","England","flick","fashion","Planet","blow","speaks","references","partner","painful","Collection","refreshing","future","trilogy","cant","anybody","extraordinary","enjoys","things","script","prepared","enjoyable","lives","accent","1st","To","Craig","desperate","Show","oh","private","mediocre","reviews","purchasing","green","nuclear","amazed","movie","Roy","reason","ordinary","ruin","experiences","serve","guy","cross","hilarious","J","depiction","core","helping","disaster","77","of","woman","saving","OK","57","pacing","drama","did","himbr","scare","President","encounter","Golden","year","During","sold","nowhere","like","know","ran","ages","cars","talents","really","viewing","lower","wedding","necessarily","Home","Jamie","2","designed","normally","copies","Harrison","joke","After","shines","Living","exact","covered","variety","owner","streets","forth","animal","figured","opposite","crowd","Take","selling","victims","count","caused","nudity","pleasant","hired","us","struggles","monsters","revealed","Comedy","suffers","cops","concerned","once","mark","AS","Finally","sight","Roberts","On","Hill","walks","Holmes","required","quest","marvelous","thoughts","tracks","TV","praise","claim","does","Santa","display","trapped","Out","raised","European","tribute","Look","pleasantly","apartment","smile","paced","DVD","wonderful","dated","Book","line","safe","changing","disc","reaction","vs","Perfect","speech","below","touches","returned","Alfred","about","even","Directors","sign","directly","sports","Doctor","another","families","We","convinced","Unfortunately","DO","quirky","picture","Dracula","underrated","fantastic","picks","place","yoga","brand","genre","breaking","Disc","contemporary","paying","clips","ever","overthetop","Nicholas","chosen","featurette","hotel","That","Ian","Cage","angry","receive","depicted","fly","superb","attractive","produce","complicated","handle","side","intelligence","spy","deadly","recorded","stated","cinematography","Now","No","dollars","fathers","weight","daughter","glimpse","Nicholson","shocked","volume","Simon","heavily","Donald","With","essential","25","themselves","hospital","therefore","exceptional","causes","battles","Set","Joel","crap","Baby","favourite","witness","anyway","Top","own","May","BE","think","spite","research","different","pI","larger","contain","recall","equal","cat","Audio","remastered","gold","Ann","Anniversary","superhero","massive","Murphy","plots","Army","including","description","besides","direction","mere","attitude","Sir","Live","exercise","account","choices","Damon","titles","stopped","ruined","spends","Master","bank","repeated","obsessed","suspenseful","Parker","dies","kinds","frightening","roll","Broadway","excuse","watches","judge","books","sympathetic","Sometimes","Pitt","stellar","Lucy","influence","shape","perform","suffering","days","real","documentaries","production","challenge","Saturday","arrives","case","Anna","stays","house","served","detailed","legend","lessons","alone","replaced","Irish","nasty","romance","independent","murdered","claims","popcorn","First","expected","segment","surrounding","gangster","debut","fourth","opened","Jake","enemy","Matrix","community","wear","initially","mebr","least","yes","accident","Especially","of","eating","Gibson","darker","explained","However","Could","cutting","Princess","mob","searching","king","Friday","covers","mature","Based","Hopkins","perfect","levels","and","marry","church","decades","villains","house","behavior","wife","Original","lifebr","style","2","village","Iron","suspense","succeeds","realism","Things","Yes","style","terribly","two","rules","unbelievable","purchase","26","struggling","drop","thrilling","beautiful","Dick","Up","breath","views","Leonard","Absolutely","Cary","WWII","childrens","related","African","ensemble","time","gory","true","Catherine","scientist","sleep","practically","else","seller","per","entertained","sound","career","poignant","1","fault","Barry","anything","Elvis","appreciated","month","highest","Carol","chick","for","Gordon","Something","industry","occasionally","Simply","Dawn","steps","men","Godzilla","blame","Dave","bed","fail","grows","Laura","Maria","guns","opposed","assume","onebr","position","gritty","owned","u","seeking","freedom","at","desert","Indiana","football","Colin","Carl","lucky","Connery","couldve","Full","curious","explanation","tons","eyes","boring","everyday","satire","remind","Men","anywhere","identity","carried","sadly","unforgettable","MGM","sense","11","Again","amazing","random","stayed","Samuel","Kirk","rights","Street","tremendous","wealthy","Such","Space","Lucas","charge","cable","veteran","wise","winner","later","post","baseball","Jewish","feet","Jon","commercial","combined","Zombie","holes","colorful","rental","tight","facial","Some","complain","installment","yet","proud","Theyre","Wild","ways","once","happen","highlight","blind","closing","Saw","SciFi","seek","AT","dialogue","station","ball","works","heck","cultural","Amy","says","Harvey","Ken","lighting","enter","age","Die","round","drugs","dubbed","Amazon","dirty","Happy","season","commentary","review","destroyed","past","delight","ring","90","past","pilot","Snow","At","guilty","supernatural","BUY","possible","child","pThis","seconds","waited","arms","review","flow","Jeffrey","letting","78","M","score","Park","sound","I","Sandra","Not","WILL","restoration","unfortunate","FROM","understood","teens","routine","promise","go","definite","advantage","Owen","laugh","upset","exist","Jay","sensitive","essence","play","Kong","soldier","library","providing","believable","thrilled","roles","released","unlikely","Carrey","believed","reference","violence","women","thin","option","Lisa","Louis","costume","weapons","context","Had","chapter","notch","beginning","sex","Angel","school","youth","two","Terminator","field","DTS","effectively","States","ice","ways","Way","answers","Next","Greek","builds","revolves","boat","vampires","drag","Rose","guitar","chilling","documentary","sequel","drives","againbr","dozen","Almost","bear","School","real","tradition","specific","returning","morning","throws","Ghost","floor","primarily","path","Mickey","which","far","Wood","allowing","edition","Karen","suit","empty","repeat","matters","blend","sends","calling","favorites","artists","47","Bobby","formula","album","improved","touched","crafted","welcome","child","crisp","ugly","ranks","sea","guy","allbr","homage","throughout","handsome","star","contact","thebr","sings","intensity","achieve","walked","terror","Dragon","currently","cinema","remaining","Sure","reviews","novel","bond","advice","endless","satisfied","speed","countless","window","lights","Scorsese","committed","slight","recording","excitement","heart","decade","campy","lot","Then","SEE","informative","something","adapted","tiny","convince","Romero","hundreds","Jeremy","worthwhile","teaches","3rd","Neil","soundtrack","Ultimate","haunted","pointless","saves","raw","finale","face","overcome","TV","school","courage","gain","birth","spiritual","movement","fictional","material","attention","occasional","Okay","mainstream","director","attempting","passing","menu","thousands","proved","myself","hide","say","hype","mouth","Real","push","Philip","cool","waybr","hanging","par","conversation","Final","So","danger","Quentin","status","differences","Super","gotta","father","enjoyment","EVER","importance","singer","bet","minds","board","DVDbr","stops","The","eight","Fans","Pacino","level","thatbr","episodes","Disneys","again","handled","it","vast","techniques","adventures","stood","Ted","Give","80s","wall","Denzel","thriller","surface","Victor","hundred","Whether","crash","murders","lonely","Kane","reminiscent","La","insane","Films","somebody","he","abandoned","infamous","You","Four","inspiring","audience","instance","writing","amazingly","go","gore","destruction","awkward","loaded","outbr","son","episodes","aside","closely","peace","John","according","format","daily","Carter","NEVER","honor","Probably","interpretation","Entertainment","robot","Cooper","downright","cares","herebr","parts","easier","stock","soft","narration","broke","Mr","notes","enhanced","Right","production","Plus","picking","Through","brilliant","Movie","very","treasure","faced","pack","expensive","sets","same","episode","identify","Hall","Annie","game","flawed","troubled","teaching","forgot","Bogart","Come","CIA","boy","politics","model","National","Five","stories","Cut","medical","corny","Brooks","IF","sorts","anyone","3","skin","standards","parody","raise","Art","ONLY","XMen","response","reason","fine","Days","anymore","FILM","tear","theater","struck","im","Finally","international","peoples","suffer","songs","instead","BBC","torture","Chuck","harsh","releasing","think","funnier","renting","Interesting","draws","refuses","Freeman","horror","belief","Campbell","sequel","Timothy","Check","also","terrifying","wins","Yet","Gregory","horrific","unnecessary","practice","LOVED","husband","states","security","en","OK","grace","sinister","filmmaking","anything","evening","relief","Upon","que","carefully","wit","Albert","come","wont","advanced","rough","target","Boy","desperately","California","episode","fallen","sounded","buy","Complete","vehicle","bomb","goodbr","thru","Im","another","Curtis","mindless","comics","Blood","suicide","sure","Foster","yourself","listed","filmmaker","LIKE","channel","movie","alone","storyline","improvement","Reeves","existence","figures","term","brother","gripping","naked","paper","remote","Kurt","teenagers","girl","aint","dressed","Crowe","Hunter","masterful","convey","Hepburn","aged","criticism","Josh","locations","planning","bodies","makeup","one","degree","cartoons","III","Nicole","regardless","millions","soundtrack","matter","parts","entertain","art","spending","combat","statement","appealing","68","subsequent","corporate","Shirley","Mexican","ignore","studios","removed","Hope","Hoffman","Seven","cases","Shakespeare","dry","problem","Diane","club","brilliant","does","profound","head","written","Murray","represents","neat","Thompson","Hitchcocks","guessing","Emma","88","amazon","dinner","then","market","plastic","Greatest","V","qualities","birthday","matches","authentic","cool","suffered","depressing","Make","starred","goofy","WHAT","storyline","too","hopefully","challenging","mass","thrill","dropped","Fan","corrupt","false","Judy","Mad","lesser","library","town","hours","fought","notable","split","instantly","hooked","true","commentaries","film","talked","likeable","individuals","entry","creation","clothes","I","Austin","Has","JUST","lawyer","encounters","documentary","successfully","grab","fine","anniversary","device","simple","eyes","laugh","young","movie","executed","redeeming","devoted","slow","men","rid","risk","westerns","flashbacks","noted","while","person","Chicago","god","classical","disc","woman","stolen","thisbr","Cold","Chan","ever","frequently","X","host","task","ages","superb","REAL","worry","hiding","ones","arrive","tales","sake","replace","star","far","sequences","why","Predator","highlights","hero","pregnant","timely","Leslie","earned","Another","surprises","instant","have","factor","Of","Vampire","Adams","teenager","jumps","appreciation","Japan","Fast","established","Sandler","stuff","reporter","interaction","shallow","Carpenter","Got","expert","Pixar","overall","Grace","experience","play","aired","threat","UK","Ralph","toobr","Tyler","age","prevent","dynamic","Harold","Welles","portion","breathtaking","daughter","warning","busy","press","Blade","animation","trash","pathetic","cruel","requires","tied","taught","Entertaining","hilarious","endbr","Hannibal","Favorite","dislike","collection","Freddy","caring","60s","language","everyone","survivors","versus","Ross","stories","base","sudden","eerie","sword","Keanu","underground","checking","newer","Collectors","Man","crying","golden","Tracy","offering","ladies","develops","novels","page","no","tense","likable","continued","throwing","Maggie","failure","37","player","Stars","problems","YOUR","spots","Price","garbage","belongs","displays","revealing","moviesbr","storytelling","musicals","Bay","amongst","side","Myers","escapes","Plus","sum","miles","thriller","game","examples","scale","seeks","person","protagonist","Walt","bothered","reality","Coen","always","books","rival","flaw","centers","discuss","rule","song","primary","graphics","utter","Although","wears","Meanwhile","inspiration","imagery","definition","100","Del","era","Professor","newly","goal","letter","shark","shows","St","inevitable","flight","Truly","Lane","slapstick","copy","Emily","site","friend","discussion","humour","Shrek","Ellen","explore","horribly","explaining","rape","basis","Natalie","riding","scary","town","non","dead","priest","Kubricks","reality","16","Making","Bryan","product","addition","upper","comfortable","Its","Road","material","shipping","Poor","technique","one","increasingly","friendly","stretch","hang","Father","of","circumstances","16","America","universe","everyones","luck","intellectual","shot","date","Robinson","Jet","historically","everything","jumping","evil","happened","lines","relevant","segments","argue","period","pointed","Marie","solve","hoped","wrapped","cash","prime","complaints","spoken","native","metal","definitive","Catholic","religion","region","new","kid","interest","you","Burtons","THEY","next","hed","Keaton","rolling","theater","Even","many","transformation","honest","screaming","Made","sophisticated","blood","outstanding","reallife","Francis","Battle","Reynolds","Li","predictable","express","flesh","What","survival","brave","stunts","portrait","drinking","Baker","fat","reasonable","causing","performing","this","shot","HBO","guest","albeit","kicks","Kill","Island","05","foot","ratio","guys","women","nice","Nancy","score","blows","aging","innocence","Add","Theres","wind","Private","Turner","perfectly","mother","Legend","awards","GET","Quite","grade","usual","Jesse","Luke","surreal","disappointment","vivid","quote","carrying","anger","Hunt","boxing","evident","size","hunt","Keith","weekend","gruesome","contained","br","14","terrible","seriously","Audrey","Kenneth","Clooney","wondered","Seeing","Heston","laid","fame","89","look","Century","ive","depicts","D","ruthless","sad","accidentally","Should","while","happily","Paramount","wooden","heroic","Hong","clues","bound","benefit","maintain","Costner","Law","worlds","controversial","physically","smaller","songs","workout","Duke","Von","Norman","bit","C","Dirty","Second","career","Hulk","Hell","doesnt","18","scientific","explores","stronger","chief","ourselves","online","specifically","shop","society","Twilight","note","court","Think","entertainment","vintage","costumes","thirty","grain","watching","cynical","HE","dedicated","attacked","tends","favorite","Baldwin","Europe","isbr","introduces","previews","sympathy","angles","health","reputation","casual","expressions","Newman","June","english","clue","art","Heres","Asian","screenwriter","45","anamorphic","shared","watchable","glorious","Soviet","Niro","morebr","staying","None","torn","wake","attempted","HIS","Las","root","rating","engaged","different","country","skill","socalled","underlying","loyal","Marilyn","Randy","Then","bucks","worse","comical","Angelina","careful","name","connected","Great","Movie","accomplished","no","flawless","Third","Benjamin","trained","Moon","title","backdrop","thumbs","shut","beings","BD","wanna","Patricia","onbr","messages","resolution","p","photo","youbr","Brilliant","obsession","Science","OUT","truck","East","strike","Fiction","lacked","challenges","ride","24","captain","theyll","Todd","im","Movie","viewer","recomend","capturing","hollywood","Wallace","dollar","Graham","Remember","Rodriguez","customer","attacks","Secret","substance","drunk","He","rendition","Nelson","reaches","released","Kids","this","improve","Travolta","Marshall","scary","mentally","overlooked","halfway","acted","Lynch","concerns","pushed","enters","con","info","moment","Angela","y","Brandon","subtitles","crude","gentle","thousand","AN","France","buddy","threw","kid","face","PG13","this","Wes","uncomfortable","Youre","worried","arm","El","filmsbr","perfection","signs","riveting","fears","wishes","Rogers","disagree","Meryl","hint","Phil","blockbuster","execution","Vegas","masterpiece","Actor","handful","table","fantastic","heroine","unrealistic","that","gratuitous","financial","striking","situation","direction","Alec","Toy","theyd","pays","Glenn","eccentric","naturally","through","Americas","L","along","Yeah","adventure","edition","areas","legal","laughs","dvd","Buffy","ANY","demands","anymore","Quality","player","headed","gags","admire","hours","Ive","chasing","dead","Amanda","punch","themselves","superbly","it","worse","piano","Burt","matter","transfer","harder","Ethan","OR","silver","fond","pulling","darn","kidnapped","Streep","consequences","bus","forever","Air","Masterpiece","dress","liking","directorial","Mexico","sisters","lines","stealing","national","least","deaths","Ridley","Drew","passionate","Mom","absurd","groups","fun","also","intelligent","Stallone","shine","believing","brilliance","gross","name","THE","Meg","grainy","Count","moment","cliche","WAY","well","future","Am","personalities","dogs","criminals","Lees","environment","enormous","yeah","Hollywood","urban","Given","bridge","USA","professor","timing","upcoming","laughter","fitting","mountain","surrounded","sucked","ripped","official","lives","Greg","workouts","Crystal","Claire","contrived","available","selection","Christmas","hardcore","remarkably","lie","tad","DONT","finish","Blair","evil","anyway","american","facing","Andrews","decisions","exchange","myself","arguably","beginning","candy","dvds","locked","film","BIG","structure","strikes","LA","orders","Years","Kennedy","stole","spoof","bitter","budget","civil","admit","60","darkness","Save","TIME","suspense","gradually","hitting","reccomend","to","tale","antics","get","fix","Aliens","Colonel","America","appearances","critic","oil","Later","connect","von","Dont","typically","right","importantly","Boys","accused","God","associated","Sadly","idea","beneath","911","1","helpful","Val","Christ","developing","works","Holly","shipped","mid","nonstop","intent","58","wonders","jobs","access","They","settings","code","cell","except","Lois","sacrifice","princess","demonstrates","lowbudget","drink","Sharon","regard","Also","3","beats","Southern","sure","Hollywoods","Lloyd","heres","Doc","delivering","Sherlock","had","sister","Perry","burning","anyway","mothers","exists","list","secrets","herbr","scares","concerning","spoiled","blew","dare","Betty","The","17","ballet","Side","Monty","movements","soap","worn","Besides","suggests","purely","mask","traveling","creators","chance","WHO","Hughes","Eastwoods","fan","strong","4","Sally","price","Bottom","Vince","Uncle","insult","dragged","Angeles","betterbr","Raymond","outrageous","best","Marvel","whereas","reviewing","involved","performers","rely","English","porn","Beatles","horror","glass","alive","closed","card","27","form","Keep"
          };
        return review_data_terms;
    }

    string CreateReviewData(ref String[] review_data_terms, int num_terms)
    {
        string return_string = "";

        for (int i=0; i < num_terms; i++)
        {
            return_string = return_string + " " + review_data_terms[r.Next(4900)];
        }
        return (return_string);
    }  // End of CreateReviewData
          
    } // End of Class User

  } // End of Namespace ds2xdriver



