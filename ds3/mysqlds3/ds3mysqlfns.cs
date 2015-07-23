
/*
 * DVD Store 2 MySQL Functions - ds2mysqlfns.cs
 *
 * Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
 *
 * Provides interface functions for DVD Store driver program ds2xdriver.cs
 * See ds2xdriver.cs for compilation and syntax
 *
 * Last Updated 6/27/05
 * Last updated 6/14/2010 by GSK
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
using MySql.Data.MySqlClient;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;


namespace ds2xdriver
  {
  /// <summary>
  /// ds2mysqlfns.cs: DVD Store 2 MySql Functions
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
    MySqlConnection objConn;
    MySqlCommand Login, New_Customer, New_Member, Browse, BrowseReviews, GetReviews, New_Review, New_Helpfulness, Purchase;
    MySqlDataReader Rdr;
    MySqlParameter cust_out_param, reviewid_out_param, helpfulnessid_out_param;
    string db_query;
    //string conn_str = "Server=" +  Controller.target + ";User ID=web;Password=web;Database=DS2";
    //Changed by GSK (connection string will be initialized in new Overloaded constructor )
    string conn_str = "";
    string target_server_name;
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
    //Added by GSK (Overloaded the constructor to handle scenario where Single instance of Driver program is driving load on multiple machines)
    public ds2Interface ( int ds2interfaceid , string target_name)
        {
        ds2Interfaceid = ds2interfaceid;
        target_server_name = target_name;
        conn_str = "Server=" + target_server_name + ";User ID=web;Password=web;Database=DS3";
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
      try
        {
        objConn = new MySqlConnection(conn_str);
        objConn.Open();
        }
      catch (MySqlException e)
        {
        //Changed by GSK
        //Console.WriteLine("Thread {0}: error in connecting to database {1}: {2}",  Thread.CurrentThread.Name,
        //  Controller.target, e.Message);
        Console.WriteLine("Thread {0}: error in connecting to database {1}: {2}",  Thread.CurrentThread.Name,
          target_server_name , e.Message );
        return(false);
        }

      // Set up MySql stored procedure calls and associated parameters
      New_Customer = new MySqlCommand("NEW_CUSTOMER", objConn);
      New_Customer.CommandType = CommandType.StoredProcedure; 
      New_Customer.Parameters.Add("username_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("password_in", MySqlDbType.VarChar, 50);    
      New_Customer.Parameters.Add("firstname_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("lastname_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("address1_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("address2_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("city_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("state_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("zip_in", MySqlDbType.Int32);
      New_Customer.Parameters.Add("country_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("region_in", MySqlDbType.Int32);
      New_Customer.Parameters.Add("email_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("phone_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("creditcardtype_in", MySqlDbType.Int32);
      New_Customer.Parameters.Add("creditcard_in", MySqlDbType.VarChar, 50);
      New_Customer.Parameters.Add("creditcardexpiration_in", MySqlDbType.VarChar, 50); 
      New_Customer.Parameters.Add("age_in", MySqlDbType.Byte);
      New_Customer.Parameters.Add("income_in", MySqlDbType.Int32);
      New_Customer.Parameters.Add("gender_in", MySqlDbType.VarChar, 1);
      cust_out_param = new MySqlParameter("customerid_out", MySqlDbType.Int32);
      cust_out_param.Direction = ParameterDirection.Output;
      cust_out_param.Value = 0;
      New_Customer.Parameters.Add(cust_out_param);  
    
      New_Member = new MySqlCommand("NEW_MEMBER", objConn);
      New_Member.CommandType = CommandType.StoredProcedure; 
      New_Member.Parameters.Add("customerid_in", MySqlDbType.Int32);
      New_Member.Parameters.Add("membershiplevel_in", MySqlDbType.Int32);
      New_Member.Parameters.Add(cust_out_param);

      New_Review = new MySqlCommand("NEW_PROD_REVIEW", objConn);
      New_Review.CommandType = CommandType.StoredProcedure;
      New_Review.Parameters.Add("prod_id_in", MySqlDbType.Int32);
      New_Review.Parameters.Add("stars_in", MySqlDbType.Int32);
      New_Review.Parameters.Add("customerid_in", MySqlDbType.Int32);
      New_Review.Parameters.Add("review_summary_in", MySqlDbType.VarChar, 50);
      New_Review.Parameters.Add("review_text_in", MySqlDbType.VarChar, 1000);
      reviewid_out_param = new MySqlParameter("review_id_out", MySqlDbType.Int32);
      reviewid_out_param.Direction = ParameterDirection.Output;
      reviewid_out_param.Value = 0;
      New_Review.Parameters.Add(reviewid_out_param);

      New_Helpfulness = new MySqlCommand("NEW_REVIEW_HELPFULNESS", objConn);
      New_Helpfulness.CommandType = CommandType.StoredProcedure;
      New_Helpfulness.Parameters.Add("review_id_in", MySqlDbType.Int32);
      New_Helpfulness.Parameters.Add("customerid_in", MySqlDbType.Int32);
      New_Helpfulness.Parameters.Add("review_helpfulness_in", MySqlDbType.Int32);
      helpfulnessid_out_param = new MySqlParameter("review_helpfulness_id_out", MySqlDbType.Int32);
      helpfulnessid_out_param.Direction = ParameterDirection.Output;
      helpfulnessid_out_param.Value = 0;
      New_Helpfulness.Parameters.Add(helpfulnessid_out_param);

      return(true);
      } // end ds2connect()
 
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2login(string username_in, string password_in, ref int customerid_out, ref int rows_returned, 
      ref string[] title_out, ref string[] actor_out, ref string[] related_title_out, ref double rt)
      {
      int i_row=0, prod_id;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif     
      
      db_query="select CUSTOMERID FROM DS3.CUSTOMERS where USERNAME='" + username_in + "' and PASSWORD='" + password_in
        + "';";
      Login = new MySqlCommand(db_query, objConn);
      rows_returned = 0;
   
#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif

      try 
        {         
        Rdr = Login.ExecuteReader();
        if (!Rdr.HasRows)  // No customer found
          {
          customerid_out = 0;
          return(true);
          }
        Rdr.Read();
        customerid_out = Rdr.GetInt32(0);
        Rdr.Close();
        db_query = "select PROD_ID from DS3.CUST_HIST where CUSTOMERID =" + customerid_out + " ORDER BY ORDERID DESC LIMIT 10;"; 
        Login = new MySqlCommand(db_query, objConn);
   
        Rdr = Login.ExecuteReader();
        if (!Rdr.HasRows)  // No previous order
          {
          //Console.WriteLine("No previous orders");
          }
        else
          {
          i_row = 0;
          while(Rdr.Read())
            {
            prod_id = Rdr.GetInt32(0);            
            string db_query2 = "select TITLE, ACTOR from DS3.PRODUCTS where PROD_ID=" + prod_id + ";";
            MySqlConnection conn2 = new MySqlConnection(conn_str);
            conn2.Open();
            MySqlCommand Login2 = new MySqlCommand(db_query2, conn2);
            MySqlDataReader Rdr2 = Login2.ExecuteReader();
            Rdr2.Read();
            title_out[i_row] = Rdr2.GetString(0);
            actor_out[i_row] = Rdr2.GetString(1);                         
            Rdr2.Close();
            string db_query3 = "select TITLE from DS3.PRODUCTS where PROD_ID = " +
              "(select COMMON_PROD_ID from DS3.PRODUCTS where PROD_ID=" + prod_id + ");";
            MySqlCommand Login3 = new MySqlCommand(db_query3, conn2);
            related_title_out[i_row] = (string) Login3.ExecuteScalar();                        
            conn2.Close();
            ++i_row;
            }
          } // End else
        Rdr.Close();
        rows_returned = i_row;
        } // End try 
      catch (MySqlException e) 
        {
        Console.WriteLine("Thread {0}: Error in Login: {1}", Thread.CurrentThread.Name, e.Message);
        return (false);
        }
        
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
      New_Customer.Parameters["username_in"].Value = username_in;
      New_Customer.Parameters["password_in"].Value = password_in;
      New_Customer.Parameters["firstname_in"].Value = firstname_in;
      New_Customer.Parameters["lastname_in"].Value = lastname_in;
      New_Customer.Parameters["address1_in"].Value = address1_in;
      New_Customer.Parameters["address2_in"].Value = address2_in;
      New_Customer.Parameters["city_in"].Value = city_in;
      New_Customer.Parameters["state_in"].Value = state_in;
      New_Customer.Parameters["zip_in"].Value = (zip_in=="") ? 0 : Convert.ToInt32(zip_in);
      New_Customer.Parameters["country_in"].Value = country_in;
      New_Customer.Parameters["region_in"].Value = region_in;
      New_Customer.Parameters["email_in"].Value = email_in;
      New_Customer.Parameters["phone_in"].Value = phone_in;
      New_Customer.Parameters["creditcardtype_in"].Value = creditcardtype_in;
      New_Customer.Parameters["creditcard_in"].Value = creditcard_in;
      New_Customer.Parameters["creditcardexpiration_in"].Value = creditcardexpiration_in;
      New_Customer.Parameters["age_in"].Value = age_in;
      New_Customer.Parameters["income_in"].Value = income_in;
      New_Customer.Parameters["gender_in"].Value = gender_in;
    
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
          New_Customer.ExecuteNonQuery();
          }
        catch (MySqlException e) 
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
            Console.WriteLine("Thread {0}: MySql Error {1} in New_Customer: {2}", 
              Thread.CurrentThread.Name, e.Number, e.Message);
            return(false);
            }
          }
        } while (deadlocked);

        customerid_out = (int) cust_out_param.Value;         
        
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
        New_Member.Parameters["customerid_in"].Value = customerid_in;
        New_Member.Parameters["membershiplevel_in"].Value = membershiplevel_in;

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
                New_Member.ExecuteNonQuery();
            }
            catch (MySqlException e)
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
                    Console.WriteLine("Thread {0}: MySql Error {1} in New_Member: {2}",
                      Thread.CurrentThread.Name, e.Number, e.Message);
                    return (false);
                }
            }
        } while (deadlocked);

        customerid_out = (int)cust_out_param.Value;


#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif

        //    Console.WriteLine("Thread {0}: New_Customer created w/username_in= {1}  region={2}  customerid={3}",
        //      Thread.CurrentThread.Name, username_in, region_in, customerid_out);

        return (true);
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
      int[] category_out = new int[GlobalConstants.MAX_ROWS];

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif  

      switch(browse_type_in)
        {
        case "title":
          db_query = "select * from PRODUCTS where MATCH (TITLE) AGAINST ('" + browse_title_in + "') LIMIT " +
            batch_size_in + ";";
          break;
        case "actor":
          db_query = "select * from PRODUCTS where MATCH (ACTOR) AGAINST ('" + browse_actor_in + "') LIMIT " +
            batch_size_in + ";";
          break;
        case "category":
          db_query="select * from PRODUCTS where CATEGORY = " + Convert.ToInt32(browse_category_in) + 
            " and SPECIAL=1 LIMIT " + batch_size_in + ";";
          break;
        }
        
        Browse = new MySqlCommand(db_query, objConn);

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif 
        
      try 
        {
        Rdr = Browse.ExecuteReader();
 
        i_row = 0;
        if (!Rdr.HasRows) // No rows returned
          {
          //Console.WriteLine("No DVDs Found");
          }
        else  // Rows returned
          {
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
          }
        Rdr.Close();
        rows_returned = i_row;
        }
      catch (MySqlException e) 
        {
        Console.WriteLine("Thread {0}: Error in Browse: {1}", Thread.CurrentThread.Name, e.Message);
        return(false);
        }

//    Console.WriteLine("Thread {0}: Calling Browse w/ browse_type= {1} batch_size_in= {2}  category= {3}" +
//      " title= {4}  actor= {5}", Thread.CurrentThread.Name, browse_type_in, batch_size_in, browse_category_in,
//      browse_title_in, browse_actor_in); 
                     
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
                db_query = "select T1.prod_id, T1.title, T1.actor, REVIEWS_HELPFULNESS.REVIEW_ID, T1.review_date, T1.stars, " +
                    "T1.customerid, T1.review_summary, T1.review_text, SUM(helpfulness) AS totalhelp from REVIEWS_HELPFULNESS " +
                    "inner join (select TITLE, ACTOR, PRODUCTS.PROD_ID,REVIEWS.review_date, REVIEWS.stars, " +
                    "REVIEWS.review_id, REVIEWS.customerid, REVIEWS.review_summary, REVIEWS.review_text  " +
                    "from PRODUCTS inner join REVIEWS on PRODUCTS.prod_id = REVIEWS.prod_id " + 
                  //  "where MATCH (ACTOR) AGAINST ('" + get_review_actor_in + "') GROUP BY TITLE limit 10) " +
                   "where MATCH (ACTOR) AGAINST ('" + get_review_actor_in + "') limit 500) " +
                    "as T1 on REVIEWS_HELPFULNESS.REVIEW_ID = T1.review_id GROUP BY REVIEW_ID ORDER BY totalhelp DESC limit 10;";
                data_in = get_review_actor_in;
                break;
            case "title":
                db_query = "select T1.prod_id, T1.title, T1.actor, REVIEWS_HELPFULNESS.REVIEW_ID, T1.review_date, T1.stars, " +
                    "T1.customerid, T1.review_summary, T1.review_text, SUM(helpfulness) AS totalhelp from REVIEWS_HELPFULNESS " +
                    "inner join (select TITLE, ACTOR, PRODUCTS.PROD_ID,REVIEWS.review_date, REVIEWS.stars, " +
                    "REVIEWS.review_id, REVIEWS.customerid, REVIEWS.review_summary, REVIEWS.review_text  " +
                    "from PRODUCTS inner join REVIEWS on PRODUCTS.prod_id = REVIEWS.prod_id " +
                  //  "where MATCH (TITLE) AGAINST ('" + get_review_title_in + "') GROUP BY TITLE limit 10) " +
                    "where MATCH (TITLE) AGAINST ('" + get_review_title_in + "') limit 500) " +
                    "as T1 on REVIEWS_HELPFULNESS.REVIEW_ID = T1.review_id GROUP BY REVIEW_ID ORDER BY totalhelp DESC limit 10;";
                data_in = get_review_title_in;
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
            BrowseReviews = new MySqlCommand(db_query, objConn);
            Rdr = BrowseReviews.ExecuteReader();
            i_row = 0;
            while (Rdr.Read())
              {
                prod_id_out[i_row] = Rdr.GetInt32(0);
                title_out[i_row] = Rdr.GetString(1);
                actor_out[i_row] = Rdr.GetString(2);
                review_id_out[i_row] = Rdr.GetInt32(3);
                review_date_out[i_row] = Rdr.GetString(4);
                review_stars_out[i_row] = Rdr.GetInt32(5);
                review_customerid_out[i_row] = Rdr.GetInt32(6);
                review_summary_out[i_row] = Rdr.GetString(7);
                review_text_out[i_row] = Rdr.GetString(8);
                review_helpfulness_sum_out[i_row] = Rdr.GetInt32(9);
                ++i_row;
            } // end while rdr.read()
            Rdr.Close();
            rows_returned = i_row;
          }
        catch (MySqlException e)
        {
            Console.WriteLine("Thread {0}: MySQL Error in Browse Product Reviews: {1}", Thread.CurrentThread.Name, e.Message);
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
        int i_row;
        int[] category_out = new int[GlobalConstants.MAX_ROWS];

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif
        switch (get_review_type_in)
        {
            case "noorder":
                db_query = "SELECT REVIEWS.review_id, REVIEWS.prod_id, REVIEWS.review_date, REVIEWS.stars, " +
                    "REVIEWS.customerid,REVIEWS.review_summary, REVIEWS.review_text, SUM(REVIEWS_HELPFULNESS.helpfulness) " +
                    "as total FROM REVIEWS INNER JOIN REVIEWS_HELPFULNESS on REVIEWS.review_id=REVIEWS_HELPFULNESS.review_id " +
                    "WHERE PROD_ID = " + get_review_prod_in + " GROUP BY REVIEWS.review_id ORDER BY total DESC;";
                break;
            case "star":
                db_query = "SELECT REVIEWS.review_id, REVIEWS.prod_id, REVIEWS.review_date, REVIEWS.stars, " +
                    "REVIEWS.customerid,REVIEWS.review_summary, REVIEWS.review_text, SUM(REVIEWS_HELPFULNESS.helpfulness) " +
                    "as total FROM REVIEWS INNER JOIN REVIEWS_HELPFULNESS on REVIEWS.review_id=REVIEWS_HELPFULNESS.review_id " +
                    "WHERE PROD_ID = " + get_review_prod_in + " AND STARS = " + get_review_stars_in + 
                    " GROUP BY REVIEWS.review_id ORDER BY total DESC;";
                break;
            case "date":
                db_query = "SELECT REVIEWS.review_id, REVIEWS.prod_id, REVIEWS.review_date, REVIEWS.stars, " +
                    "REVIEWS.customerid,REVIEWS.review_summary, REVIEWS.review_text, SUM(REVIEWS_HELPFULNESS.helpfulness) " +
                    "as total FROM REVIEWS INNER JOIN REVIEWS_HELPFULNESS on REVIEWS.review_id=REVIEWS_HELPFULNESS.review_id " +
                    "WHERE PROD_ID = " + get_review_prod_in + " GROUP BY REVIEWS.review_id ORDER BY REVIEW_DATE DESC;";
                break;
        }
                
#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif

        try
        {
            GetReviews = new MySqlCommand(db_query, objConn);
            Rdr = GetReviews.ExecuteReader();
            i_row = 0;
            while (Rdr.Read())
            {
                review_id_out[i_row] = Rdr.GetInt32(0);
                prod_id_out[i_row] = Rdr.GetInt32(1);
                review_date_out[i_row] = Rdr.GetString(2);
                review_stars_out[i_row] = Rdr.GetInt32(3);
                review_customerid_out[i_row] = Rdr.GetInt32(4);
                review_summary_out[i_row] = Rdr.GetString(5);
                review_text_out[i_row] = Rdr.GetString(6);
                review_helpfulness_sum_out[i_row] = Rdr.GetInt32(7);
                ++i_row;
            } // end while rdr.read()
            Rdr.Close();
            rows_returned = i_row;
        }
        catch (MySqlException e)
        {
            Console.WriteLine("Thread {0}: MySQL Error in Get Product Reviews: {1}", Thread.CurrentThread.Name, e.Message);
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
        New_Review.Parameters["prod_id_in"].Value = new_review_prod_id_in;
        New_Review.Parameters["stars_in"].Value = new_review_stars_in;
        New_Review.Parameters["customerid_in"].Value = new_review_customerid_in;
        New_Review.Parameters["review_summary_in"].Value = new_review_summary_in;
        New_Review.Parameters["review_text_in"].Value = new_review_text_in;


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
                New_Review.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                if (e.Number == 1205)
                {
                    deadlocked = true;
                    Random r = new Random(DateTime.Now.Millisecond);
                    int wait = r.Next(1000);
                    Console.WriteLine("Thread {0}: New_Review deadlocked...waiting {1} msec, then will retry",
                      Thread.CurrentThread.Name, wait);
                    Thread.Sleep(wait); // Wait up to 1 sec, then try again
                }
                else
                {
                    Console.WriteLine("Thread {0}: MySql Error {1} in New_Review: {2}",
                      Thread.CurrentThread.Name, e.Number, e.Message);
                    return (false);
                }
            }
        } while (deadlocked);

        newreviewid_out = (int)reviewid_out_param.Value;   


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

        New_Helpfulness.Parameters["review_id_in"].Value = reviewid_in;
        New_Helpfulness.Parameters["customerid_in"].Value = customerid_in;
        New_Helpfulness.Parameters["review_helpfulness_in"].Value = reviewhelpfulness_in;

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
                New_Helpfulness.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                if (e.Number == 1205)
                {
                    deadlocked = true;
                    Random r = new Random(DateTime.Now.Millisecond);
                    int wait = r.Next(1000);
                    Console.WriteLine("Thread {0}: New_Helpfulness deadlocked...waiting {1} msec, then will retry",
                      Thread.CurrentThread.Name, wait);
                    Thread.Sleep(wait); // Wait up to 1 sec, then try again
                }
                else
                {
                    Console.WriteLine("Thread {0}: MySql Error {1} in New_Helpfulness: {2}",
                      Thread.CurrentThread.Name, e.Number, e.Message);
                    return (false);
                }
            }
        } while (deadlocked);

        reviewhelpfulnessid_out = (int)helpfulnessid_out_param.Value;   
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
      bool success = false;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif 

      // Find total cost of purchase
      Decimal netamount_in = 0;  
      string db_query = "select PROD_ID, PRICE from PRODUCTS where PROD_ID in (" + prod_id_in[0];
      for (i=1; i<cart_items; i++) db_query = db_query + "," + prod_id_in[i];
      db_query = db_query + ")";
      //Console.WriteLine(db_query);
      Purchase = new MySqlCommand(db_query, objConn);
      Rdr = Purchase.ExecuteReader();
      while (Rdr.Read())
        {
        j = 0;
        int prod_id = Rdr.GetInt32(0);
        while (prod_id_in[j] != prod_id) ++j; // Find which product was returned
        netamount_in = netamount_in + qty_in[j] * Rdr.GetDecimal(1);
        //Console.WriteLine(j + " " + prod_id + " " + Rdr.GetDecimal(1));
        }
      Rdr.Close();
      Decimal taxamount_in =  (Decimal) 0.0825 * netamount_in;
      Decimal totalamount_in = netamount_in + taxamount_in;
      
      // Insert new order into ORDERS table
      string currentdate = DateTime.Today.ToString("yyyy'-'MM'-'dd");
      MySqlTransaction trans = objConn.BeginTransaction(IsolationLevel.RepeatableRead);
      db_query = String.Format("INSERT into DS3.ORDERS (ORDERDATE, CUSTOMERID, NETAMOUNT, TAX, TOTALAMOUNT) VALUES" + 
        "('{0}', {1}, {2:F2}, {3:F2}, {4:F2})", currentdate, customerid_out, netamount_in, taxamount_in, totalamount_in);
      
      Purchase = new MySqlCommand(db_query, objConn, trans);
     
//    Console.WriteLine("Thread {0}: Calling Purchase w/ customerid = {1}  number_items= {2}",  
//      Thread.CurrentThread.Name, customerid_out, cart_items);

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)  
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  

      bool deadlocked = false;      
      do
        {
        try 
          {
          deadlocked = false;
          Purchase.ExecuteNonQuery();  
          db_query = "select LAST_INSERT_ID();";
          Purchase = new MySqlCommand(db_query, objConn);
          neworderid_out = Convert.ToInt32(Purchase.ExecuteScalar().ToString());
//        Console.WriteLine("Thread {0}: Purchase: neworderid_out= {1}", Thread.CurrentThread.Name, neworderid_out);
          if (neworderid_out > 0) success = true;
      
          // loop through purchased items and make inserts into orderdetails table 
          // (o_insert_query) and cust_hist table (ch_insert_query)
         
          string o_insert_query = "INSERT into DS3.ORDERLINES (ORDERLINEID, ORDERID, PROD_ID, QUANTITY, ORDERDATE)" +
            " VALUES "; 
          string c_insert_query = "INSERT into DS3.CUST_HIST (CUSTOMERID, ORDERID, PROD_ID) VALUES "; 
        
          for (i=0; i<cart_items; i++)
            {
            j = i+1;
            db_query = "SELECT QUAN_IN_STOCK, SALES FROM DS3.INVENTORY WHERE PROD_ID=" + prod_id_in[i] + ";";
            Purchase = new MySqlCommand(db_query, objConn);
            Rdr = Purchase.ExecuteReader();
            Rdr.Read();
            int curr_quan = Rdr.GetInt32(0);
            int curr_sales = Rdr.GetInt32(1);
            Rdr.Close();
            int new_quan = curr_quan - qty_in[i];
            int new_sales = curr_sales + qty_in [i];
            if (new_quan < 0)
              {
              //Console.WriteLine("Insufficient quantity for product " + prod_id_in[i]);
              success = false;
              }
            else   
              {
              db_query = "UPDATE DS3.INVENTORY SET QUAN_IN_STOCK=" + new_quan + ", SALES=" + 
                new_sales + " WHERE PROD_ID=" + prod_id_in[i] + ";";
              Purchase = new MySqlCommand(db_query, objConn, trans);
              Purchase.ExecuteNonQuery();
              }

            o_insert_query = o_insert_query + 
              "(" + j + "," +  neworderid_out + "," + prod_id_in[i] + "," + qty_in[i] + ",'" + currentdate + "'),";
            c_insert_query = c_insert_query + 
              "(" + customerid_out + "," +  neworderid_out + "," + prod_id_in[i] + "),";
            } // End of for (i=0; i<cart_items; i++)
          
          o_insert_query = o_insert_query.Remove(o_insert_query.Length-1,1) + ";";
//        Console.WriteLine(o_insert_query); 
         
          c_insert_query = c_insert_query.Remove(c_insert_query.Length-1,1) + ";";
//        Console.WriteLine(c_insert_query);
            
          Purchase = new MySqlCommand(o_insert_query, objConn, trans);
          if (Purchase.ExecuteNonQuery()<0)
            {
            Console.WriteLine("Thread {0}: Insert into ORDERLINES table failed; query= {1}",
              Thread.CurrentThread.Name, o_insert_query);
            success = false;
            }
       
          Purchase = new MySqlCommand(c_insert_query, objConn, trans);
          if (Purchase.ExecuteNonQuery()<0)
            {
            Console.WriteLine("Thread {0}: Insert into CUST_HIST table failed; query= {1}", 
              Thread.CurrentThread.Name, c_insert_query);
            success = false;
            }
                            
          if (success)trans.Commit();
          else trans.Rollback();   
          }  // End Try
          
        catch (MySqlException e) 
          {
          if ((e.Number == 1205) || (e.Number == 1213))
            {
            deadlocked = true;
            Random r = new Random(DateTime.Now.Millisecond);
            int wait = r.Next(1000);
            Console.WriteLine("Thread {0}: Purchase deadlocked (error {1}: {2})...waiting {3} msec, then will retry",
              Thread.CurrentThread.Name, e.Number, e.Message, wait);
            Thread.Sleep(wait); // Wait up to 1 sec, then try again
            }
          else if (e.Number == 1062)
            {
            deadlocked = true; // Not really but it will cause a retry
            Console.WriteLine("Thread {0}: Duplicate entry found in Purchase (error {1}: {2})... will retry",
              Thread.CurrentThread.Name, e.Number, e.Message);            
            }
          else
            {           
            Console.WriteLine("Thread {0}: MySql Error {1} in Purchase: {2}", 
              Thread.CurrentThread.Name, e.Number, e.Message);
            return(false);
            }
          } // End Catch
        } while (deadlocked);
        
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif
       
      if (!success)
        {
        IsRollback = true;
//      Console.WriteLine("Thread {0}: Purchase: Insufficient stock for order {1} - order not processed",
//        Thread.CurrentThread.Name, neworderid_out);
        neworderid_out = 0;
        }

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
  
        
