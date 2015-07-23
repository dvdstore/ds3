
/*
 * DVD Store 2 Oracle Functions - ds2oraclefns_64b_client.cs
 *
 * Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
 *
 * Provides interface functions for DVD Store driver program ds2xdriver.cs
 * Requires Oracle Data Provider for .NET
 * See ds2xdriver.cs for compilation and syntax
 *
 * Updated 12/29/09 
 *   w/ changes for Oracle Data Provider for .NET 11g Release 1 (11.1.0.7.0) (11107_w2k8_x64_production_client.zip)
 *   
 * Updated 06/24/2010 by GSK (Single instance of driver driving multiple DB instances and Parameterization of IN query)
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
using System.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;


namespace ds2xdriver
  {
  /// <summary>
  /// ds2oraclefns.cs: DVD Store 3 Oracle Functions
  /// </summary>
  public class ds2Interface
    {
#if (USE_WIN32_TIMER)
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceCounter(ref long x);
    [DllImport("kernel32.dll")]
    extern static short QueryPerformanceFrequency(ref long x);  
#endif

    int ds2Interfaceid, i;
    OracleConnection objConn;
    OracleCommand Login, New_Customer, Browse_By_Category, Browse_By_Actor, Browse_By_Title, Purchase;
    OracleCommand Get_Prod_Reviews, Get_Prod_Reviews_By_Actor, Get_Prod_Reviews_By_Title, Get_Prod_Reviews_By_Date, Get_Prod_Reviews_By_Stars;
    OracleCommand New_Member, New_Prod_Review, New_Review_Helpfulness;
    OracleCommand Browse_By_Actor_For_Membertype, Browse_by_Title_For_Membertype, Browse_By_Cat_For_Membertype;
    
    OracleParameter[] Login_prm = new OracleParameter[5];
    OracleParameter[] New_Customer_prm = new OracleParameter[20];
    OracleParameter[] Browse_By_Category_prm = new OracleParameter[3];
    OracleParameter[] Browse_By_Actor_prm = new OracleParameter[3];
    OracleParameter[] Browse_By_Title_prm = new OracleParameter[3];
    OracleParameter[] Purchase_prm = new OracleParameter[6];

    OracleParameter[] New_Member_prm = new OracleParameter[3];
    OracleParameter[] Get_Prod_Reviews_prm = new OracleParameter[3];
    OracleParameter[] Get_Prod_Reviews_By_Actor_prm = new OracleParameter[3];
    OracleParameter[] Get_Prod_Reviews_By_Title_prm = new OracleParameter[3];
    OracleParameter[] Get_Prod_Reviews_By_Date_prm = new OracleParameter[3];
    OracleParameter[] Get_Prod_Reviews_By_Stars_prm = new OracleParameter[4];
    OracleParameter[] New_Prod_Review_prm = new OracleParameter[6];
    OracleParameter[] New_Review_Helpfulness_prm = new OracleParameter[4];
    OracleParameter[] Browse_By_Actor_For_Membertype_prm = new OracleParameter[4];
    OracleParameter[] Browse_By_Title_For_Membertype_prm = new OracleParameter[4];
    OracleParameter[] Browse_By_Cat_For_Membertype_prm = new OracleParameter[4];
    
    OracleParameter Login_title_out, Login_actor_out, Login_related_title_out;
    OracleParameter Browse_By_Category_prod_id_out, Browse_By_Category_category_out, Browse_By_Category_title_out,
       Browse_By_Category_actor_out, Browse_By_Category_price_out,
       Browse_By_Category_special_out, Browse_By_Category_common_prod_id_out, Browse_By_Category_membership_item_out;
    OracleParameter Browse_By_Actor_prod_id_out, Browse_By_Actor_category_out, Browse_By_Actor_title_out,
       Browse_By_Actor_actor_out, Browse_By_Actor_price_out,   
       Browse_By_Actor_special_out, Browse_By_Actor_common_prod_id_out, Browse_By_Actor_membership_item_out;
    OracleParameter Browse_By_Title_prod_id_out, Browse_By_Title_category_out, Browse_By_Title_title_out,
       Browse_By_Title_actor_out, Browse_By_Title_price_out,
       Browse_By_Title_special_out, Browse_By_Title_common_prod_id_out, Browse_By_Title_membership_item_out;
    OracleParameter Purchase_prod_id_in, Purchase_qty_in;

    OracleParameter Get_Prod_Reviews_review_id_out, Get_Prod_Reviews_prod_id_out, 
        Get_Prod_Reviews_review_date_out, Get_Prod_Reviews_review_stars_out, 
        Get_Prod_Reviews_review_customerid_out, Get_Prod_Reviews_review_summary_out,
        Get_Prod_Reviews_review_text_out, Get_Prod_Reviews_review_helpfulness_sum_out;
    OracleParameter Get_Prod_Reviews_By_Date_review_id_out, Get_Prod_Reviews_By_Date_prod_id_out,
        Get_Prod_Reviews_By_Date_review_date_out, Get_Prod_Reviews_By_Date_review_stars_out,
        Get_Prod_Reviews_By_Date_review_customerid_out, Get_Prod_Reviews_By_Date_review_summary_out,
        Get_Prod_Reviews_By_Date_review_text_out, Get_Prod_Reviews_By_Date_review_helpfulness_sum_out;
    OracleParameter Get_Prod_Reviews_By_Stars_review_id_out, Get_Prod_Reviews_By_Stars_prod_id_out,
        Get_Prod_Reviews_By_Stars_review_date_out, Get_Prod_Reviews_By_Stars_review_stars_out,
        Get_Prod_Reviews_By_Stars_review_customerid_out, Get_Prod_Reviews_By_Stars_review_summary_out,
        Get_Prod_Reviews_By_Stars_review_text_out, Get_Prod_Reviews_By_Stars_review_helpfulness_sum_out;
    OracleParameter Get_Prod_Reviews_By_Title_title_out, Get_Prod_Reviews_By_Title_actor_out,
        Get_Prod_Reviews_By_Title_review_id_out, Get_Prod_Reviews_By_Title_prod_id_out,
        Get_Prod_Reviews_By_Title_review_date_out, Get_Prod_Reviews_By_Title_review_stars_out,
        Get_Prod_Reviews_By_Title_review_customerid_out, Get_Prod_Reviews_By_Title_review_summary_out,
        Get_Prod_Reviews_By_Title_review_text_out, Get_Prod_Reviews_By_Title_review_helpfulness_sum_out;
    OracleParameter Get_Prod_Reviews_By_Actor_title_out, Get_Prod_Reviews_By_Actor_actor_out,
        Get_Prod_Reviews_By_Actor_review_id_out, Get_Prod_Reviews_By_Actor_prod_id_out,
        Get_Prod_Reviews_By_Actor_review_date_out, Get_Prod_Reviews_By_Actor_review_stars_out,
        Get_Prod_Reviews_By_Actor_review_customerid_out, Get_Prod_Reviews_By_Actor_review_summary_out,
        Get_Prod_Reviews_By_Actor_review_text_out, Get_Prod_Reviews_By_Actor_review_helpfulness_sum_out;
      
    OracleString[] o_title_out = new OracleString[GlobalConstants.MAX_ROWS];
    OracleString[] o_actor_out = new OracleString[GlobalConstants.MAX_ROWS];
    OracleString[] o_related_title_out = new OracleString[GlobalConstants.MAX_ROWS];
    int[] o_prod_id_out = new int[GlobalConstants.MAX_ROWS];
    int[] o_special_out = new int[GlobalConstants.MAX_ROWS];
    int[] o_common_prod_id_out = new int[GlobalConstants.MAX_ROWS];
    int[] o_membership_item_out = new int[GlobalConstants.MAX_ROWS];
    byte[] o_category_out = new byte[GlobalConstants.MAX_ROWS];
    decimal[] o_price_out = new decimal[GlobalConstants.MAX_ROWS];

    int[] o_review_id_out = new int[GlobalConstants.MAX_ROWS];
    OracleString[] o_review_date_out = new OracleString[GlobalConstants.MAX_ROWS];
    int[] o_review_stars_out = new int[GlobalConstants.MAX_ROWS];
    int[] o_review_customerid_out = new int[GlobalConstants.MAX_ROWS];
    OracleString[] o_review_summary_out = new OracleString[GlobalConstants.MAX_ROWS];
    OracleString[] o_review_text_out = new OracleString[1000];
    int[] o_review_helpfulness_sum_out = new int[GlobalConstants.MAX_ROWS];
    int[] o_review_helpfulness_id_out = new int[GlobalConstants.MAX_ROWS];


    //Added by GSK (This variable will have target server name to which thread is tied to and users will login to the database on this server)
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
// 

    //Added by GSK (Overloaded constructor to to consider scenario where single Driver program is driving workload on multiple machines)
    public ds2Interface(int ds2interfaceid, string target_name)
    {
        ds2Interfaceid = ds2interfaceid;
        target_server_name = target_name;
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
      string sConnectionString = "User ID=ds3;Password=ds3;Connection Timeout=120;Data Source=" + target_server_name;
      try
        {
        objConn = new OracleConnection(sConnectionString);
        objConn.Open();
        //Console.WriteLine("Thread {0}: connected to database {1}",  Thread.CurrentThread.Name, Controller.target);
        //changed by GSK
        //Console.WriteLine("Thread {0}: connected to database {1}", Thread.CurrentThread.Name, target_server_name);
        }
      catch (OracleException e)
        {
        //Console.WriteLine("Thread {0}: Oracle error in connecting to database {1}: {2}",  Thread.CurrentThread.Name,
        //  Controller.target, e.Message);
        //Changed by GSK
        Console.WriteLine("Thread {0}: error in connecting to database {1}: {2}", Thread.CurrentThread.Name,
        target_server_name, e.Message);
        return (false);
        }
      catch (System.Exception e)
        {
        //Console.WriteLine("Thread {0}: System error in connecting to database {1}: {2}",  Thread.CurrentThread.Name,
        //  Controller.target, e.Message);
        //return(false);
        //Changed by GSK
        Console.WriteLine("Thread {0}: System error in connecting to database {1}: {2}", Thread.CurrentThread.Name,
        target_server_name, e.Message);
        return (false);
        }

      // Set up Oracle stored procedure calls and associated parameters
      
      // Login
      Login = new OracleCommand("", objConn);
      Login.CommandText = "Login";
      Login.CommandType = CommandType.StoredProcedure;
      Login_prm[0] = Login.Parameters.Add("username_in", OracleDbType.Varchar2, ParameterDirection.Input);
      Login_prm[1] = Login.Parameters.Add("password_in", OracleDbType.Varchar2, ParameterDirection.Input);
      Login_prm[2] = Login.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Login_prm[3] = Login.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Login_prm[4] = Login.Parameters.Add("customerid_out", OracleDbType.Int32, ParameterDirection.Output);
      
      Login_title_out = 
        Login.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Login_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Login_title_out.Value = null;
      Login_title_out.Size = GlobalConstants.MAX_ROWS;     
      Login_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Login_title_out.ArrayBindSize[i] = 50;
            
      Login_actor_out = 
        Login.Parameters.Add("actor_out", OracleDbType.Varchar2, ParameterDirection.Output);      
      Login_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Login_actor_out.Value = null;
      Login_actor_out.Size = GlobalConstants.MAX_ROWS;  
      Login_actor_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Login_actor_out.ArrayBindSize[i] = 50;
                  
      Login_related_title_out = 
        Login.Parameters.Add("related_title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Login_related_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Login_related_title_out.Value = null;
      Login_related_title_out.Size = GlobalConstants.MAX_ROWS;     
      Login_related_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Login_related_title_out.ArrayBindSize[i] = 50;
            
      // New_Customer
      New_Customer = new OracleCommand("", objConn);
      New_Customer.CommandText = "New_Customer";
      New_Customer.CommandType = CommandType.StoredProcedure; 
      New_Customer_prm[0] = 
        New_Customer.Parameters.Add("firstname_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[1] = 
        New_Customer.Parameters.Add("lastname_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[2] = 
        New_Customer.Parameters.Add("address1_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[3] = 
        New_Customer.Parameters.Add("address2_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[4] = 
        New_Customer.Parameters.Add("city_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[5] = 
        New_Customer.Parameters.Add("state_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[6] = 
        New_Customer.Parameters.Add("zip_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Customer_prm[7] = 
        New_Customer.Parameters.Add("country_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[8] = 
        New_Customer.Parameters.Add("region_in", OracleDbType.Int16, ParameterDirection.Input);
      New_Customer_prm[9] = 
        New_Customer.Parameters.Add("email_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[10] = 
        New_Customer.Parameters.Add("phone_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[11] = 
        New_Customer.Parameters.Add("creditcardtype_in", OracleDbType.Int16, ParameterDirection.Input);
      New_Customer_prm[12] = 
        New_Customer.Parameters.Add("creditcard_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[13] = 
        New_Customer.Parameters.Add("creditcardexpiration_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[14] = 
        New_Customer.Parameters.Add("username_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[15] = 
        New_Customer.Parameters.Add("password_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Customer_prm[16] = 
        New_Customer.Parameters.Add("age_in", OracleDbType.Int16, ParameterDirection.Input);                   
      New_Customer_prm[17] = 
        New_Customer.Parameters.Add("income_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Customer_prm[18] = 
        New_Customer.Parameters.Add("gender_in", OracleDbType.Varchar2, 1, ParameterDirection.Input);
      New_Customer_prm[19] = 
        New_Customer.Parameters.Add("customerid_out", OracleDbType.Int32, ParameterDirection.Output);                   

       //New Member
      New_Member = new OracleCommand("", objConn);
      New_Member.CommandText = "New_Member";
      New_Member.CommandType = CommandType.StoredProcedure; 
      New_Member_prm[0] =
              New_Member.Parameters.Add("customerid_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Member_prm[1] =
          New_Member.Parameters.Add("membershiplevel_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Member_prm[2] =
          New_Member.Parameters.Add("customerid_out", OracleDbType.Int32, ParameterDirection.Output);

      //Browse_By_Category
      Browse_By_Category = new OracleCommand("", objConn);
      Browse_By_Category.CommandText = "Browse_By_Category";
      Browse_By_Category.CommandType = CommandType.StoredProcedure;
      
      Browse_By_Category_prm[0] = 
        Browse_By_Category.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Browse_By_Category_prm[1] = 
        Browse_By_Category.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_prm[2] = 
        Browse_By_Category.Parameters.Add("category_in", OracleDbType.Int32, ParameterDirection.Input);
      
      Browse_By_Category_prod_id_out = 
        Browse_By_Category.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_prod_id_out.Value = null;
      Browse_By_Category_prod_id_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Category_category_out = 
        Browse_By_Category.Parameters.Add("category_out", OracleDbType.Byte, ParameterDirection.Output);
      Browse_By_Category_category_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_category_out.Value = null;
      Browse_By_Category_category_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Category_title_out = 
        Browse_By_Category.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Category_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_title_out.Value = null;
      Browse_By_Category_title_out.Size = GlobalConstants.MAX_ROWS;     
      Browse_By_Category_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Category_title_out.ArrayBindSize[i] = 50;
            
      Browse_By_Category_actor_out = 
        Browse_By_Category.Parameters.Add("actor_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Category_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_actor_out.Value = null;
      Browse_By_Category_actor_out.Size = GlobalConstants.MAX_ROWS;  
      Browse_By_Category_actor_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Category_actor_out.ArrayBindSize[i] = 50;      
            
      Browse_By_Category_price_out = 
        Browse_By_Category.Parameters.Add("price_out", OracleDbType.Decimal, ParameterDirection.Output);
      Browse_By_Category_price_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_price_out.Value = null;
      Browse_By_Category_price_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Category_special_out = 
        Browse_By_Category.Parameters.Add("special_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_special_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_special_out.Value = null;
      Browse_By_Category_special_out.Size = GlobalConstants.MAX_ROWS;
              
      Browse_By_Category_common_prod_id_out =
        Browse_By_Category.Parameters.Add("common_prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_common_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_common_prod_id_out.Value = null;
      Browse_By_Category_common_prod_id_out.Size = GlobalConstants.MAX_ROWS;

      Browse_By_Category_membership_item_out =
        Browse_By_Category.Parameters.Add("membership_item_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Category_membership_item_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Category_membership_item_out.Value = null;
      Browse_By_Category_membership_item_out.Size = GlobalConstants.MAX_ROWS;
            
      //Browse_By_Actor
      Browse_By_Actor = new OracleCommand("", objConn);
      Browse_By_Actor.CommandText = "Browse_By_Actor";
      Browse_By_Actor.CommandType = CommandType.StoredProcedure;
      
      Browse_By_Actor_prm[0] = 
        Browse_By_Actor.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Browse_By_Actor_prm[1] = 
        Browse_By_Actor.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_prm[2] = 
        Browse_By_Actor.Parameters.Add("actor_in", OracleDbType.Varchar2, ParameterDirection.Input);
      
      Browse_By_Actor_prod_id_out = 
        Browse_By_Actor.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_prod_id_out.Value = null;
      Browse_By_Actor_prod_id_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Actor_category_out = 
        Browse_By_Actor.Parameters.Add("category_out", OracleDbType.Byte, ParameterDirection.Output);
      Browse_By_Actor_category_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_category_out.Value = null;
      Browse_By_Actor_category_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Actor_title_out = 
        Browse_By_Actor.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Actor_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_title_out.Value = null;
      Browse_By_Actor_title_out.Size = GlobalConstants.MAX_ROWS;     
      Browse_By_Actor_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Actor_title_out.ArrayBindSize[i] = 50;
      
      Browse_By_Actor_actor_out = 
        Browse_By_Actor.Parameters.Add("actor_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Actor_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_actor_out.Value = null;
      Browse_By_Actor_actor_out.Size = GlobalConstants.MAX_ROWS;  
      Browse_By_Actor_actor_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Actor_actor_out.ArrayBindSize[i] = 50;      
            
      Browse_By_Actor_price_out = 
        Browse_By_Actor.Parameters.Add("price_out", OracleDbType.Decimal, ParameterDirection.Output);
      Browse_By_Actor_price_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_price_out.Value = null;
      Browse_By_Actor_price_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Actor_special_out = 
        Browse_By_Actor.Parameters.Add("special_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_special_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_special_out.Value = null;
      Browse_By_Actor_special_out.Size = GlobalConstants.MAX_ROWS;
              
      Browse_By_Actor_common_prod_id_out = 
        Browse_By_Actor.Parameters.Add("common_prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_common_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_common_prod_id_out.Value = null;
      Browse_By_Actor_common_prod_id_out.Size = GlobalConstants.MAX_ROWS;

      Browse_By_Actor_membership_item_out =
      Browse_By_Actor.Parameters.Add("membership_item_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Actor_membership_item_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Actor_membership_item_out.Value = null;
      Browse_By_Actor_membership_item_out.Size = GlobalConstants.MAX_ROWS;
                
      //Browse_By_Title
      Browse_By_Title = new OracleCommand("", objConn);
      Browse_By_Title.CommandText = "Browse_By_Title";
      Browse_By_Title.CommandType = CommandType.StoredProcedure;
      
      Browse_By_Title_prm[0] = 
        Browse_By_Title.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Browse_By_Title_prm[1] = 
        Browse_By_Title.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_prm[2] = 
        Browse_By_Title.Parameters.Add("title_in", OracleDbType.Varchar2, ParameterDirection.Input);
      
      Browse_By_Title_prod_id_out = 
        Browse_By_Title.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_prod_id_out.Value = null;
      Browse_By_Title_prod_id_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Title_category_out = 
        Browse_By_Title.Parameters.Add("category_out", OracleDbType.Byte, ParameterDirection.Output);
      Browse_By_Title_category_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_category_out.Value = null;
      Browse_By_Title_category_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Title_title_out = 
        Browse_By_Title.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Title_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_title_out.Value = null;
      Browse_By_Title_title_out.Size = GlobalConstants.MAX_ROWS;     
      Browse_By_Title_title_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Title_title_out.ArrayBindSize[i] = 50;
      
      Browse_By_Title_actor_out = 
        Browse_By_Title.Parameters.Add("actor_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Browse_By_Title_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_actor_out.Value = null;
      Browse_By_Title_actor_out.Size = GlobalConstants.MAX_ROWS;  
      Browse_By_Title_actor_out.ArrayBindSize = new int [GlobalConstants.MAX_ROWS];
      for (i=0; i<GlobalConstants.MAX_ROWS; i++) Browse_By_Title_actor_out.ArrayBindSize[i] = 50;      
            
      Browse_By_Title_price_out = 
        Browse_By_Title.Parameters.Add("price_out", OracleDbType.Decimal, ParameterDirection.Output);
      Browse_By_Title_price_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_price_out.Value = null;
      Browse_By_Title_price_out.Size = GlobalConstants.MAX_ROWS;
      
      Browse_By_Title_special_out = 
        Browse_By_Title.Parameters.Add("special_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_special_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_special_out.Value = null;
      Browse_By_Title_special_out.Size = GlobalConstants.MAX_ROWS;
              
      Browse_By_Title_common_prod_id_out = 
        Browse_By_Title.Parameters.Add("common_prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_common_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_common_prod_id_out.Value = null;
      Browse_By_Title_common_prod_id_out.Size = GlobalConstants.MAX_ROWS;

      Browse_By_Title_membership_item_out =
      Browse_By_Title.Parameters.Add("membership_item_out", OracleDbType.Int32, ParameterDirection.Output);
      Browse_By_Title_membership_item_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Browse_By_Title_membership_item_out.Value = null;
      Browse_By_Title_membership_item_out.Size = GlobalConstants.MAX_ROWS;
        
      // Get_Prod_Reviews

      Get_Prod_Reviews = new OracleCommand("", objConn);
      Get_Prod_Reviews.CommandText = "Get_Prod_Reviews";
      Get_Prod_Reviews.CommandType = CommandType.StoredProcedure;

      Get_Prod_Reviews_prm[0] =
        Get_Prod_Reviews.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Get_Prod_Reviews_prm[1] =
        Get_Prod_Reviews.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_prm[2] =
        Get_Prod_Reviews.Parameters.Add("prod_in", OracleDbType.Int32, ParameterDirection.Input);

      Get_Prod_Reviews_review_id_out =
        Get_Prod_Reviews.Parameters.Add("review_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_review_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_review_id_out.Value = null;
      Get_Prod_Reviews_review_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_prod_id_out =
        Get_Prod_Reviews.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_prod_id_out.Value = null;
      Get_Prod_Reviews_prod_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_review_date_out =
        Get_Prod_Reviews.Parameters.Add("review_date_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_review_date_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_review_date_out.Value = null;
      Get_Prod_Reviews_review_date_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_review_date_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_review_date_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_review_stars_out =
      Get_Prod_Reviews.Parameters.Add("review_stars_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_review_stars_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_review_stars_out.Value = null;
      Get_Prod_Reviews_review_stars_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_review_customerid_out =
    Get_Prod_Reviews.Parameters.Add("review_customerid_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_review_customerid_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_review_customerid_out.Value = null;
      Get_Prod_Reviews_review_customerid_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_review_summary_out =
        Get_Prod_Reviews.Parameters.Add("review_summary_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_review_summary_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_review_summary_out.Value = null;
      Get_Prod_Reviews_review_summary_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_review_summary_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_review_summary_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_review_text_out =
      Get_Prod_Reviews.Parameters.Add("review_text_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_review_text_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_review_text_out.Value = null;
      Get_Prod_Reviews_review_text_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_review_text_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_review_text_out.ArrayBindSize[i] = 1000;

      Get_Prod_Reviews_review_helpfulness_sum_out =
        Get_Prod_Reviews.Parameters.Add("review_helpfulness_sum_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_review_helpfulness_sum_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_review_helpfulness_sum_out.Value = null;
      Get_Prod_Reviews_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;

      //Get_Prod_Reviews_By_Date
      Get_Prod_Reviews_By_Date = new OracleCommand("", objConn);
      Get_Prod_Reviews_By_Date.CommandText = "Get_Prod_Reviews_By_Date";
      Get_Prod_Reviews_By_Date.CommandType = CommandType.StoredProcedure;

      Get_Prod_Reviews_By_Date_prm[0] =
        Get_Prod_Reviews_By_Date.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Get_Prod_Reviews_By_Date_prm[1] =
        Get_Prod_Reviews_By_Date.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_prm[2] =
        Get_Prod_Reviews_By_Date.Parameters.Add("prod_in", OracleDbType.Int32, ParameterDirection.Input);

      Get_Prod_Reviews_By_Date_review_id_out =
        Get_Prod_Reviews_By_Date.Parameters.Add("review_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_review_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Date_review_id_out.Value = null;
      Get_Prod_Reviews_By_Date_review_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Date_prod_id_out =
        Get_Prod_Reviews_By_Date.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Date_prod_id_out.Value = null;
      Get_Prod_Reviews_By_Date_prod_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Date_review_date_out =
        Get_Prod_Reviews_By_Date.Parameters.Add("review_date_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_review_date_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Date_review_date_out.Value = null;
      Get_Prod_Reviews_By_Date_review_date_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Date_review_date_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Date_review_date_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Date_review_stars_out =
      Get_Prod_Reviews_By_Date.Parameters.Add("review_stars_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_review_stars_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Date_review_stars_out.Value = null;
      Get_Prod_Reviews_By_Date_review_stars_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Date_review_customerid_out =
      Get_Prod_Reviews_By_Date.Parameters.Add("review_customerid_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_review_customerid_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Date_review_customerid_out.Value = null;
      Get_Prod_Reviews_By_Date_review_customerid_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Date_review_summary_out =
        Get_Prod_Reviews_By_Date.Parameters.Add("review_summary_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_review_summary_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Date_review_summary_out.Value = null;
      Get_Prod_Reviews_By_Date_review_summary_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Date_review_summary_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Date_review_summary_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Date_review_text_out =
      Get_Prod_Reviews_By_Date.Parameters.Add("review_text_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_review_text_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Date_review_text_out.Value = null;
      Get_Prod_Reviews_By_Date_review_text_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Date_review_text_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Date_review_text_out.ArrayBindSize[i] = 1000;

      Get_Prod_Reviews_By_Date_review_helpfulness_sum_out =
        Get_Prod_Reviews_By_Date.Parameters.Add("review_helpfulness_sum_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Date_review_helpfulness_sum_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Date_review_helpfulness_sum_out.Value = null;
      Get_Prod_Reviews_By_Date_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;

      //Get_Prod_Reviews_By_Stars
      Get_Prod_Reviews_By_Stars = new OracleCommand("", objConn);
      Get_Prod_Reviews_By_Stars.CommandText = "Get_Prod_Reviews_By_Stars";
      Get_Prod_Reviews_By_Stars.CommandType = CommandType.StoredProcedure;

      Get_Prod_Reviews_By_Stars_prm[0] =
        Get_Prod_Reviews_By_Stars.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Get_Prod_Reviews_By_Stars_prm[1] =
        Get_Prod_Reviews_By_Stars.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_prm[2] =
        Get_Prod_Reviews_By_Stars.Parameters.Add("prod_in", OracleDbType.Int32, ParameterDirection.Input);
      Get_Prod_Reviews_By_Stars_prm[3] =
              Get_Prod_Reviews_By_Stars.Parameters.Add("stars_in", OracleDbType.Int32, ParameterDirection.Input);

      Get_Prod_Reviews_By_Stars_review_id_out =
        Get_Prod_Reviews_By_Stars.Parameters.Add("review_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_review_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Stars_review_id_out.Value = null;
      Get_Prod_Reviews_By_Stars_review_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Stars_prod_id_out =
        Get_Prod_Reviews_By_Stars.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Stars_prod_id_out.Value = null;
      Get_Prod_Reviews_By_Stars_prod_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Stars_review_date_out =
        Get_Prod_Reviews_By_Stars.Parameters.Add("review_date_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_review_date_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Stars_review_date_out.Value = null;
      Get_Prod_Reviews_By_Stars_review_date_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Stars_review_date_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Stars_review_date_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Stars_review_stars_out =
        Get_Prod_Reviews_By_Stars.Parameters.Add("review_stars_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_review_stars_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Stars_review_stars_out.Value = null;
      Get_Prod_Reviews_By_Stars_review_stars_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Stars_review_customerid_out =
    Get_Prod_Reviews_By_Stars.Parameters.Add("review_customerid_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_review_customerid_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Stars_review_customerid_out.Value = null;
      Get_Prod_Reviews_By_Stars_review_customerid_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Stars_review_summary_out =
        Get_Prod_Reviews_By_Stars.Parameters.Add("review_summary_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_review_summary_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Stars_review_summary_out.Value = null;
      Get_Prod_Reviews_By_Stars_review_summary_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Stars_review_summary_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Stars_review_summary_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Stars_review_text_out =
      Get_Prod_Reviews_By_Stars.Parameters.Add("review_text_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_review_text_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Stars_review_text_out.Value = null;
      Get_Prod_Reviews_By_Stars_review_text_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Stars_review_text_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Stars_review_text_out.ArrayBindSize[i] = 1000;

      Get_Prod_Reviews_By_Stars_review_helpfulness_sum_out =
        Get_Prod_Reviews_By_Stars.Parameters.Add("review_helpfulness_sum_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Stars_review_helpfulness_sum_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Stars_review_helpfulness_sum_out.Value = null;
      Get_Prod_Reviews_By_Stars_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;

        //Get_Prod_Reviews_By_Title

      Get_Prod_Reviews_By_Title = new OracleCommand("", objConn);
      Get_Prod_Reviews_By_Title.CommandText = "Get_Prod_Reviews_By_Title";
      Get_Prod_Reviews_By_Title.CommandType = CommandType.StoredProcedure;

      Get_Prod_Reviews_By_Title_prm[0] =
        Get_Prod_Reviews_By_Title.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Get_Prod_Reviews_By_Title_prm[1] =
        Get_Prod_Reviews_By_Title.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_prm[2] =
        Get_Prod_Reviews_By_Title.Parameters.Add("title_in", OracleDbType.Varchar2, ParameterDirection.Input);

      Get_Prod_Reviews_By_Title_title_out =
              Get_Prod_Reviews_By_Title.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_title_out.Value = null;
      Get_Prod_Reviews_By_Title_title_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Title_title_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Title_title_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Title_actor_out =
        Get_Prod_Reviews_By_Title.Parameters.Add("actor_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_actor_out.Value = null;
      Get_Prod_Reviews_By_Title_actor_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Title_actor_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Title_actor_out.ArrayBindSize[i] = 50;      

      Get_Prod_Reviews_By_Title_review_id_out =
        Get_Prod_Reviews_By_Title.Parameters.Add("review_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_review_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_review_id_out.Value = null;
      Get_Prod_Reviews_By_Title_review_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Title_prod_id_out =
        Get_Prod_Reviews_By_Title.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_prod_id_out.Value = null;
      Get_Prod_Reviews_By_Title_prod_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Title_review_date_out =
        Get_Prod_Reviews_By_Title.Parameters.Add("review_date_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_review_date_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_review_date_out.Value = null;
      Get_Prod_Reviews_By_Title_review_date_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Title_review_date_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Title_review_date_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Title_review_stars_out =
      Get_Prod_Reviews_By_Title.Parameters.Add("review_stars_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_review_stars_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_review_stars_out.Value = null;
      Get_Prod_Reviews_By_Title_review_stars_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Title_review_customerid_out =
    Get_Prod_Reviews_By_Title.Parameters.Add("review_customerid_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_review_customerid_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_review_customerid_out.Value = null;
      Get_Prod_Reviews_By_Title_review_customerid_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Title_review_summary_out =
        Get_Prod_Reviews_By_Title.Parameters.Add("review_summary_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_review_summary_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_review_summary_out.Value = null;
      Get_Prod_Reviews_By_Title_review_summary_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Title_review_summary_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Title_review_summary_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Title_review_text_out =
      Get_Prod_Reviews_By_Title.Parameters.Add("review_text_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_review_text_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_review_text_out.Value = null;
      Get_Prod_Reviews_By_Title_review_text_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Title_review_text_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Title_review_text_out.ArrayBindSize[i] = 1000;

      Get_Prod_Reviews_By_Title_review_helpfulness_sum_out =
        Get_Prod_Reviews_By_Title.Parameters.Add("review_helpfulness_sum_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Title_review_helpfulness_sum_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Title_review_helpfulness_sum_out.Value = null;
      Get_Prod_Reviews_By_Title_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;

      //Get_Prod_Reviews_By_Actor

      Get_Prod_Reviews_By_Actor = new OracleCommand("", objConn);
      Get_Prod_Reviews_By_Actor.CommandText = "Get_Prod_Reviews_By_Actor";
      Get_Prod_Reviews_By_Actor.CommandType = CommandType.StoredProcedure;

      Get_Prod_Reviews_By_Actor_prm[0] =
        Get_Prod_Reviews_By_Actor.Parameters.Add("batch_size", OracleDbType.Int32, ParameterDirection.Input);
      Get_Prod_Reviews_By_Actor_prm[1] =
        Get_Prod_Reviews_By_Actor.Parameters.Add("found", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_prm[2] =
        Get_Prod_Reviews_By_Actor.Parameters.Add("actor_in", OracleDbType.Varchar2, ParameterDirection.Input);

      Get_Prod_Reviews_By_Actor_title_out =
              Get_Prod_Reviews_By_Actor.Parameters.Add("title_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_title_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_title_out.Value = null;
      Get_Prod_Reviews_By_Actor_title_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Actor_title_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Actor_title_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Actor_actor_out =
        Get_Prod_Reviews_By_Actor.Parameters.Add("actor_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_actor_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_actor_out.Value = null;
      Get_Prod_Reviews_By_Actor_actor_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Actor_actor_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Actor_actor_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Actor_review_id_out =
        Get_Prod_Reviews_By_Actor.Parameters.Add("review_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_review_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_review_id_out.Value = null;
      Get_Prod_Reviews_By_Actor_review_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Actor_prod_id_out =
        Get_Prod_Reviews_By_Actor.Parameters.Add("prod_id_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_prod_id_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_prod_id_out.Value = null;
      Get_Prod_Reviews_By_Actor_prod_id_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Actor_review_date_out =
        Get_Prod_Reviews_By_Actor.Parameters.Add("review_date_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_review_date_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_review_date_out.Value = null;
      Get_Prod_Reviews_By_Actor_review_date_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Actor_review_date_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Actor_review_date_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Actor_review_stars_out =
      Get_Prod_Reviews_By_Actor.Parameters.Add("review_stars_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_review_stars_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_review_stars_out.Value = null;
      Get_Prod_Reviews_By_Actor_review_stars_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Actor_review_customerid_out =
    Get_Prod_Reviews_By_Actor.Parameters.Add("review_customerid_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_review_customerid_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_review_customerid_out.Value = null;
      Get_Prod_Reviews_By_Actor_review_customerid_out.Size = GlobalConstants.MAX_ROWS;

      Get_Prod_Reviews_By_Actor_review_summary_out =
        Get_Prod_Reviews_By_Actor.Parameters.Add("review_summary_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_review_summary_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_review_summary_out.Value = null;
      Get_Prod_Reviews_By_Actor_review_summary_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Actor_review_summary_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Actor_review_summary_out.ArrayBindSize[i] = 50;

      Get_Prod_Reviews_By_Actor_review_text_out =
      Get_Prod_Reviews_By_Actor.Parameters.Add("review_text_out", OracleDbType.Varchar2, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_review_text_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_review_text_out.Value = null;
      Get_Prod_Reviews_By_Actor_review_text_out.Size = GlobalConstants.MAX_ROWS;
      Get_Prod_Reviews_By_Actor_review_text_out.ArrayBindSize = new int[GlobalConstants.MAX_ROWS];
      for (i = 0; i < GlobalConstants.MAX_ROWS; i++) Get_Prod_Reviews_By_Actor_review_text_out.ArrayBindSize[i] = 1000;

      Get_Prod_Reviews_By_Actor_review_helpfulness_sum_out =
        Get_Prod_Reviews_By_Actor.Parameters.Add("review_helpfulness_sum_out", OracleDbType.Int32, ParameterDirection.Output);
      Get_Prod_Reviews_By_Actor_review_helpfulness_sum_out.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Get_Prod_Reviews_By_Actor_review_helpfulness_sum_out.Value = null;
      Get_Prod_Reviews_By_Actor_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;

       //New Prod Reviews
      New_Prod_Review = new OracleCommand("", objConn);
      New_Prod_Review.CommandText = "New_Prod_Review";
      New_Prod_Review.CommandType = CommandType.StoredProcedure; 
      New_Prod_Review_prm[0] = 
        New_Prod_Review.Parameters.Add("prod_id_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Prod_Review_prm[1] = 
        New_Prod_Review.Parameters.Add("stars_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Prod_Review_prm[2] = 
        New_Prod_Review.Parameters.Add("customerid_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Prod_Review_prm[3] = 
        New_Prod_Review.Parameters.Add("review_summary_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Prod_Review_prm[4] = 
        New_Prod_Review.Parameters.Add("review_text_in", OracleDbType.Varchar2, ParameterDirection.Input);
      New_Prod_Review_prm[5] = 
        New_Prod_Review.Parameters.Add("review_id_out", OracleDbType.Int32, ParameterDirection.Output);

      //New Review Helpfulness
      New_Review_Helpfulness = new OracleCommand("", objConn);
      New_Review_Helpfulness.CommandText = "New_Review_Helpfulness";
      New_Review_Helpfulness.CommandType = CommandType.StoredProcedure;
      New_Review_Helpfulness_prm[0] =
          New_Review_Helpfulness.Parameters.Add("reviewid_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Review_Helpfulness_prm[1] =
          New_Review_Helpfulness.Parameters.Add("customerid_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Review_Helpfulness_prm[2] =
          New_Review_Helpfulness.Parameters.Add("review_helpfulness_in", OracleDbType.Int32, ParameterDirection.Input);
      New_Review_Helpfulness_prm[3] =
          New_Review_Helpfulness.Parameters.Add("customerid_out", OracleDbType.Int32, ParameterDirection.Output);

      //Purchase
      Purchase = new OracleCommand("", objConn);
      Purchase.CommandText = "Purchase";
      Purchase.CommandType = CommandType.StoredProcedure;
      
      Purchase_prm[0] = 
        Purchase.Parameters.Add("customerid_in", OracleDbType.Int32, ParameterDirection.Input);
      Purchase_prm[1] = 
        Purchase.Parameters.Add("number_items", OracleDbType.Int32, ParameterDirection.Input);
      Purchase_prm[2] = 
        Purchase.Parameters.Add("netamount_in", OracleDbType.Decimal, ParameterDirection.Input);
      Purchase_prm[3] = 
        Purchase.Parameters.Add("taxamount_in", OracleDbType.Decimal, ParameterDirection.Input);
      Purchase_prm[4] = 
        Purchase.Parameters.Add("totalamount_in", OracleDbType.Decimal, ParameterDirection.Input);
      Purchase_prm[5] = 
        Purchase.Parameters.Add("neworderid_out", OracleDbType.Int32, ParameterDirection.Output);
              
      Purchase_prod_id_in = 
        Purchase.Parameters.Add("prod_id_in", OracleDbType.Int32, ParameterDirection.Input);
      Purchase_prod_id_in.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Purchase_prod_id_in.Size = 10;
           
      Purchase_qty_in = 
        Purchase.Parameters.Add("qty_in", OracleDbType.Int32, ParameterDirection.Input);
      Purchase_qty_in.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
      Purchase_qty_in.Size = 10;
   
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

      int batch_size = 10;
      
      Login_prm[0].Value = username_in;
      Login_prm[1].Value = password_in;
      Login_prm[2].Value = batch_size;
         
#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif
        
      try 
        {
        Login.ExecuteNonQuery();
        rows_returned = Convert.ToInt32(Login_prm[3].Value.ToString());
        customerid_out = Convert.ToInt32(Login_prm[4].Value.ToString());
        o_title_out = (OracleString[]) Login_title_out.Value;
        o_actor_out = (OracleString[]) Login_actor_out.Value;
        o_related_title_out = (OracleString[]) Login_related_title_out.Value;
        }
      catch (OracleException e) 
        {
        Console.WriteLine("Thread {0}: Oracle Error in Login: {1}", Thread.CurrentThread.Name, e.Message);
        return (false);
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Thread {0}: System Error in Login: {1}", Thread.CurrentThread.Name, e.Message);
        return (false);
        }
      
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif
            
//    Console.WriteLine("Thread {0}: {1} successfully logged in;   rows_returned={2}  customerid_out={3}",  
//     Thread.CurrentThread.Name, username_in, rows_returned, customerid_out);
      for (int i_row=0; i_row<rows_returned; i_row++)
        {
        title_out[i_row] = o_title_out[i_row].ToString();
        actor_out[i_row] = o_actor_out[i_row].ToString();
        related_title_out[i_row] = o_related_title_out[i_row].ToString();
//      Console.WriteLine("  title= {0}  actor= {1}  related_title= {2}",
//        title_out[i_row], actor_out[i_row], related_title_out[i_row]);
        }

      Login_title_out.Size=GlobalConstants.MAX_ROWS;
      Login_actor_out.Size=GlobalConstants.MAX_ROWS;
      Login_related_title_out.Size=GlobalConstants.MAX_ROWS;
                    
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
     
      New_Customer_prm[0].Value = firstname_in;
      New_Customer_prm[1].Value = lastname_in;
      New_Customer_prm[2].Value = address1_in;
      New_Customer_prm[3].Value = address2_in;
      New_Customer_prm[4].Value = city_in;
      New_Customer_prm[5].Value = state_in;
      New_Customer_prm[6].Value = (zip_in=="") ? 0 : Convert.ToInt32(zip_in);
      New_Customer_prm[7].Value = country_in;
      New_Customer_prm[8].Value = region_in;
      New_Customer_prm[9].Value = email_in;
      New_Customer_prm[10].Value = phone_in;
      New_Customer_prm[11].Value = creditcardtype_in;
      New_Customer_prm[12].Value = creditcard_in;
      New_Customer_prm[13].Value = creditcardexpiration_in;
      New_Customer_prm[14].Value = username_in;
      New_Customer_prm[15].Value = password_in;
      New_Customer_prm[16].Value = age_in;
      New_Customer_prm[17].Value = income_in;
      New_Customer_prm[18].Value = gender_in;
    

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  
      
      try 
        {
        New_Customer.ExecuteNonQuery();
        customerid_out = Convert.ToInt32(New_Customer_prm[19].Value.ToString());
        }       
      catch (OracleException e) 
        {
        Console.WriteLine("Thread {0}: Oracle Error in New_Customer.ExecuteNonQuery(): {1}", 
          Thread.CurrentThread.Name, e.Message);
        return(false);
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Thread {0}: System Error in New_Customer.ExecuteNonQuery(): {1}", 
          Thread.CurrentThread.Name, e.Message);
        return(false);
        }
     
            
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
     
      New_Member_prm[0].Value = customerid_in;
      New_Member_prm[1].Value = membershiplevel_in;
        

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  
      
      try 
        {
        New_Member.ExecuteNonQuery();
        customerid_out = Convert.ToInt32(New_Member_prm[2].Value.ToString());
        }       
      catch (OracleException e) 
        {
        Console.WriteLine("Thread {0}: Oracle Error in New_Member.ExecuteNonQuery(): {1}", 
          Thread.CurrentThread.Name, e.Message);
        return(false);
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Thread {0}: System Error in New_Member.ExecuteNonQuery(): {1}", 
          Thread.CurrentThread.Name, e.Message);
        return(false);
        }
     
            
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
          Browse_By_Category_prm[0].Value = batch_size_in;
          Browse_By_Category_prm[2].Value = Convert.ToInt32(browse_category_in);
          data_in = browse_category_in;
          break;
        case "actor":
          Browse_By_Actor_prm[0].Value = batch_size_in;
          Browse_By_Actor_prm[2].Value = browse_actor_in;
          data_in = browse_actor_in;
          break;
        case "title":
          Browse_By_Title_prm[0].Value = batch_size_in;
          Browse_By_Title_prm[2].Value = browse_title_in;
          data_in = browse_title_in;
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
        switch(browse_type_in)
          {
          case "category":
            Browse_By_Category.ExecuteNonQuery();
            rows_returned = Convert.ToInt32(Browse_By_Category_prm[1].Value.ToString());
            for (i=0; i<rows_returned; i++)
              {
              o_prod_id_out[i] = Convert.ToInt32(((Browse_By_Category_prod_id_out.Value as Array).GetValue(i)).ToString());
              o_category_out[i] = Convert.ToByte(((Browse_By_Category_category_out.Value as Array).GetValue(i)).ToString());
              o_price_out[i] = Convert.ToDecimal(((Browse_By_Category_price_out.Value as Array).GetValue(i)).ToString());
              o_special_out[i] = Convert.ToInt32(((Browse_By_Category_special_out.Value as Array).GetValue(i)).ToString());
              o_common_prod_id_out[i] = Convert.ToInt32(((Browse_By_Category_common_prod_id_out.Value as Array).GetValue(i)).ToString());            
              }
            o_title_out = (OracleString[]) Browse_By_Category_title_out.Value;
            o_actor_out = (OracleString[]) Browse_By_Category_actor_out.Value;
            break;
          case "actor":
            Browse_By_Actor.ExecuteNonQuery();        
            rows_returned = Convert.ToInt32(Browse_By_Actor_prm[1].Value.ToString());
            for (i=0; i<rows_returned; i++)
              {
              o_prod_id_out[i] = Convert.ToInt32(((Browse_By_Actor_prod_id_out.Value as Array).GetValue(i)).ToString());
              o_category_out[i] = Convert.ToByte(((Browse_By_Actor_category_out.Value as Array).GetValue(i)).ToString());
              o_price_out[i] = Convert.ToDecimal(((Browse_By_Actor_price_out.Value as Array).GetValue(i)).ToString());
              o_special_out[i] = Convert.ToInt32(((Browse_By_Actor_special_out.Value as Array).GetValue(i)).ToString());
              o_common_prod_id_out[i] = Convert.ToInt32(((Browse_By_Actor_common_prod_id_out.Value as Array).GetValue(i)).ToString());            
              }
            o_title_out = (OracleString[]) Browse_By_Actor_title_out.Value;
            o_actor_out = (OracleString[]) Browse_By_Actor_actor_out.Value;
            break;
          case "title":
            Browse_By_Title.ExecuteNonQuery();
            rows_returned = Convert.ToInt32(Browse_By_Title_prm[1].Value.ToString());
            for (i=0; i<rows_returned; i++)
              {
              o_prod_id_out[i] = Convert.ToInt32(((Browse_By_Title_prod_id_out.Value as Array).GetValue(i)).ToString());
              o_category_out[i] = Convert.ToByte(((Browse_By_Title_category_out.Value as Array).GetValue(i)).ToString());
              o_price_out[i] = Convert.ToDecimal(((Browse_By_Title_price_out.Value as Array).GetValue(i)).ToString());
              o_special_out[i] = Convert.ToInt32(((Browse_By_Title_special_out.Value as Array).GetValue(i)).ToString());
              o_common_prod_id_out[i] = Convert.ToInt32(((Browse_By_Title_common_prod_id_out.Value as Array).GetValue(i)).ToString());            
              }
            o_title_out = (OracleString[]) Browse_By_Title_title_out.Value;
            o_actor_out = (OracleString[]) Browse_By_Title_actor_out.Value;
            break;
          }
        }
      catch (OracleException e) 
        {
        Console.WriteLine("Thread {0}: Oracle Error in Browse: {1}", Thread.CurrentThread.Name, e.Message);
        return(false);
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Thread {0}: System Error in Browse: {1}", Thread.CurrentThread.Name, e.Message);
        return(false);
        }
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif  

//    Console.WriteLine("Thread {0}: Browse successful: type= {1}  rows_returned={2}",  
//       Thread.CurrentThread.Name, browse_type_in, rows_returned);
      for (int i_row=0; i_row<rows_returned; i_row++)
        {
        prod_id_out[i_row] = o_prod_id_out[i_row];
        category_out[i_row] = o_category_out[i_row];
        title_out[i_row] = o_title_out[i_row].ToString();
        actor_out[i_row] = o_actor_out[i_row].ToString();
        price_out[i_row] = o_price_out[i_row];
        special_out[i_row] = o_special_out[i_row];
        common_prod_id_out[i_row] = o_common_prod_id_out[i_row];
        
//    Console.WriteLine("  prod_id= {0} category= {1} title= {2} actor= {3} price= {4} special= {5} common_prod_id= {6}",
//      prod_id_out[i_row], category_out[i_row], title_out[i_row], actor_out[i_row], price_out[i_row], 
//      special_out[i_row], common_prod_id_out[i_row]);
        }           

      switch(browse_type_in)
        {
        case "category":
          Browse_By_Category_prod_id_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Category_category_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Category_title_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Category_actor_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Category_price_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Category_special_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Category_common_prod_id_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Category_membership_item_out.Size = GlobalConstants.MAX_ROWS;
          break;
        case "actor":
          Browse_By_Actor_prod_id_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Actor_category_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Actor_title_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Actor_actor_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Actor_price_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Actor_special_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Actor_common_prod_id_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Actor_membership_item_out.Size = GlobalConstants.MAX_ROWS;
          break;
        case "title":
          Browse_By_Title_prod_id_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Title_category_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Title_title_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Title_actor_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Title_price_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Title_special_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Title_common_prod_id_out.Size=GlobalConstants.MAX_ROWS;
          Browse_By_Title_membership_item_out.Size = GlobalConstants.MAX_ROWS;
          break;
        }

      return(true);
      } // end ds2browse()
    
//
//-------------------------------------------------------------------------------------------------
// 
      
    public bool ds2browsereview(string browse_review_type_in, string get_review_category_in, string get_review_actor_in,
      string get_review_title_in, int batch_size_in, int customerid_out, ref int rows_returned,
      ref int[] prod_id_out, ref string[] title_out, ref string[] actor_out, ref int[] review_id_out,
      ref string[] review_date_out, ref int[] review_stars_out,ref int[] review_customerid_out,
      ref string[]review_summary_out, ref string[]review_text_out, ref int[]review_helpfulness_sum_out, ref double rt)
    {
        // Reviews Table: "REVIEW_ID" NUMBER,  "PROD_ID" NUMBER,  "REVIEW_DATE" DATE, "STARS" NUMBER,
        // "CUSTOMERID" NUMBER,  "REVIEW_SUMMARY" VARCHAR2(50 byte), "REVIEW_TEXT" VARCHAR2(1000 byte) 
        string data_in = null;
        int[] category_out = new int[GlobalConstants.MAX_ROWS];

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif
        switch (browse_review_type_in)
        {
            case "actor":
                Get_Prod_Reviews_By_Actor_prm[0].Value = batch_size_in;
                Get_Prod_Reviews_By_Actor_prm[2].Value = get_review_actor_in;
                data_in = get_review_actor_in;
                break;
            case "title":
                Get_Prod_Reviews_By_Title_prm[0].Value = batch_size_in;
                Get_Prod_Reviews_By_Title_prm[2].Value = get_review_title_in;
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
            switch (browse_review_type_in)
            {
                case "actor":
                    Get_Prod_Reviews_By_Actor.ExecuteNonQuery();
                    rows_returned = Convert.ToInt32(Get_Prod_Reviews_By_Actor_prm[1].Value.ToString());
                    for (i = 0; i < rows_returned; i++)
                    {
                        o_review_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Actor_review_id_out.Value as Array).GetValue(i)).ToString());
                        o_prod_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Actor_prod_id_out.Value as Array).GetValue(i)).ToString());
                        o_review_stars_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Actor_review_stars_out.Value as Array).GetValue(i)).ToString());
                        o_review_customerid_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Actor_review_customerid_out.Value as Array).GetValue(i)).ToString());
                        o_review_helpfulness_sum_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Actor_review_helpfulness_sum_out.Value as Array).GetValue(i)).ToString());
                    }
                    o_title_out = (OracleString[])Get_Prod_Reviews_By_Actor_title_out.Value;
                    o_actor_out = (OracleString[])Get_Prod_Reviews_By_Actor_actor_out.Value;
                    o_review_date_out = (OracleString[])Get_Prod_Reviews_By_Actor_review_date_out.Value;
                    o_review_summary_out = (OracleString[])Get_Prod_Reviews_By_Actor_review_summary_out.Value;
                    o_review_text_out = (OracleString[])Get_Prod_Reviews_By_Actor_review_text_out.Value;
                    break;
                case "title":
                    Get_Prod_Reviews_By_Title.ExecuteNonQuery();
                    rows_returned = Convert.ToInt32(Get_Prod_Reviews_By_Title_prm[1].Value.ToString());
                    for (i = 0; i < rows_returned; i++)
                    {
                        o_review_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Title_review_id_out.Value as Array).GetValue(i)).ToString());
                        o_prod_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Title_prod_id_out.Value as Array).GetValue(i)).ToString());
                        o_review_stars_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Title_review_stars_out.Value as Array).GetValue(i)).ToString());
                        o_review_customerid_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Title_review_customerid_out.Value as Array).GetValue(i)).ToString());
                        o_review_helpfulness_sum_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Title_review_helpfulness_sum_out.Value as Array).GetValue(i)).ToString());
                    }
                    o_title_out = (OracleString[])Get_Prod_Reviews_By_Title_title_out.Value;
                    o_actor_out = (OracleString[])Get_Prod_Reviews_By_Title_actor_out.Value;
                    o_review_date_out = (OracleString[])Get_Prod_Reviews_By_Title_review_date_out.Value;
                    o_review_summary_out = (OracleString[])Get_Prod_Reviews_By_Title_review_summary_out.Value;
                    o_review_text_out = (OracleString[])Get_Prod_Reviews_By_Title_review_text_out.Value;
                    break;
            }
        }
        catch (OracleException e)
        {
            Console.WriteLine("Thread {0}: Oracle Error in Browse Product Reviews: {1}", Thread.CurrentThread.Name, e.Message);
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

        //    Console.WriteLine("Thread {0}: Browse successful: type= {1}  rows_returned={2}",  
        //       Thread.CurrentThread.Name, browse_type_in, rows_returned);
        for (int i_row = 0; i_row < rows_returned; i_row++)
        {
            prod_id_out[i_row] = o_prod_id_out[i_row];
            title_out[i_row] = o_title_out[i_row].ToString();
            actor_out[i_row] = o_actor_out[i_row].ToString();
            review_id_out[i_row] = o_review_id_out[i_row];
            review_date_out[i_row] = o_review_date_out[i_row].ToString();
            review_stars_out[i_row] = o_review_stars_out[i_row];
            review_customerid_out[i_row] = o_review_customerid_out[i_row];
            review_summary_out[i_row] = o_review_summary_out[i_row].ToString();
            review_text_out[i_row] = o_review_text_out[i_row].ToString();
            review_helpfulness_sum_out[i_row] = o_review_helpfulness_sum_out[i_row];

            //    Console.WriteLine("  prod_id= {0} category= {1} title= {2} actor= {3} price= {4} special= {5} common_prod_id= {6}",
            //      prod_id_out[i_row], category_out[i_row], title_out[i_row], actor_out[i_row], price_out[i_row], 
            //      special_out[i_row], common_prod_id_out[i_row]);
        }

        switch (browse_review_type_in)
        {
            case "actor":
                Get_Prod_Reviews_By_Actor_review_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_prod_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_review_stars_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_review_customerid_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_title_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_actor_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_review_date_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_review_summary_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Actor_review_text_out.Size = GlobalConstants.MAX_ROWS;
                                
                break;
            case "title":
                Get_Prod_Reviews_By_Title_review_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_prod_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_review_stars_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_review_customerid_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_title_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_actor_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_review_date_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_review_summary_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Title_review_text_out.Size = GlobalConstants.MAX_ROWS;

                break;
        }


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

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif
        switch (get_review_type_in)
        {
            case "noorder":
                Get_Prod_Reviews_prm[0].Value = batch_size_in;
                Get_Prod_Reviews_prm[2].Value = get_review_prod_in;
                break;
            case "star":
                Get_Prod_Reviews_By_Stars_prm[0].Value = batch_size_in;
                Get_Prod_Reviews_By_Stars_prm[2].Value = get_review_prod_in;
                Get_Prod_Reviews_By_Stars_prm[3].Value = get_review_stars_in;
                break;
            case "date":
                Get_Prod_Reviews_By_Date_prm[0].Value = batch_size_in;
                Get_Prod_Reviews_By_Date_prm[2].Value = get_review_prod_in;
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
                    Get_Prod_Reviews.ExecuteNonQuery();
                    rows_returned = Convert.ToInt32(Get_Prod_Reviews_prm[1].Value.ToString());
                    for (i = 0; i < rows_returned; i++)
                    {
                        o_review_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_review_id_out.Value as Array).GetValue(i)).ToString());
                        o_prod_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_prod_id_out.Value as Array).GetValue(i)).ToString());
                        o_review_stars_out[i] = Convert.ToInt32(((Get_Prod_Reviews_review_stars_out.Value as Array).GetValue(i)).ToString());
                        o_review_customerid_out[i] = Convert.ToInt32(((Get_Prod_Reviews_review_customerid_out.Value as Array).GetValue(i)).ToString());
                        o_review_helpfulness_sum_out[i] = Convert.ToInt32(((Get_Prod_Reviews_review_helpfulness_sum_out.Value as Array).GetValue(i)).ToString());
                    }
                    o_review_date_out = (OracleString[])Get_Prod_Reviews_review_date_out.Value;
                    o_review_summary_out = (OracleString[])Get_Prod_Reviews_review_summary_out.Value;
                    o_review_text_out = (OracleString[])Get_Prod_Reviews_review_text_out.Value;
                    break;
                case "star":
                    Get_Prod_Reviews_By_Stars.ExecuteNonQuery();
                    rows_returned = Convert.ToInt32(Get_Prod_Reviews_By_Stars_prm[1].Value.ToString());
                    for (i = 0; i < rows_returned; i++)
                    {
                        o_review_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Stars_review_id_out.Value as Array).GetValue(i)).ToString());
                        o_prod_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Stars_prod_id_out.Value as Array).GetValue(i)).ToString());
                        o_review_stars_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Stars_review_stars_out.Value as Array).GetValue(i)).ToString());
                        o_review_customerid_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Stars_review_customerid_out.Value as Array).GetValue(i)).ToString());
                        o_review_helpfulness_sum_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Stars_review_helpfulness_sum_out.Value as Array).GetValue(i)).ToString());
                    }
                    o_review_date_out = (OracleString[])Get_Prod_Reviews_By_Stars_review_date_out.Value;
                    o_review_summary_out = (OracleString[])Get_Prod_Reviews_By_Stars_review_summary_out.Value;
                    o_review_text_out = (OracleString[])Get_Prod_Reviews_By_Stars_review_text_out.Value;
                    break;
                case "date":
                    Get_Prod_Reviews_By_Date.ExecuteNonQuery();
                    rows_returned = Convert.ToInt32(Get_Prod_Reviews_By_Date_prm[1].Value.ToString());
                    for (i = 0; i < rows_returned; i++)
                    {
                        o_review_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Date_review_id_out.Value as Array).GetValue(i)).ToString());
                        o_prod_id_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Date_prod_id_out.Value as Array).GetValue(i)).ToString());
                        o_review_stars_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Date_review_stars_out.Value as Array).GetValue(i)).ToString());
                        o_review_customerid_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Date_review_customerid_out.Value as Array).GetValue(i)).ToString());
                        o_review_helpfulness_sum_out[i] = Convert.ToInt32(((Get_Prod_Reviews_By_Date_review_helpfulness_sum_out.Value as Array).GetValue(i)).ToString());
                    }
                    o_review_date_out = (OracleString[])Get_Prod_Reviews_By_Date_review_date_out.Value;
                    o_review_summary_out = (OracleString[])Get_Prod_Reviews_By_Date_review_summary_out.Value;
                    o_review_text_out = (OracleString[])Get_Prod_Reviews_By_Date_review_text_out.Value;
                    break;
            }
        }
        catch (OracleException e)
        {
            Console.WriteLine("Thread {0}: Oracle Error in Get Product Reviews: {1}", Thread.CurrentThread.Name, e.Message);
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

        //    Console.WriteLine("Thread {0}: Browse successful: type= {1}  rows_returned={2}",  
        //       Thread.CurrentThread.Name, browse_type_in, rows_returned);
        for (int i_row = 0; i_row < rows_returned; i_row++)
        {
            prod_id_out[i_row] = o_prod_id_out[i_row];
            review_id_out[i_row] = o_review_id_out[i_row];
            review_date_out[i_row] = o_review_date_out[i_row].ToString();
            review_stars_out[i_row] = o_review_stars_out[i_row];
            review_customerid_out[i_row] = o_review_customerid_out[i_row];
            review_summary_out[i_row] = o_review_summary_out[i_row].ToString();
            review_text_out[i_row] = o_review_text_out[i_row].ToString();
            review_helpfulness_sum_out[i_row] = o_review_helpfulness_sum_out[i_row];
            
            //    Console.WriteLine("  prod_id= {0} category= {1} title= {2} actor= {3} price= {4} special= {5} common_prod_id= {6}",
            //      prod_id_out[i_row], category_out[i_row], title_out[i_row], actor_out[i_row], price_out[i_row], 
            //      special_out[i_row], common_prod_id_out[i_row]);
        }

        switch (get_review_type_in)
        {
            case "noorder":
                Get_Prod_Reviews_review_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_prod_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_review_stars_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_review_customerid_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_review_date_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_review_summary_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_review_text_out.Size = GlobalConstants.MAX_ROWS;

                break;
            case "star":
                Get_Prod_Reviews_By_Stars_review_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Stars_prod_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Stars_review_stars_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Stars_review_customerid_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Stars_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Stars_review_date_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Stars_review_summary_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Stars_review_text_out.Size = GlobalConstants.MAX_ROWS;

                break;
            case "date":
                Get_Prod_Reviews_By_Date_review_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Date_prod_id_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Date_review_stars_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Date_review_customerid_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Date_review_helpfulness_sum_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Date_review_date_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Date_review_summary_out.Size = GlobalConstants.MAX_ROWS;
                Get_Prod_Reviews_By_Date_review_text_out.Size = GlobalConstants.MAX_ROWS;

                break;
        }
                
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
     
      New_Prod_Review_prm[0].Value = new_review_prod_id_in;
      New_Prod_Review_prm[1].Value = new_review_stars_in;
      New_Prod_Review_prm[2].Value = new_review_customerid_in;
      New_Prod_Review_prm[3].Value = new_review_summary_in;
      New_Prod_Review_prm[4].Value = new_review_text_in;
          

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  
      
      try 
        {
        New_Prod_Review.ExecuteNonQuery();
        newreviewid_out = Convert.ToInt32(New_Prod_Review_prm[5].Value.ToString());
        }       
      catch (OracleException e) 
        {
        Console.WriteLine("Thread {0}: Oracle Error in New_Prod_Review.ExecuteNonQuery(): {1}", 
          Thread.CurrentThread.Name, e.Message);
        return(false);
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Thread {0}: System Error in New_Prod_Review.ExecuteNonQuery(): {1}", 
          Thread.CurrentThread.Name, e.Message);
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

        New_Review_Helpfulness_prm[0].Value = reviewid_in;
        New_Review_Helpfulness_prm[1].Value = customerid_in;
        New_Review_Helpfulness_prm[2].Value = reviewhelpfulness_in;

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif

        try
        {
            New_Review_Helpfulness.ExecuteNonQuery();
            reviewhelpfulnessid_out = Convert.ToInt32(New_Review_Helpfulness_prm[3].Value.ToString());
        }
        catch (OracleException e)
        {
            Console.WriteLine("Thread {0}: Oracle Error in New_Review_Helpfulness.ExecuteNonQuery(): {1}",
              Thread.CurrentThread.Name, e.Message);
            return (false);
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Thread {0}: System Error in New_Review_Helpfulness.ExecuteNonQuery(): {1}",
              Thread.CurrentThread.Name, e.Message);
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
    } // end ds2newreviewhelpfulness()


    public bool ds2purchase(int cart_items, int[] prod_id_in, int[] qty_in, int customerid_out,
      ref int neworderid_out, ref bool IsRollback, ref double rt)
      {
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif 

      
      //Cap cart_items at 10 for this implementation of stored procedure
      cart_items = System.Math.Min(10, cart_items);
      
      // Extra, non-stored procedure query to find total cost of purchase
 
      int i, j; 
      decimal netamount_in = 0; 
      
      //Modified by GSK for parameterized query
      //Original Implementation
      //string cost_query = "select PROD_ID, PRICE from PRODUCTS where PROD_ID in (" + prod_id_in[0];
      //for (i=1; i<cart_items; i++) cost_query = cost_query + "," + prod_id_in[i];
      //cost_query = cost_query + ")";
      ////Console.WriteLine(cost_query);
      //OracleCommand cost_command = new OracleCommand(cost_query, objConn);


      //Implementation for parameterizing IN query by GSK
      string cost_query = "select PROD_ID, PRICE from PRODUCTS where PROD_ID in ( :ARG0 ";
      for (i = 1; i < cart_items; i++) cost_query = cost_query + ", :ARG" + i;
      cost_query = cost_query + ")";
      OracleCommand cost_command = new OracleCommand(cost_query, objConn);
      string argHolder;
      for (i = 0; i < cart_items; i++)
      {
          argHolder = ":ARG" + i;
          cost_command.Parameters.Add(argHolder, OracleDbType.Int32);
          cost_command.Parameters[argHolder].Value = prod_id_in[i];
      }
    
      OracleDataReader Rdr = cost_command.ExecuteReader();
      while (Rdr.Read())
        {
        j = 0;
        int prod_id = Convert.ToInt32(Rdr.GetDecimal(0));
        while (prod_id_in[j] != prod_id) ++j; // Find which product was returned
        netamount_in = netamount_in + qty_in[j] * Rdr.GetDecimal(1);
        //Console.WriteLine(j + " " + prod_id + " " + qty_in[j] + " " + Rdr.GetDecimal(1));
        }
      Rdr.Close();

      // Can use following code instead if you don't want extra roundtrip to database:
      //Random rr = new Random(DateTime.Now.Millisecond);
      //decimal netamount_in = (decimal) (0.01 * (1 + rr.Next(40000)));
      //Console.WriteLine(netamount_in);
      decimal taxamount_in =  (decimal) 0.0825 * netamount_in;
      decimal totalamount_in = netamount_in + taxamount_in;
      
      Purchase_prm[0].Value = customerid_out;
      Purchase_prm[1].Value = cart_items;
      Purchase_prm[2].Value = netamount_in;
      Purchase_prm[3].Value = taxamount_in;
      Purchase_prm[4].Value = totalamount_in;
      
      Purchase_prod_id_in.Value = prod_id_in; 
      Purchase_qty_in.Value = qty_in;     

//    Console.WriteLine("Thread {0}: Calling Purchase w/ customerid = {1}  number_items= {2}",  
//      Thread.CurrentThread.Name, customerid_out, cart_items);

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)  
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  
        
      try 
        {
        Purchase.ExecuteNonQuery();
        neworderid_out = Convert.ToInt32(Purchase_prm[5].Value.ToString());
        }

      catch(OracleException e)
        {
        Console.WriteLine("Thread {0}: Oracle Error in Purchase.ExecuteNonQuery(): {1}",  
          Thread.CurrentThread.Name, e.Message);
        return(false);
        }
      catch(System.Exception e)
        {
        Console.WriteLine("Thread {0}: System Error in Purchase.ExecuteNonQuery(): {1}",  
          Thread.CurrentThread.Name, e.Message);
        return(false);
        }

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif  
      if (neworderid_out == 0) IsRollback = true;    

//    Console.WriteLine("Thread {0}: Purchase successful: customerid = {1}  number_items= {2}  IsRollback= {3}",  
//      Thread.CurrentThread.Name, customerid_out, cart_items, IsRollback);

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
  
        
