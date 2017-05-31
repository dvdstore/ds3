
/*
 * DVD Store 3 SQL Server Functions - ds3sqlserverfns.cs
 *
 * Copyright (C) 2005 Dell, Inc. <djaffe@vmware.com> and <tmuirhead@vmware.com>
 *
 * Provides interface functions for DVD Store driver program ds3xdriver.cs
 * See ds3xdriver.cs for compilation and syntax
 *
 * Last Updated 11/28/07
 * Last Updated 6/14/2010 by GSK 
 * Last updated 6/29/2010 by GSK for SQL query parameterization
 * Last Updated 11/18/2015 updated from ds2 to ds3 
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
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;


namespace ds2xdriver
  {
  /// <summary>
  /// ds3sqlserverfns.cs: DVD Store 3 SQL Server Functions
  /// </summary>
  public class ds2Interface
    {
#if (USE_WIN32_TIMER)
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceCounter(ref long x);
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceFrequency(ref long x);  
#endif

    int ds2Interfaceid;
    string target_server;       //Added by GSK
    SqlConnection objConn;
    SqlCommand Login, New_Customer, Browse_By_Category, Browse_By_Actor, Browse_By_Title, Purchase;
    SqlCommand Get_Prod_Reviews, Get_Prod_Reviews_By_Actor, Get_Prod_Reviews_By_Title, Get_Prod_Reviews_By_Date, Get_Prod_Reviews_By_Stars;
    SqlCommand New_Member, New_Prod_Review, New_Review_Helpfulness;
    SqlDataReader Rdr;

//
//-------------------------------------------------------------------------------------------------
// 
    public ds2Interface(int ds2interfaceid)
      {
      ds2Interfaceid = ds2interfaceid;
      //Console.WriteLine("ds2Interface {0} created", ds2Interfaceid);
      }
//
//-------------------------------------------------------------------------------------------------
// 
    //Added by GSK for passing target DB Server / Web server name for connecting
    public ds2Interface ( int ds2interfaceid , string target_server_name)
        {
        ds2Interfaceid = ds2interfaceid;
        target_server = target_server_name;
        //Console.WriteLine("ds2Interface {0} created", ds2Interfaceid);
        }
//
//-------------------------------------------------------------------------------------------------
// 
    public  bool ds2initialize()
      {
      return(true);
      } // end ds2initialize()
 
//
//-------------------------------------------------------------------------------------------------
//  
    public bool ds2connect()
      {
      // Add Password=xxx to sConnectionString if password is set
      //string sConnectionString = "User ID=sa;Initial Catalog=DS3;Connection Timeout=120;Data Source=" + Controller.target;
      //Changed by GSK (added new user ds3user and new server to connect everytime)
      string sConnectionString = "User ID=ds3user;Initial Catalog=DS3;Connection Timeout=120;Data Source=" + target_server;
      try
        {
        objConn = new SqlConnection(sConnectionString);
        objConn.Open();
        }
      catch (SqlException e)
        {
        //Console.WriteLine("Thread {0}: error in connecting to database {1}: {2}",  Thread.CurrentThread.Name,
        //  Controller.target, e.Message);
        //Changed by GSK
        Console.WriteLine ( "Thread {0}: error in connecting to database {1}: {2}" , Thread.CurrentThread.Name ,
        target_server , e.Message );
        return(false);
        }

      // Set up SQL stored procedure calls and associated parameters
      Login = new SqlCommand("LOGIN", objConn);
      Login.CommandType = CommandType.StoredProcedure;
      Login.Parameters.Add("@username_in", SqlDbType.VarChar, 50);
      Login.Parameters.Add("@password_in", SqlDbType.VarChar, 50);
      
      New_Customer = new SqlCommand("NEW_CUSTOMER", objConn);
      New_Customer.CommandType = CommandType.StoredProcedure; 
      New_Customer.Parameters.Add("@username_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@password_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@firstname_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@lastname_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@address1_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@address2_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@city_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@state_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@zip_in", SqlDbType.Int);
      New_Customer.Parameters.Add("@country_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@region_in", SqlDbType.TinyInt);
      New_Customer.Parameters.Add("@email_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@phone_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@creditcardtype_in", SqlDbType.TinyInt);
      New_Customer.Parameters.Add("@creditcard_in", SqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("@creditcardexpiration_in", SqlDbType.VarChar, 50); 
      New_Customer.Parameters.Add("@age_in", SqlDbType.TinyInt);
      New_Customer.Parameters.Add("@income_in", SqlDbType.Int);
      New_Customer.Parameters.Add("@gender_in", SqlDbType.VarChar, 1);

      New_Member = new SqlCommand("NEW_MEMBER", objConn);
      New_Member.CommandType = CommandType.StoredProcedure;
      New_Member.Parameters.Add("@customerid_in", SqlDbType.Int);
      New_Member.Parameters.Add("@membershiplevel_in", SqlDbType.Int);

      New_Prod_Review = new SqlCommand("NEW_PROD_REVIEW", objConn);
      New_Prod_Review.CommandType = CommandType.StoredProcedure;
      New_Prod_Review.Parameters.Add("@prod_id_in", SqlDbType.Int);
      New_Prod_Review.Parameters.Add("@stars_in", SqlDbType.Int);
      New_Prod_Review.Parameters.Add("@customerid_in", SqlDbType.Int);
      New_Prod_Review.Parameters.Add("@review_summary_in", SqlDbType.VarChar, 50);
      New_Prod_Review.Parameters.Add("@review_text_in", SqlDbType.VarChar, 1000);

      New_Review_Helpfulness = new SqlCommand("NEW_REVIEW_HELPFULNESS", objConn);
      New_Review_Helpfulness.CommandType = CommandType.StoredProcedure;
      New_Review_Helpfulness.Parameters.Add("@review_id_in", SqlDbType.Int);
      New_Review_Helpfulness.Parameters.Add("@customerid_in", SqlDbType.Int);
      New_Review_Helpfulness.Parameters.Add("@review_helpfulness_in", SqlDbType.Int);
            
      Browse_By_Category = new SqlCommand("BROWSE_BY_CATEGORY", objConn);
      Browse_By_Category.CommandType = CommandType.StoredProcedure; 
      Browse_By_Category.Parameters.Add("@batch_size_in", SqlDbType.Int);
      Browse_By_Category.Parameters.Add("@category_in", SqlDbType.Int);
      
      Browse_By_Actor = new SqlCommand("BROWSE_BY_ACTOR", objConn);
      Browse_By_Actor.CommandType = CommandType.StoredProcedure; 
      Browse_By_Actor.Parameters.Add("@batch_size_in", SqlDbType.Int);
      Browse_By_Actor.Parameters.Add("@actor_in", SqlDbType.VarChar, 50);

      Browse_By_Title = new SqlCommand("BROWSE_BY_TITLE", objConn);
      Browse_By_Title.CommandType = CommandType.StoredProcedure; 
      Browse_By_Title.Parameters.Add("@batch_size_in", SqlDbType.Int);
      Browse_By_Title.Parameters.Add("@title_in", SqlDbType.VarChar, 50);

      Get_Prod_Reviews = new SqlCommand("GET_PROD_REVIEWS", objConn);
      Get_Prod_Reviews.CommandType = CommandType.StoredProcedure;
      Get_Prod_Reviews.Parameters.Add("@batch_size_in", SqlDbType.Int);
      Get_Prod_Reviews.Parameters.Add("@prod_in", SqlDbType.Int);

      Get_Prod_Reviews_By_Date = new SqlCommand("GET_PROD_REVIEWS_BY_DATE", objConn);
      Get_Prod_Reviews_By_Date.CommandType = CommandType.StoredProcedure;
      Get_Prod_Reviews_By_Date.Parameters.Add("@batch_size_in", SqlDbType.Int);
      Get_Prod_Reviews_By_Date.Parameters.Add("@prod_in", SqlDbType.Int);

      Get_Prod_Reviews_By_Stars = new SqlCommand("GET_PROD_REVIEWS_BY_STARS", objConn);
      Get_Prod_Reviews_By_Stars.CommandType = CommandType.StoredProcedure;
      Get_Prod_Reviews_By_Stars.Parameters.Add("@batch_size_in", SqlDbType.Int);
      Get_Prod_Reviews_By_Stars.Parameters.Add("@prod_in", SqlDbType.Int);
      Get_Prod_Reviews_By_Stars.Parameters.Add("@stars_in", SqlDbType.Int);

      Get_Prod_Reviews_By_Title = new SqlCommand("GET_PROD_REVIEWS_BY_TITLE", objConn);
      Get_Prod_Reviews_By_Title.CommandType = CommandType.StoredProcedure;
      Get_Prod_Reviews_By_Title.Parameters.Add("@batch_size_in", SqlDbType.Int);
      Get_Prod_Reviews_By_Title.Parameters.Add("@title_in", SqlDbType.VarChar, 50);

      Get_Prod_Reviews_By_Actor = new SqlCommand("GET_PROD_REVIEWS_BY_ACTOR", objConn);
      Get_Prod_Reviews_By_Actor.CommandType = CommandType.StoredProcedure;
      Get_Prod_Reviews_By_Actor.Parameters.Add("@batch_size_in", SqlDbType.Int);
      Get_Prod_Reviews_By_Actor.Parameters.Add("@actor_in", SqlDbType.VarChar, 50);

      
      Purchase = new SqlCommand("PURCHASE", objConn);
      Purchase.CommandType = CommandType.StoredProcedure; 
      Purchase.Parameters.Add("@customerid_in", SqlDbType.Int);
      Purchase.Parameters.Add("@number_items", SqlDbType.Int);
      Purchase.Parameters.Add("@netamount_in", SqlDbType.Money);
      Purchase.Parameters.Add("@taxamount_in", SqlDbType.Money);
      Purchase.Parameters.Add("@totalamount_in", SqlDbType.Money);
      Purchase.Parameters.Add("@prod_id_in0", SqlDbType.Int); Purchase.Parameters.Add("@qty_in0", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in1", SqlDbType.Int); Purchase.Parameters.Add("@qty_in1", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in2", SqlDbType.Int); Purchase.Parameters.Add("@qty_in2", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in3", SqlDbType.Int); Purchase.Parameters.Add("@qty_in3", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in4", SqlDbType.Int); Purchase.Parameters.Add("@qty_in4", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in5", SqlDbType.Int); Purchase.Parameters.Add("@qty_in5", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in6", SqlDbType.Int); Purchase.Parameters.Add("@qty_in6", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in7", SqlDbType.Int); Purchase.Parameters.Add("@qty_in7", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in8", SqlDbType.Int); Purchase.Parameters.Add("@qty_in8", SqlDbType.Int);
      Purchase.Parameters.Add("@prod_id_in9", SqlDbType.Int); Purchase.Parameters.Add("@qty_in9", SqlDbType.Int);
     
      return(true);
      } // end ds2connect()
 
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2login(string username_in, string password_in, ref int customerid_out, ref int rows_returned, 
      ref string[] title_out, ref string[] actor_out, ref string[] related_title_out, ref double rt)
      {
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif     
      Login.Parameters["@username_in"].Value = username_in;
      Login.Parameters["@password_in"].Value = password_in;
          
#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif

      try 
        {
        Rdr = Login.ExecuteReader();
        Rdr.Read();
        customerid_out = Rdr.GetInt32(0);
        }
      catch (SqlException e) 
        {
        Console.WriteLine("Thread {0}: Error in Login: {1}", Thread.CurrentThread.Name, e.Message);
        return (false);
        }
 
      int i_row = 0;
      if (Rdr.NextResult())
        {      
        while (Rdr.Read() && (i_row < GlobalConstants.MAX_ROWS))
          {
          title_out[i_row] = Rdr.GetString(0);
          actor_out[i_row] = Rdr.GetString(1);
          related_title_out[i_row] = Rdr.GetString(2);
          ++i_row;
          }
        }
      Rdr.Close();
      rows_returned = i_row;
      
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif            

      return(true);
      }  // end ds2login
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2newcustomer(string username_in, string password_in, string firstname_in, 
      string lastname_in, string address1_in, string address2_in, string city_in, string state_in, 
      string zip_in, string country_in, string email_in, string phone_in, int creditcardtype_in, 
      string creditcard_in, int ccexpmon_in, int ccexpyr_in, int age_in, int income_in, 
      string gender_in, ref int customerid_out, ref double rt) 
      {
      int region_in = (country_in == "US") ? 1:2;
      string creditcardexpiration_in = String.Format("{0:D4}/{1:D2}", ccexpyr_in, ccexpmon_in);
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif   
     
      New_Customer.Parameters["@username_in"].Value = username_in;
      New_Customer.Parameters["@password_in"].Value = password_in;
      New_Customer.Parameters["@firstname_in"].Value = firstname_in;
      New_Customer.Parameters["@lastname_in"].Value = lastname_in;
      New_Customer.Parameters["@address1_in"].Value = address1_in;
      New_Customer.Parameters["@address2_in"].Value = address2_in;
      New_Customer.Parameters["@city_in"].Value = city_in;
      New_Customer.Parameters["@state_in"].Value = state_in;
      New_Customer.Parameters["@zip_in"].Value = (zip_in=="") ? 0 : Convert.ToInt32(zip_in);
      New_Customer.Parameters["@country_in"].Value = country_in;
      New_Customer.Parameters["@region_in"].Value = region_in;
      New_Customer.Parameters["@email_in"].Value = email_in;
      New_Customer.Parameters["@phone_in"].Value = phone_in;
      New_Customer.Parameters["@creditcardtype_in"].Value = creditcardtype_in;
      New_Customer.Parameters["@creditcard_in"].Value = creditcard_in;
      New_Customer.Parameters["@creditcardexpiration_in"].Value = creditcardexpiration_in;
      New_Customer.Parameters["@age_in"].Value = age_in;
      New_Customer.Parameters["@income_in"].Value = income_in;
      New_Customer.Parameters["@gender_in"].Value = gender_in;
    
//    Console.WriteLine("Thread {0}: Calling New_Customer w/username_in= {1}  region={2}  ccexp={3}",
//      Thread.CurrentThread.Name, username_in, region_in, creditcardexpiration_in);

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  

      bool deadlocked;      
      do
        {
        try 
          {
          deadlocked = false;
          customerid_out = Convert.ToInt32(New_Customer.ExecuteScalar().ToString(), 10); // Needed for @@IDENTITY
          }
        catch (SqlException e) 
          {
          if (e.Number == 1205)
            {
            deadlocked = true;
            Random r = new Random(DateTime.Now.Millisecond);
            int wait = r.Next(1000);
            Console.WriteLine("Thread {0}: New_Customer deadlocked...waiting {1} msec, then will retry",
              Thread.CurrentThread.Name, wait);
            Thread.Sleep(wait); // Wait up to 1 sec, then try again
            }
          else
            {           
            Console.WriteLine("Thread {0}: SQL Error {1} in New_Customer: {2}", 
              Thread.CurrentThread.Name, e.Number, e.Message);
            return(false);
            }
          }
        } while (deadlocked);
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif        

      return(true);
      } // end ds2newcustomer()



//
//-------------------------------------------------------------------------------------------------
// 
      public bool ds2newmember(int customerid_in, int membershiplevel_in, ref int customerid_out, ref double rt)
      {
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif   
     
      New_Member.Parameters["@customerid_in"].Value = customerid_in;
      New_Member.Parameters["@membershiplevel_in"].Value = membershiplevel_in;
        

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  
      
      bool deadlocked;
      do
        {
        try 
          {
          deadlocked = false;
          customerid_out = Convert.ToInt32(New_Member.ExecuteScalar().ToString());
          }       
        catch (SqlException e) 
          {
            if (e.Number == 1205)
            {
              deadlocked = true;
              Random r = new Random(DateTime.Now.Millisecond);
              int wait = r.Next(1000);
              Console.WriteLine("Thread {0}: New_Member deadlocked...waiting {1} msec, then will retry",
                                 Thread.CurrentThread.Name, wait);
              Thread.Sleep(wait); // Wait up to 1 sec, then try again
            }
            else
           { 
              Console.WriteLine("Thread {0}: Sql Server Error in New_Member.ExecuteScalar(): {1}", 
                                 Thread.CurrentThread.Name, e.Message);
              return(false);
           }
         }
        catch (System.Exception e)
        {
            Console.WriteLine("Thread {0}: System Error in New_Member.ExecuteScalar(): {1}",
              Thread.CurrentThread.Name, e.Message);
            return (false);
        }
      } while (deadlocked);
      
     
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif        
    
//    Console.WriteLine("Thread {0}: New_Customer created w/username_in= {1}  region={2}  customerid={3}",
//      Thread.CurrentThread.Name, username_in, region_in, customerid_out);

      return(true);
      } // end ds2newmember()

//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2browse(string browse_type_in, string browse_category_in, string browse_actor_in,
      string browse_title_in, int batch_size_in, int customerid_out, ref int rows_returned, 
      ref int[] prod_id_out, ref string[] title_out, ref string[] actor_out, ref decimal[] price_out, 
      ref int[] special_out, ref int[] common_prod_id_out, ref double rt)
      {
      // Products table: PROD_ID INT, CATEGORY TINYINT, TITLE VARCHAR(50), ACTOR VARCHAR(50), 
      //   PRICE DECIMAL(12,2), SPECIAL TINYINT, COMMON_PROD_ID INT
      int i_row;
      string data_in = null;
      int[] category_out = new int[GlobalConstants.MAX_ROWS];

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif  
      switch(browse_type_in)
        {
        case "category":
          Browse_By_Category.Parameters["@batch_size_in"].Value = batch_size_in;
          Browse_By_Category.Parameters["@category_in"].Value = Convert.ToInt32(browse_category_in);
          data_in = browse_category_in;
          break;
        case "actor":
          Browse_By_Actor.Parameters["@batch_size_in"].Value = batch_size_in;
          Browse_By_Actor.Parameters["@actor_in"].Value = "\"" + browse_actor_in + "\"";
          data_in = "\"" + browse_actor_in + "\"";
          break;
        case "title":
          Browse_By_Title.Parameters["@batch_size_in"].Value = batch_size_in;
          Browse_By_Title.Parameters["@title_in"].Value = "\"" + browse_title_in + "\"";
          data_in = "\"" + browse_title_in + "\"";
          break;
        }

//    Console.WriteLine("Thread {0}: Calling Browse w/ browse_type= {1} batch_size_in= {2}  data_in= {3}",  
//      Thread.CurrentThread.Name, browse_type_in, batch_size_in, data_in); 

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif 
                 
      try 
        {
        switch(browse_type_in)
          {
          case "category":
            Rdr = Browse_By_Category.ExecuteReader();
            break;
          case "actor":
            Rdr = Browse_By_Actor.ExecuteReader();        
            break;
          case "title":
            Rdr = Browse_By_Title.ExecuteReader();        
            break;
          }
        
        i_row = 0;
        while (Rdr.Read())
          {
          prod_id_out[i_row] = Rdr.GetInt32(0);
          category_out[i_row] = Rdr.GetByte(1);
          title_out[i_row] = Rdr.GetString(2);
          actor_out[i_row] = Rdr.GetString(3);
          price_out[i_row] = Rdr.GetDecimal(4);
          special_out[i_row] = Rdr.GetByte(5);
          common_prod_id_out[i_row] = Rdr.GetInt32(6);
          ++i_row;
          }
        Rdr.Close();
        rows_returned = i_row;
        }
      catch (SqlException e) 
        {
        Console.WriteLine("Thread {0}: Error in Browse: {1}", Thread.CurrentThread.Name, e.Message);
        return(false);
        }
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif  

      return(true);
      } // end ds2browse()

    //
    //-------------------------------------------------------------------------------------------------
    // 

    public bool ds2browsereview(string browse_review_type_in, string get_review_category_in, string get_review_actor_in,
      string get_review_title_in, int batch_size_in, int customerid_out, ref int rows_returned,
      ref int[] prod_id_out, ref string[] title_out, ref string[] actor_out, ref int[] review_id_out,
      ref string[] review_date_out, ref int[] review_stars_out, ref int[] review_customerid_out,
      ref string[] review_summary_out, ref string[] review_text_out, ref int[] review_helpfulness_sum_out, ref double rt)
    {
        // Reviews Table: "REVIEW_ID" NUMBER,  "PROD_ID" NUMBER,  "REVIEW_DATE" DATE, "STARS" NUMBER,
        // "CUSTOMERID" NUMBER,  "REVIEW_SUMMARY" VARCHAR2(50 byte), "REVIEW_TEXT" VARCHAR2(1000 byte) 
        string data_in = null;
        int[] category_out = new int[GlobalConstants.MAX_ROWS];
        int i_row;

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif
        switch (browse_review_type_in)
        {
            case "actor":
                Get_Prod_Reviews_By_Actor.Parameters["@batch_size_in"].Value = batch_size_in;
                Get_Prod_Reviews_By_Actor.Parameters["@actor_in"].Value = "\"" + get_review_actor_in + "\"";
                data_in = "\"" + get_review_actor_in + "\"";
                break;
            case "title":
                Get_Prod_Reviews_By_Title.Parameters["@batch_size_in"].Value = batch_size_in;
                Get_Prod_Reviews_By_Title.Parameters["@title_in"].Value = "\"" + get_review_title_in + "\"";
                data_in = "\"" + get_review_title_in + "\"";
                break;
        }

        //    Console.WriteLine("Thread {0}: Calling Browse w/ browse_type= {1}  batch_size_in= {2}  data_in= {3}",  
        //      Thread.CurrentThread.Name, browse_type_in, batch_size_in, data_in); 

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif

        try
        {
            switch (browse_review_type_in)
              {
                case "actor":
                    Rdr = Get_Prod_Reviews_By_Actor.ExecuteReader();
                    break;
                case "title":
                    Rdr = Get_Prod_Reviews_By_Title.ExecuteReader();
                    break;
              }
            i_row = 0;
            while (Rdr.Read())
              {
                  prod_id_out[i_row] = Rdr.GetInt32(0);
                  title_out[i_row] = Rdr.GetString(1);
                  actor_out[i_row] = Rdr.GetString(2);
                  review_id_out[i_row] = Rdr.GetInt32(3);
                  review_date_out[i_row] = Convert.ToString(Rdr.GetSqlDateTime(4));
                  review_stars_out[i_row] = Rdr.GetInt32(5);
                  review_customerid_out[i_row] = Rdr.GetInt32(6);
                  review_summary_out[i_row] = Rdr.GetString(7);
                  review_text_out[i_row] = Rdr.GetString(8);
                  review_helpfulness_sum_out[i_row] = Rdr.GetInt32(9);
                  ++i_row;
              }
            Rdr.Close();
            rows_returned = i_row;
        }
        catch (SqlException e)
        {
            Console.WriteLine("Thread {0}: Sql Server Error in Browse Product Reviews: {1}", Thread.CurrentThread.Name, e.Message);
            return (false);
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Thread {0}: System Error in Browse Product Reviews: {1}", Thread.CurrentThread.Name, e.Message);
            return (false);
        }

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif
           
            
       return (true);
    } // end ds2browsereview()

    //
    //-------------------------------------------------------------------------------------------------
    // 

    public bool ds2getreview(string get_review_type_in, int get_review_prod_in, int get_review_stars_in, int customerid_out, int batch_size_in, ref int rows_returned,
      ref int[] prod_id_out, ref int[] review_id_out, ref string[] review_date_out, ref int[] review_stars_out, ref int[] review_customerid_out,
      ref string[] review_summary_out, ref string[] review_text_out, ref int[] review_helpfulness_sum_out, ref double rt)
    {
        // Reviews Table: "REVIEW_ID" NUMBER,  "PROD_ID" NUMBER,  "REVIEW_DATE" DATE, "STARS" NUMBER,
        // "CUSTOMERID" NUMBER,  "REVIEW_SUMMARY" VARCHAR2(50 byte), "REVIEW_TEXT" VARCHAR2(1000 byte) 
        string data_in = null;
        int[] category_out = new int[GlobalConstants.MAX_ROWS];
        int i_row;

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif
        switch (get_review_type_in)
        {
            case "noorder":
                Get_Prod_Reviews.Parameters["@batch_size_in"].Value = batch_size_in;
                Get_Prod_Reviews.Parameters["@prod_in"].Value = get_review_prod_in;
                break;
            case "star":
                Get_Prod_Reviews_By_Stars.Parameters["@batch_size_in"].Value = batch_size_in;
                Get_Prod_Reviews_By_Stars.Parameters["@prod_in"].Value = get_review_prod_in;
                Get_Prod_Reviews_By_Stars.Parameters["@stars_in"].Value = get_review_stars_in;
                break;
            case "date":
                Get_Prod_Reviews_By_Date.Parameters["@batch_size_in"].Value = batch_size_in;
                Get_Prod_Reviews_By_Date.Parameters["@prod_in"].Value = get_review_prod_in;
                break;
        }

        //    Console.WriteLine("Thread {0}: Calling Browse w/ browse_type= {1}  batch_size_in= {2}  data_in= {3}",  
        //      Thread.CurrentThread.Name, browse_type_in, batch_size_in, data_in); 

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif

        try
        {
            switch (get_review_type_in)
            {
              case "noorder":
                 Rdr = Get_Prod_Reviews.ExecuteReader();
                 break;
              case "star":
                 Rdr = Get_Prod_Reviews_By_Stars.ExecuteReader();
                 break;
              case "date":
                 Rdr = Get_Prod_Reviews_By_Date.ExecuteReader();
                 break;
            }

            i_row = 0;
            while (Rdr.Read())
            {
                prod_id_out[i_row] = Rdr.GetInt32(0);
                review_id_out[i_row] = Rdr.GetInt32(1);
                review_date_out[i_row] = Convert.ToString(Rdr.GetSqlDateTime(2));
                review_stars_out[i_row] = Rdr.GetInt32(3);
                review_customerid_out[i_row] = Rdr.GetInt32(4);
                review_summary_out[i_row] = Rdr.GetString(5);
                review_text_out[i_row] = Rdr.GetString(6);
                review_helpfulness_sum_out[i_row] = Rdr.GetInt32(7);
                ++i_row;
            }
            Rdr.Close();
            rows_returned = i_row;
        }
        catch (SqlException e)
        {
            Console.WriteLine("Thread {0}: Sql Server Error in Get Product Reviews: {1}", Thread.CurrentThread.Name, e.Message);
            return (false);
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Thread {0}: System Error in Get Product Reviews: {1}", Thread.CurrentThread.Name, e.Message);
            return (false);
        }

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif
        
        return (true);
    } // end ds2getreview()

    //
    //-------------------------------------------------------------------------------------------------
    // 
    public bool ds2newreview(int new_review_prod_id_in, int new_review_stars_in, int new_review_customerid_in,
            string new_review_summary_in, string new_review_text_in, ref int newreviewid_out, ref double rt)
    {

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif

        New_Prod_Review.Parameters["@prod_id_in"].Value = new_review_prod_id_in;
        New_Prod_Review.Parameters["@stars_in"].Value = new_review_stars_in;
        New_Prod_Review.Parameters["@customerid_in"].Value = new_review_customerid_in;
        New_Prod_Review.Parameters["@review_summary_in"].Value = new_review_summary_in;
        New_Prod_Review.Parameters["@review_text_in"].Value = new_review_text_in;


#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif
        bool deadlocked;
        do
        {
            try
            {
                deadlocked = false;
                newreviewid_out = Convert.ToInt32(New_Prod_Review.ExecuteScalar().ToString(), 10);
            }
            catch (SqlException e)
            {
                if (e.Number == 1205)
                {
                    deadlocked = true;
                    Random r = new Random(DateTime.Now.Millisecond);
                    int wait = r.Next(1000);
                    Console.WriteLine("Thread {0}: New_Prod_Review deadlocked...waiting {1} msec, then will retry",
                                       Thread.CurrentThread.Name, wait);
                    Thread.Sleep(wait); // Wait up to 1 sec, then try again
                }
                else
                {
                    Console.WriteLine("Thread {0}: Sql Server Error in New_Prod_Review.ExecuteScalar(): {1}",
                      Thread.CurrentThread.Name, e.Message);
                    return (false);
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Thread {0}: System Error in New_Prod_Review.ExecuteNonQuery(): {1}",
                  Thread.CurrentThread.Name, e.Message);
                return (false);
            }
        } while (deadlocked);


#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif
        return (true);
    } // end ds2newreview()



    //
    //-------------------------------------------------------------------------------------------------
    // 
    public bool ds2newreviewhelpfulness(int reviewid_in, int customerid_in, int reviewhelpfulness_in, ref int reviewhelpfulnessid_out, ref double rt)
    {
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif

        New_Review_Helpfulness.Parameters["@review_id_in"].Value = reviewid_in;
        New_Review_Helpfulness.Parameters["@customerid_in"].Value = customerid_in;
        New_Review_Helpfulness.Parameters["@review_helpfulness_in"].Value = reviewhelpfulness_in;

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif
        bool deadlocked;
        do
          {
          try
            {
            deadlocked = false;
            reviewhelpfulnessid_out = Convert.ToInt32(New_Review_Helpfulness.ExecuteScalar().ToString(), 10);
            }
          catch (SqlException e)
            {
            if (e.Number == 1205)
              {
              deadlocked = true;
              Random r = new Random(DateTime.Now.Millisecond);
              int wait = r.Next(1000);
              Console.WriteLine("Thread {0}: New_Customer deadlocked...waiting {1} msec, then will retry",
                                 Thread.CurrentThread.Name, wait);
              Thread.Sleep(wait); // Wait up to 1 sec, then try again
              }
            else
              {
              Console.WriteLine("Thread {0}: Sql Server Error in New_Review_Helpfulness.ExecuteScalar(): {1}",
                                  Thread.CurrentThread.Name, e.Message);
              return (false);
              }
          }
          catch (System.Exception e)
          {
              Console.WriteLine("Thread {0}: System Error in New_Review_Helpfulness.ExecuteScalar(): {1}",
              Thread.CurrentThread.Name, e.Message);
              return (false);
          }
        } while (deadlocked);
        


#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif

        return (true);
    } // end ds2newreviewhelpfulness()




//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2purchase(int cart_items, int[] prod_id_in, int[] qty_in, int customerid_out,
      ref int neworderid_out, ref bool IsRollback, ref double rt)
      {
      int i, j;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif 
 
      //Cap cart_items at 10 for this implementation of stored procedure
      cart_items = System.Math.Min(10, cart_items);
      
      // Extra, non-stored procedure query to find total cost of purchase
      Decimal netamount_in = 0;  
      //Modified by GSK for parameterization of query below - Affects performance in case of Query Caching      
      //string cost_query = "select PROD_ID, PRICE from PRODUCTS where PROD_ID in (" + prod_id_in[0];
      //for (i=1; i<cart_items; i++) cost_query = cost_query + "," + prod_id_in[i];
      //cost_query = cost_query + ")";
      ////Console.WriteLine(cost_query);
      //SqlCommand cost_command = new SqlCommand(cost_query, objConn);

      //Parameterized query by GSK
      string cost_query = "select PROD_ID, PRICE from PRODUCTS where PROD_ID in ( @ARG0";
      for ( i = 1 ; i < cart_items ; i++ ) cost_query = cost_query + ", @ARG" + i;
      cost_query = cost_query + ")";
      SqlCommand cost_command = new SqlCommand ( cost_query , objConn );
      string ArgHolder;
      for ( i = 0 ; i < cart_items ; i++ )
          {
          ArgHolder = "@ARG" + i;
          cost_command.Parameters.Add ( ArgHolder , SqlDbType.Int );
          cost_command.Parameters[ArgHolder].Value = prod_id_in[i];
          //Console.WriteLine (cost_command.Parameters[ArgHolder].Value);
          }
            
      Rdr = cost_command.ExecuteReader();
      while (Rdr.Read())
        {
        j = 0;
        int prod_id = Rdr.GetInt32(0);
        while (prod_id_in[j] != prod_id) ++j; // Find which product was returned
        netamount_in = netamount_in + qty_in[j] * Rdr.GetDecimal(1);
        //Console.WriteLine(j + " " + prod_id + " " + Rdr.GetDecimal(1));
        }
      Rdr.Close();
      // Can use following code instead if you don't want extra roundtrip to database:
      // Random rr = new Random(DateTime.Now.Millisecond);
      // Decimal netamount_in = (Decimal) (0.01 * (1 + rr.Next(40000)));
      Decimal taxamount_in =  (Decimal) 0.0825 * netamount_in;
      Decimal totalamount_in = netamount_in + taxamount_in;
      //Console.WriteLine(netamount_in);
      
      Purchase.Parameters["@customerid_in"].Value = customerid_out;
      Purchase.Parameters["@number_items"].Value = cart_items;
      Purchase.Parameters["@netamount_in"].Value = netamount_in;
      Purchase.Parameters["@taxamount_in"].Value = taxamount_in;
      Purchase.Parameters["@totalamount_in"].Value = totalamount_in;
      Purchase.Parameters["@prod_id_in0"].Value = prod_id_in[0]; Purchase.Parameters["@qty_in0"].Value = qty_in[0];
      Purchase.Parameters["@prod_id_in1"].Value = prod_id_in[1]; Purchase.Parameters["@qty_in1"].Value = qty_in[1];
      Purchase.Parameters["@prod_id_in2"].Value = prod_id_in[2]; Purchase.Parameters["@qty_in2"].Value = qty_in[2];
      Purchase.Parameters["@prod_id_in3"].Value = prod_id_in[3]; Purchase.Parameters["@qty_in3"].Value = qty_in[3];
      Purchase.Parameters["@prod_id_in4"].Value = prod_id_in[4]; Purchase.Parameters["@qty_in4"].Value = qty_in[4];
      Purchase.Parameters["@prod_id_in5"].Value = prod_id_in[5]; Purchase.Parameters["@qty_in5"].Value = qty_in[5];
      Purchase.Parameters["@prod_id_in6"].Value = prod_id_in[6]; Purchase.Parameters["@qty_in6"].Value = qty_in[6];
      Purchase.Parameters["@prod_id_in7"].Value = prod_id_in[7]; Purchase.Parameters["@qty_in7"].Value = qty_in[7];
      Purchase.Parameters["@prod_id_in8"].Value = prod_id_in[8]; Purchase.Parameters["@qty_in8"].Value = qty_in[8];
      Purchase.Parameters["@prod_id_in9"].Value = prod_id_in[9]; Purchase.Parameters["@qty_in9"].Value = qty_in[9];
               
//    Console.WriteLine("Thread {0}: Calling Purchase w/ customerid = {1}  number_items= {2}",  
//      Thread.CurrentThread.Name, customerid_out, cart_items);

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)  
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  

      bool deadlocked;      
      do
        {
        try 
          {
          deadlocked = false;
          neworderid_out = (int) Purchase.ExecuteScalar();
          }
        catch (SqlException e) 
          {
          if (e.Number == 1205)
            {
            deadlocked = true;
            Random r = new Random(DateTime.Now.Millisecond);
            int wait = r.Next(1000);
            Console.WriteLine("Thread {0}: Purchase deadlocked...waiting {1} msec, then will retry",
              Thread.CurrentThread.Name, wait);
            Thread.Sleep(wait); // Wait up to 1 sec, then try again
            }
          else
            {           
            Console.WriteLine("Thread {0}: SQL Error {1} in Purchase: {2}", 
              Thread.CurrentThread.Name, e.Number, e.Message);
            return(false);
            }
          }
        } while (deadlocked);

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif  
      if (neworderid_out == 0) IsRollback = true;    
      return(true);
      } // end ds2purchase()
    
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2close()
      {
      objConn.Close();   
      return(true);   
      } // end ds2close()
    } // end Class ds2Interface
  } // end namespace ds2xdriver
  
        
