
/*
 * DVD Store 3 Web Functions - ds3webfns.cs
 *
 * Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
 *
 * Provides interface functions for DVD Store driver program ds2xdriver.cs
 * See ds2xdriver.cs for compilation and syntax
 *
 * Last Updated 6/24/05
 * Last Updated 6/14/2010 by GSK <girish.khadke@gmail.com> and <tmuirhead@vmware.com>
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
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;


namespace ds2xdriver
  {
  /// <summary>
  /// ds3webfns.cs: DVD Store 3 Web Functions
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
    Char[] read = new Char[256];
    string URL, str_acc;
    System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
    HttpWebRequest httpWebRequest;
    HttpWebResponse httpWebResponse;
    Stream receiveStream;
    StreamReader readStream;

    //Added by GSK to get target server name
    string target_server_name;

    public ds2Interface(int ds2interfaceid)
      {
      ds2Interfaceid = ds2interfaceid;
      //Console.WriteLine("ds2Interface {0} created", ds2Interfaceid);
      }
//
//-------------------------------------------------------------------------------------------------
    //Added overloaded constructor to handle scenario in which single instance of web driver is driving workload on multiple target machines
    public ds2Interface ( int ds2interfaceid , string target_name)
        {
        ds2Interfaceid = ds2interfaceid;
        target_server_name = target_name;
        //Console.WriteLine("ds2Interface {0} created for server {1}", ds2Interfaceid, target_server_name);
        }
//
//-------------------------------------------------------------------------------------------------
//
    public bool ds2initialize()
      {
      return(true);
      } // end ds2initialize()
//
//-------------------------------------------------------------------------------------------------
// 
    public bool ds2connect()
      {
      URL =
        "http://" + target_server_name + "/" + Controller.virt_dir + "/index.html";          //changed Controller.target to target_server_name

      // Create a 'HttpWebRequest' object with the specified url. 
      httpWebRequest = (HttpWebRequest) WebRequest.Create(URL);
//    httpWebRequest.KeepAlive = false; 
      //    Console.WriteLine("Thread {0}: Connecting to {1} with URL {2}", Thread.CurrentThread.Name, target_server_name,     //changed Controller.target to target_server_name
//      URL);
      ServicePoint sp = httpWebRequest.ServicePoint;
      sp.ConnectionLimit = 200;     // Limits max threads or users to 100. Will get timeout errors above 100 threads
//    Console.WriteLine("Thread {0}: ServicePoint: name= {1}  max conns= {2}  max idle= {3}", 
//      Thread.CurrentThread.Name, sp.ConnectionName, sp.ConnectionLimit, sp.MaxIdleTime);
      try
        {
        httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Error returned from web server: {0}", e.Message);
        return(false);
        }
  
      // Obtain a 'Stream' object associated with the response object.
      receiveStream = httpWebResponse.GetResponseStream();                
      // Pipe the stream to a higher level stream reader with the required encoding format. 
      readStream = new StreamReader( receiveStream, encode );
      
      if (200 == (int) httpWebResponse.StatusCode) return(true);
      else return(false);
      } // end ds2connect()
 
//
//-------------------------------------------------------------------------------------------------
//
    public bool ds2login(string username_in, string password_in, ref int customerid_out, ref int rows_returned, 
      ref string[] title_out, ref string[] actor_out, ref string[] related_title_out, ref double rt)
      {
      int count, ind_a, ind_b, ind_e, i_row;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif     
      //changed Controller.target to target_server_name
      URL = 
        "http://" + target_server_name + "/" + Controller.virt_dir + "/dslogin." + Controller.page_type +        
        "?username=" + username_in +"&password=" + password_in; 

      // Create a 'HttpWebRequest' object with the specified url. 
      httpWebRequest = (HttpWebRequest) WebRequest.Create(URL);
//    Console.WriteLine("Thread {0}: Calling dslogin." + Controller.page_type + " w/ URL=\n  {1}", 
//      Thread.CurrentThread.Name, URL); 
          
#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif
      // httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
      // Added exception handling to flag specific cause of errors and terminate dead threads
      try
		{
		httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
		}
	  catch (System.Exception e) 
		{
		Console.WriteLine("Error during ds2login returned from web server: {0}", e.Message);
		return(false);
		}

      receiveStream = httpWebResponse.GetResponseStream();
      readStream = new StreamReader( receiveStream, encode );

      str_acc = "";
      do  
        {
        count = readStream.Read(read, 0, 256);
        String str = new String(read, 0, count);
        str_acc = str_acc + str;
        } while (count > 0);
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif            
  
//    Console.WriteLine("Thread {0}:  ds2login str_acc length: {1}  str_acc: \n{2}\n", 
//      Thread.CurrentThread.Name, str_acc.Length, str_acc);                  

//    String to parse: ...<INPUT TYPE=HIDDEN NAME=customerid VALUE=12091>...
//                                                ^                ^    ^
//                                                |                |    |
//                              IndexOf(customerid)            ind_b    ind_e
//    or:              ...<INPUT TYPE="HIDDEN" NAME="customerid" VALUE="12091">...
//                                                   ^                 ^      ^
//                                                   |                 |      |
//                                 IndexOf(customerid)             ind_b      ind_e
          
      if(str_acc.IndexOf("customerid")<0)
        {
        customerid_out = 0;
        Console.WriteLine("Thread {0}: Login: Username/password incorrect; str_acc length: {1}  str_acc: \n{2}\n",
          Thread.CurrentThread.Name, str_acc.Length, str_acc); 
        return(false);
        }         
 
      ind_a = str_acc.IndexOf("customerid");
      str_acc = str_acc.Substring(ind_a);
      ind_b = str_acc.IndexOf("=")  + 1;
      ind_e = str_acc.Substring(ind_b).IndexOf(">");
//    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc[]= {2}", 
//      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
      customerid_out = 0;
      try
        {
        customerid_out = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
        }
      catch (System.Exception e)
        {
        Console.WriteLine("Thread {0}: Login Error: {1}", Thread.CurrentThread.Name, e.Message);
        return(false);
        }

//    Parse Previous Order if there is one

//    Results HTML looks like:
//    <H3>Your previous purchases:</H3>
//    <TABLE border=2>
//    <TR>
//    <TH>Title</TH>
//    <TH>Actor</TH>
//    <TH>People who liked this DVD also liked</TH>
//    </TR>
//     <TR>
//    <TD>AGENT DEEP</TD><TD>DORIS GOLDBERG</TD><TD>AIRPORT BABY</TD></TR>
//     <TR>
//    <TD>ALADDIN TIMBERLAND</TD><TD>LIV NORTON</TD><TD>ADAPTATION ACADEMY</TD></TR>
//     <TR>
//    <TD>ADAPTATION WINDOW</TD><TD>WILLEM KILMER</TD><TD>AIRPORT FAMILY</TD></TR>
//     <TR>
//    <TD>AFFAIR MUPPET</TD><TD>WINONA PECK</TD><TD>AIRPORT HANDICAP</TD></TR>
//    </TABLE>

      rows_returned = 0;
      if (str_acc.IndexOf("Your previous purchases:") > 0)
        {
        i_row = 0;
        str_acc = str_acc.Substring(str_acc.IndexOf("<TABLE")); // Snip off everything up to <TABLE> tag
        str_acc = str_acc.Substring(4 + str_acc.IndexOf("<TR>")); // Skip first <TR> tag
        while (str_acc.IndexOf("<TR>") > 0)
          {
          str_acc = str_acc.Substring(str_acc.IndexOf("<TR>")); // Find <TR> tag
          str_acc = str_acc.Substring(4 + str_acc.IndexOf("<TD>")); // Find first <TD> tag
          ind_e = str_acc.IndexOf("<");
          title_out[i_row] = str_acc.Substring(0, ind_e);
          str_acc = str_acc.Substring(4 + str_acc.IndexOf("<TD>")); // Find 2nd <TD> tag
          ind_e = str_acc.IndexOf("<");
          actor_out[i_row] = str_acc.Substring(0, ind_e);
          str_acc = str_acc.Substring(4 + str_acc.IndexOf("<TD>")); // Find 3rd <TD> tag
          ind_e = str_acc.IndexOf("<");
          related_title_out[i_row] = str_acc.Substring(0, ind_e);
           ++i_row;
          }
        rows_returned = i_row;
        }

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
      int count, ind_e, temp_ind;
      string temp_string;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif   
      //changed Controller.target to target_server_name
      URL = 
        "http://" + target_server_name + "/" + Controller.virt_dir + "/dsnewcustomer." + Controller.page_type +
        "?firstname=" + firstname_in + 
        "&lastname=" + lastname_in +
        "&address1=" + address1_in +
        "&address2=" + address2_in +
        "&city=" + city_in +
        "&state=" + state_in +
        "&zip=" + zip_in +
        "&country=" + country_in +
        "&email=" + email_in +
        "&phone=" + phone_in +
        "&creditcardtype=" + creditcardtype_in +
        "&creditcard=" + creditcard_in +
        "&ccexpmon=" + ccexpmon_in +
        "&ccexpyr=" + ccexpyr_in +
        "&username=" + username_in +
        "&password=" + password_in +
        "&age=" + age_in +
        "&income=" + income_in +
        "&gender=" + gender_in;
             
      httpWebRequest = (HttpWebRequest) WebRequest.Create(URL); 
//    Console.WriteLine("Thread {0}: Calling dsnewcustomer." + Controller.page_type + " w/ URL=\n  {1}",
//      Thread.CurrentThread.Name, URL); 

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  

      // httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
      // Added exception handling to flag specific cause of errors and terminate dead threads
      try
        {
        httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Error during ds2newcustomer returned from web server: {0}", e.Message);
        return(false);
        }

      receiveStream = httpWebResponse.GetResponseStream();
      readStream = new StreamReader( receiveStream, encode );
      str_acc = "";
      do  
        {
        count = readStream.Read(read, 0, 256);
        String str = new String(read, 0, count);
        str_acc = str_acc + str;
        } while (count > 0);
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif        

//    Console.WriteLine("Thread {0}:  ds2neworder str_acc length: {1}  str_acc: \n{2}\n", 
//      Thread.CurrentThread.Name, str_acc.Length, str_acc);        
                        
      customerid_out = 0;
      if (str_acc.IndexOf("Username already in use") > 0) return(true); // customerid_out==0 => name in use
              
//    String to parse: ...<INPUT TYPE=HIDDEN NAME=customerid VALUE=12091>...
//                                                ^                ^    ^
//                                                |                |    |
//                              IndexOf(customerid)            plus 17  ind_e
//    or:              ...<INPUT TYPE="HIDDEN" NAME="customerid" VALUE="12091">...
//                                                   ^                 ^      ^
//                                                   |                 |      |
//                                 IndexOf(customerid)             plus 17      ind_e
// 

      temp_ind = str_acc.IndexOf("customerid");
      temp_string = str_acc;
      str_acc = str_acc.Substring(17 + str_acc.IndexOf("customerid"));
      ind_e = str_acc.IndexOf(">");
      if (ind_e < 1) 
      { 
          customerid_out = -1;
          return (true); 
      }
//    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
//      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
      try
        {
        customerid_out = Convert.ToInt32(str_acc.Substring(0, ind_e).Trim('"'));
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Error in parsing customerid: {0}; ind_e= {1}  str_acc[]= {2}", 
          e.Message, ind_e, str_acc.Substring(0, ind_e));
        return(false);
        }
      return(true);
      } // end ds2newcustomer()

    //
    //-------------------------------------------------------------------------------------------------
    // 
    
    public bool ds2newmember(int customerid_in, int membershiplevel_in, ref int customerid_out, ref double rt)
    {
    int count, ind_a, ind_b, ind_e;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif

        URL =
        "http://" + target_server_name + "/" + Controller.virt_dir + "/dsnewmember." + Controller.page_type +
        "?customerid=" + customerid_in +
        "&membershiplevel=" + membershiplevel_in;

        httpWebRequest = (HttpWebRequest)WebRequest.Create(URL); 
        
#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif
        try
        {
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error during ds2newmember returned from web server: {0}", e.Message);
            return (false);
        }

        receiveStream = httpWebResponse.GetResponseStream();
        readStream = new StreamReader(receiveStream, encode);
        str_acc = "";
        do
        {
            count = readStream.Read(read, 0, 256);
            String str = new String(read, 0, count);
            str_acc = str_acc + str;
        } while (count > 0);

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif      

        customerid_out = 0;
        if (str_acc.IndexOf("Username already in use") > 0) return (true); // customerid_out==0 => name in use

        //    String to parse: ...<INPUT TYPE=HIDDEN NAME=customerid VALUE=12091>...
        //                                                ^                ^    ^
        //                                                |                |    |
        //                              IndexOf(customerid)            ind_b    ind_e
        //    or:              ...<INPUT TYPE="HIDDEN" NAME="customerid" VALUE="12091">...
        //                                                   ^                 ^      ^
        //                                                   |                 |      |
        //                                 IndexOf(customerid)             ind_b      ind_e
        // 
        ind_a = str_acc.IndexOf("customerid");
        str_acc = str_acc.Substring(ind_a);
        ind_b = str_acc.IndexOf("=") + 1;
        ind_e = str_acc.Substring(ind_b).IndexOf(">");
        //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
        //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
        try
        {
            customerid_out = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error in parsing customerid: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
              e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
            return (false);
        }
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
      
      int count, ind_e, i_row;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif  
      //changed Controller.target to target_server_name
      URL = 
        "http://" + target_server_name + "/" + Controller.virt_dir + "/dsbrowse." + Controller.page_type + 
        "?browsetype=" + browse_type_in +
        "&browse_category=" + browse_category_in +
        "&browse_actor=" + browse_actor_in +
        "&browse_title=" + browse_title_in +
        "&limit_num=" + batch_size_in +
        "&customerid=" + customerid_out;
              
        httpWebRequest = (HttpWebRequest) WebRequest.Create(URL); 
//      Console.WriteLine("Thread {0}: Calling dsbrowse." + Controller.page_type + " w/ URL=\n  {1}", 
//        Thread.CurrentThread.Name, URL); 

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif 

      // httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
      // Added exception handling to flag specific cause of errors and terminate dead threads
      try
        {
        httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Error during ds2browse returned from web server: {0}", e.Message);
        return(false);
        }
      receiveStream = httpWebResponse.GetResponseStream();
      readStream = new StreamReader( receiveStream, encode );
      str_acc = "";
      do  
        {
        count = readStream.Read(read, 0, 256);
        String str = new String(read, 0, count);
        str_acc = str_acc + str;
        } while (count > 0);
            
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif  

//    Console.WriteLine("Thread {0}:  ds2browse str_acc length: {1}  str_acc: \n{2}\n", 
//      Thread.CurrentThread.Name, str_acc.Length, str_acc);    

      if(str_acc.IndexOf("No DVDs Found") > 0)
        {
        rows_returned = 0;
        return(true);
        }
        
      if(str_acc.IndexOf("<TABLE") < 0)
        {
        Console.WriteLine("Thread {0}:  error in parsing response from browse products request",
          Thread.CurrentThread.Name);      
        return(false);
        }
                        
      else  // DVDs were found
        {
//      Results HTML looks like:
//      <H2>Search Results</H2>
//      <FORM ACTION='./dsbrowse.php' METHOD='GET'>
//      <TABLE border=2>
//      <TR>
//      <TH>Add to Shopping Cart</TH>
//      <TH>Title</TH>
//      <TH>Actor</TH>
//      <TH>Price</TH>
//      <TH>Add to Shopping Cart</TH>
//      <TH>Title</TH>
//      <TH>Actor</TH>
//      <TH>Price</TH>
//      </TR>
//      <TR>
//      <TD><INPUT NAME=selected_item[] TYPE=CHECKBOX VALUE=12976></TD>
//      <TD><a href='dsgetreviews.php?customerid=3&review_title=ALI WIND&productid=12976' target='_blank'>ALI WIND</a></TD>
//      <TD>RICHARD HAWN</TD>
//      <TD>23.99</TD>
//      </TR>
//      ...
//      </TABLE>

        rows_returned = 0;
        i_row = 0;
        str_acc = str_acc.Substring(str_acc.IndexOf("<TABLE")); // Snip off everything up to <TABLE> tag
        str_acc = str_acc.Substring(4 + str_acc.IndexOf("<TR>")); // Skip first <TR> tag
        while (str_acc.IndexOf("<TR>") > 0)
          {
          str_acc = str_acc.Substring(str_acc.IndexOf("<TR>")); // Find <TR> tag
          str_acc = str_acc.Substring(6 + str_acc.IndexOf("VALUE"));
          ind_e = str_acc.IndexOf(">");
          prod_id_out[i_row] = Convert.ToInt32(str_acc.Substring(0, ind_e).Trim('"'));
          str_acc = str_acc.Substring(4 + str_acc.IndexOf("<TD>")); // Find <TD> tag
          str_acc = str_acc.Substring(1 + str_acc.IndexOf(">"));   //Find the end of the href tag
          ind_e = str_acc.IndexOf("<");
          title_out[i_row] = str_acc.Substring(0, ind_e);
          str_acc = str_acc.Substring(4 + str_acc.IndexOf("<TD>")); // Find <TD> tag
          ind_e = str_acc.IndexOf("<");
          actor_out[i_row] = str_acc.Substring(0, ind_e);
          str_acc = str_acc.Substring(4 + str_acc.IndexOf("<TD>")); // Find <TD> tag
          ind_e = str_acc.IndexOf("<");
          price_out[i_row] = Convert.ToDecimal(str_acc.Substring(0, ind_e));
          ++i_row;
          ++rows_returned;
          }
        } //End DVDs were found
      return(true);
      } // end ds2browse()

      //
      //--------------------------------------------------------------------------------------
      //
      
    public bool ds2browsereview(string browse_review_type_in, string get_review_category_in, string get_review_actor_in,
    string get_review_title_in, int batch_size_in, int customerid_out, ref int rows_returned,
    ref int[] prod_id_out, ref string[] title_out, ref string[] actor_out, ref int[] review_id_out,
    ref string[] review_date_out, ref int[] review_stars_out, ref int[] review_customerid_out,
    ref string[] review_summary_out, ref string[] review_text_out, ref int[] review_helpfulness_sum_out, ref double rt)
    {
        int count, ind_a, ind_b, ind_e, i_row;

#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif
        URL =
                "http://" + target_server_name + "/" + Controller.virt_dir + "/dsbrowsereviews." + Controller.page_type +
                "?browsereviewtype=" + browse_review_type_in +
                "&review_actor=" + get_review_actor_in +
                "&review_title=" + get_review_title_in +
                "&limit_num=" + batch_size_in +
                "&customerid=" + customerid_out;
        httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif
        
        try
        {
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();  
            
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error during browse for reviews returned by web server: {0}", e.Message);
            return (false);
        }
        receiveStream = httpWebResponse.GetResponseStream();
        readStream = new StreamReader(receiveStream, encode);
        str_acc = "";
        do
        {
            count = readStream.Read(read, 0, 256);
            String str = new String(read, 0, count);
            str_acc = str_acc + str;
        } while (count > 0);

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif
        if (str_acc.IndexOf("No Reviews Found") > 0)
        {
            rows_returned = 0;
            return (true);
        }

        if (str_acc.IndexOf("------<BR>") < 0)
        {
            Console.WriteLine("Thread {0}:  error in parsing response from browse reviews request",
              Thread.CurrentThread.Name);
            return (false);
        }

        else  // Reviews were found
        {
            // Results output looks like this:
            //      <H2> Most Helpful Reviews matching keyword </H2>
                //------------------------<BR> CHITTY WIND starring BURT TOMEI<BR>
                // Review Summary - well fact demands<BR>
                // Rated 1 stars<BR>
                // Review Created By 13585179 on 2013-03-08<BR>
                // You 2nd actual needed hed home dialogue reputation seen normally Alfred fascinating shines amount believable Blade it When cool harder loyal our Lucas begins worse raise tears Also backdrop concerned corporate desert Robert least boxing future thats amazing pointed wise helpful values list Planet believed possible Mel terror Four made identity steps a party creation war Ill alot politics enjoy bit recognize remaining demonstrates Beautiful genre headed films The woman Way settings unless exist via see 7 <BR>
                //<FORM ACTION='./dsnewhelpfulness.php' METHOD='GET'>
                //Helpfulness ranking of this review (10 is most helpful) 
                //<SELECT NAME='review_helpfulness'>
                // <OPTION VALUE=1>1</OPTION>
                //<OPTION VALUE=2>2</OPTION>
                // ........
                //<OPTION VALUE=10>10</OPTION>
                //</SELECT><BR>
                //<INPUT TYPE=HIDDEN NAME=customerid VALUE='2'>
                //<INPUT TYPE=HIDDEN NAME=reviewid VALUE='2264089'>
                //<INPUT TYPE=HIDDEN NAME=productid VALUE='145976'>
                //<INPUT TYPE=HIDDEN NAME=helpfulness_sum VALUE='260'>
                //<INPUT TYPE='submit' VALUE='Submit Helpfulness Rating'>
                //</FORM>

            rows_returned = 0;
            i_row = 0;
            str_acc = str_acc.Substring(str_acc.IndexOf("Most Helpful")); // Snip off everything up to Most Helpful text
            while (str_acc.IndexOf("Review Summary") > 0)
            {
                str_acc = str_acc.Substring(4 + str_acc.IndexOf("<BR>"));
                ind_e = str_acc.IndexOf("starring");
                title_out[i_row] = str_acc.Substring(0, ind_e);
                str_acc = str_acc.Substring(9 + str_acc.IndexOf("starring"));
                ind_e = str_acc.IndexOf("<");
                actor_out[i_row] = str_acc.Substring(0, ind_e);
                str_acc = str_acc.Substring(17 + str_acc.IndexOf("Review Summary")); // Review Summary
                ind_e = str_acc.IndexOf("<");
                review_summary_out[i_row] = str_acc.Substring(0, ind_e);

                str_acc = str_acc.Substring(12 + str_acc.IndexOf("<BR>")); // Find <BR> tag
                ind_e = str_acc.IndexOf("stars");
                review_stars_out[i_row] = Convert.ToInt32(str_acc.Substring(0, ind_e));

                str_acc = str_acc.Substring(3 + str_acc.IndexOf("By")); // Find By to located reviewer customerid 
                ind_e = str_acc.IndexOf("on");
                review_customerid_out[i_row] = Convert.ToInt32(str_acc.Substring(0, ind_e));

                str_acc = str_acc.Substring(2 + str_acc.IndexOf("on")); // Find on to the locate the date of review
                ind_e = str_acc.IndexOf("<");
                review_date_out[i_row] = (str_acc.Substring(0, ind_e));

                str_acc = str_acc.Substring(4 + str_acc.IndexOf("<BR>")); // Find <BR> to the locate review text
                ind_e = str_acc.IndexOf("<");
                review_text_out[i_row] = (str_acc.Substring(0, ind_e));
                                
                str_acc = str_acc.Substring(15 + str_acc.IndexOf("reviewid"));
                ind_e = str_acc.IndexOf(">");
                   // Console.WriteLine("  ind_e= {0}  str_acc.Substring(0, ind_e)= {1}", 
                   //   ind_e, str_acc.Substring(0, ind_e));
                try
                {
                    review_id_out[i_row] = Convert.ToInt32(str_acc.Substring(0, ind_e).Trim('"'));
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Error in parsing review_id_out: {0}; ind_e= {1}  str_acc[]= {2}",
                      e.Message, ind_e, str_acc.Substring(0, ind_e));
                    return (false);
                }

                ind_a = str_acc.IndexOf("productid");
                str_acc = str_acc.Substring(ind_a);
                ind_b = str_acc.IndexOf("=") + 1;
                ind_e = str_acc.Substring(ind_b).IndexOf(">");
                //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
                //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                try
                {
                    prod_id_out[i_row] = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Error in parsing prod_id_out: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
                      e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                    return (false);
                }

                ind_a = str_acc.IndexOf("helpfulness_sum");
                str_acc = str_acc.Substring(ind_a);
                ind_b = str_acc.IndexOf("=") + 1;
                ind_e = str_acc.Substring(ind_b).IndexOf(">");
                //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
                //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                try
                {
                    review_helpfulness_sum_out[i_row] = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Error in parsing review_helpfulness_sum_out: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
                      e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                    return (false);
                }

                ++i_row;
                ++rows_returned;
            }
        } //End DVDs were found

        return (true);
    } // end ds2browsereview()

    //
    //-------------------------------------------------------------------------------------------------
    // 

    public bool ds2getreview(string get_review_type_in, int get_review_prod_in, int get_review_stars_in, int customerid_out, int batch_size_in, ref int rows_returned,
      ref int[] prod_id_out, ref int[] review_id_out, ref string[] review_date_out, ref int[] review_stars_out, ref int[] review_customerid_out,
      ref string[] review_summary_out, ref string[] review_text_out, ref int[] review_helpfulness_sum_out, ref double rt)
    {
        int count, ind_a, ind_b, ind_e, i_row;
        
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif
        URL =
                "http://" + target_server_name + "/" + Controller.virt_dir + "/dsgetreviews." + Controller.page_type +
                "?getreviewtype=" + get_review_type_in +
                "&productid=" + get_review_prod_in +
                "&review_stars=" + get_review_stars_in +
                "&limit_num=" + batch_size_in +
                "&customerid=" + customerid_out;
        httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
        

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif
        try
        {
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error during get reviews returned by web server: {0}", e.Message);
            return (false);
        }
        receiveStream = httpWebResponse.GetResponseStream();
        readStream = new StreamReader(receiveStream, encode);
        str_acc = "";
        do
        {
            count = readStream.Read(read, 0, 256);
            String str = new String(read, 0, count);
            str_acc = str_acc + str;
        } while (count > 0);  

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif
        if (str_acc.IndexOf("No Reviews Found") > 0)
        {
            rows_returned = 0;
            return (true);
        }

        if (str_acc.IndexOf("-------<BR>") < 0)
        {
            Console.WriteLine("Thread {0}:  error in parsing response from get reviews request",
              Thread.CurrentThread.Name);
            return (false);
        }

        else  // Reviews were found
        {
            // Results output looks like this:
            //<H2> Most Helpful Reviews for  </H2>
            //-----------------------------------<BR> Review Summary - Theres Overall wonderfully<BR>
            //Rated 4 stars<BR>
            // Review Created By 38319823 on 2013-12-11<BR>
            // boss own us flat collection stop anything ideas J disappointed random god seemingly perform surprises tales writers this timebr Southern PG13 ridiculous mother glorious dozen cool lines looking science NEVER hitting IF told actor workout remarkably laugh Travolta tired sinister him are caused kinda nasty street delivers Burt Roger Not Meryl Billy become wasnt bonus storyline false girl musicals funny continued far REAL civil poorly review situation called your edge Legend wrong bunch prior amazed name Great region evil showed extras library serve Matrix predictable market blue heck br originally bet mainstream Christmas offered vampires dad church wife Nicholson Colin East faith ignore Who but beat friend messages masterpiece cell figure cruel soundtrack matter definately boring us im gotten <BR>
            // <FORM ACTION='./dsnewhelpfulness.php' METHOD='GET'>
            //  Helpfulness ranking of this review (10 is most helpful) 
            // <SELECT NAME='review_helpfulness'>
            //   <OPTION VALUE=1>1</OPTION>
            //   <OPTION VALUE=2>2</OPTION>
            //   .....
            //   <OPTION VALUE=10>10</OPTION>
            // </SELECT><BR>
            // <INPUT TYPE=HIDDEN NAME=customerid VALUE=2>
            // <INPUT TYPE=HIDDEN NAME=reviewid VALUE=4389533>
            // <INPUT TYPE=HIDDEN NAME=productid VALUE=28040>
            // <INPUT TYPE=HIDDEN NAME=helpfulness_sum VALUE=117>
            // <INPUT TYPE='submit' VALUE='Submit Helpfulness Rating'>
            // </FORM>

            rows_returned = 0;
            i_row = 0;
            str_acc = str_acc.Substring(str_acc.IndexOf("Most Helpful")); // Snip off everything up to Most Helpful text
            while (str_acc.IndexOf("Review Summary") > 0)
            {
                str_acc = str_acc.Substring(17 + str_acc.IndexOf("Review Summary")); // Review Summary
                ind_e = str_acc.IndexOf("<");
                review_summary_out[i_row] = str_acc.Substring(0, ind_e);

                str_acc = str_acc.Substring(12 + str_acc.IndexOf("<BR>")); // Find <BR> tag
                ind_e = str_acc.IndexOf("stars");
                review_stars_out[i_row] = Convert.ToInt32(str_acc.Substring(0, ind_e));

                str_acc = str_acc.Substring(3 + str_acc.IndexOf("By")); // Find By to located reviewer customerid 
                ind_e = str_acc.IndexOf("on");
                review_customerid_out[i_row] = Convert.ToInt32(str_acc.Substring(0, ind_e));

                str_acc = str_acc.Substring(2 + str_acc.IndexOf("on")); // Find on to the locate the date of review
                ind_e = str_acc.IndexOf("<");
                review_date_out[i_row] = (str_acc.Substring(0, ind_e));

                str_acc = str_acc.Substring(4 + str_acc.IndexOf("<BR>")); // Find <BR> to the locate review text
                ind_e = str_acc.IndexOf("<");
                review_text_out[i_row] = (str_acc.Substring(0, ind_e));

                ind_a = str_acc.IndexOf("customerid");
                str_acc = str_acc.Substring(ind_a);
                ind_b = str_acc.IndexOf("=") + 1;
                ind_e = str_acc.Substring(ind_b).IndexOf(">");
                //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
                //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                try
                {
                    review_customerid_out[i_row] = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Error in parsing review_id_out: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
                      e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                    return (false);
                }

                ind_a = str_acc.IndexOf("reviewid");
                str_acc = str_acc.Substring(ind_a);
                ind_b = str_acc.IndexOf("=") + 1;
                ind_e = str_acc.Substring(ind_b).IndexOf(">");
                //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
                //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                try
                {
                    review_id_out[i_row] = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Error in parsing review_id_out: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
                      e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                    return (false);
                }

                ind_a = str_acc.IndexOf("productid");
                str_acc = str_acc.Substring(ind_a);
                ind_b = str_acc.IndexOf("=") + 1;
                ind_e = str_acc.Substring(ind_b).IndexOf(">");
                //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
                //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                try
                {
                    prod_id_out[i_row] = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Error in parsing prod_id_out: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
                      e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                    return (false);
                }

                ind_a = str_acc.IndexOf("helpfulness_sum");
                str_acc = str_acc.Substring(ind_a);
                ind_b = str_acc.IndexOf("=") + 1;
                ind_e = str_acc.Substring(ind_b).IndexOf(">");
                //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
                //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                try
                {
                    review_helpfulness_sum_out[i_row] = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Error in parsing review_helpfulness_sum_out: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
                      e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
                    return (false);
                }

                ++i_row;
                ++rows_returned;
            }
        } //End DVDs were found


        return (true);
    } // end ds2getreview()

    //
    //-------------------------------------------------------------------------------------------------
    // 
    public bool ds2newreview(int new_review_prod_id_in, int new_review_stars_in, int new_review_customerid_in,
            string new_review_summary_in, string new_review_text_in, ref int newreviewid_out, ref double rt)
    {
        int count, ind_a, ind_b, ind_e;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif
        URL =
        "http://" + target_server_name + "/" + Controller.virt_dir + "/dsnewreview." + Controller.page_type +
        "?customerid=" + new_review_customerid_in +
        "&productid=" + new_review_prod_id_in +
        "&review_summary=" + new_review_summary_in +
        "&review_text=" + new_review_text_in +
        "&review_stars=" + new_review_stars_in;

        httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);


#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif

        try
        {
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error during dsnewreview returned from web server: {0}", e.Message);
            return (false);
        }

        receiveStream = httpWebResponse.GetResponseStream();
        readStream = new StreamReader(receiveStream, encode);
        str_acc = "";
        do
        {
            count = readStream.Read(read, 0, 256);
            String str = new String(read, 0, count);
            str_acc = str_acc + str;
        } while (count > 0);

#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif

        newreviewid_out = 0;
        //    String to parse: ...<INPUT TYPE=HIDDEN NAME=reviewid VALUE=12091>...
        //                                                ^              ^    ^
        //                                                |              |    |
        //                              IndexOf(reviewid)            ind_b    ind_e
        //    or:              ...<INPUT TYPE="HIDDEN" NAME="reviewid" VALUE="12091">...
        //                                                   ^               ^      ^
        //                                                   |               |      |
        //                                 IndexOf(reviewid)             ind_b      ind_e
        // 
        ind_a = str_acc.IndexOf("reviewid");
        str_acc = str_acc.Substring(ind_a);
        ind_b = str_acc.IndexOf("=") + 1;
        ind_e = str_acc.Substring(ind_b).IndexOf(">");
        //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
        //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
        try
        {
            newreviewid_out = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error in parsing reviewid: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
              e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
            return (false);
        }
        return (true);
    } // end ds2newreview()



    //
    //-------------------------------------------------------------------------------------------------
    // 
    public bool ds2newreviewhelpfulness(int reviewid_in, int customerid_in, int reviewhelpfulness_in, ref int reviewhelpfulnessid_out, ref double rt)
    {
        int count, ind_a, ind_b, ind_e;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
        TimeSpan TS = new TimeSpan();
        DateTime DT0;
#endif

        URL =
        "http://" + target_server_name + "/" + Controller.virt_dir + "/dsnewhelpfulness." + Controller.page_type +
        "?customerid=" + customerid_in +
        "&review_helpfulness=" + reviewhelpfulness_in +
        "&reviewid=" + reviewid_in;

        httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
        
#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
        DT0 = DateTime.Now;
#endif

        try
        {
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error during ds2newhelpfulness returned from web server: {0}", e.Message);
            return (false);
        }

        receiveStream = httpWebResponse.GetResponseStream();
        readStream = new StreamReader(receiveStream, encode);
        str_acc = "";
        do
        {
            count = readStream.Read(read, 0, 256);
            String str = new String(read, 0, count);
            str_acc = str_acc + str;
        } while (count > 0);
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
        TS = DateTime.Now - DT0;
        rt = TS.TotalSeconds; // Calculate response time
#endif


        reviewhelpfulnessid_out = 0;
        
        //    String to parse: ...<INPUT TYPE=HIDDEN NAME=helpfulnessid VALUE=12091>...
        //                                                ^                   ^    ^
        //                                                |                   |    |
        //                              IndexOf(helpfulnessid)            ind_b    ind_e
        //    or:              ...<INPUT TYPE="HIDDEN" NAME="helpfulnessid" VALUE="12091">...
        //                                                   ^                    ^      ^
        //                                                   |                    |      |
        //                                 IndexOf(helpfulnessid)             ind_b      ind_e
        // 
        ind_a = str_acc.IndexOf("helpfulnessid");
        str_acc = str_acc.Substring(ind_a);
        ind_b = str_acc.IndexOf("=") + 1;
        ind_e = str_acc.Substring(ind_b).IndexOf(">");
        //    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc.Substring(ind_b, ind_e)= {2}", 
        //      ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
        try
        {
            reviewhelpfulnessid_out = Convert.ToInt32(str_acc.Substring(ind_b, ind_e).Trim('"'));
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error in parsing helpfulnessid: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}",
              e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
            return (false);
        }

        


        return (true);
    } // end ds2newreviewhelpfulness()






//
//-------------------------------------------------------------------------------------------------
//
    public bool ds2purchase(int cart_items, int[] prod_id_in, int[] qty_in, int customerid_out,
      ref int neworderid_out, ref bool IsRollback, ref double rt)
      {
      int i, count, ind_b, ind_e;
#if (USE_WIN32_TIMER)
      long ctr0 = 0, ctr = 0, freq = 0;
#else
      TimeSpan TS = new TimeSpan();
      DateTime DT0;
#endif  
      //changed Controller.target to target_server_name
      URL = 
        "http://" + target_server_name + "/" + Controller.virt_dir + "/dspurchase." + Controller.page_type + 
        "?confirmpurchase=yes&customerid=" + customerid_out;
      for (i=0; i<cart_items; i++)
        {
        if (Controller.page_type == "php") 
          URL = URL + "&item%5B%5D=" + prod_id_in[i] + "&quan%5B%5D=" + qty_in[i];
        else URL = URL + "&item=" + prod_id_in[i] + "&quan=" + qty_in[i];
        }
             
      httpWebRequest = (HttpWebRequest) WebRequest.Create(URL); 
//    Console.WriteLine("Thread {0}: Calling dspurchase." + Controller.page_type + " w/ URL=\n  {1}",
//      Thread.CurrentThread.Name, URL); 

#if (USE_WIN32_TIMER)
      QueryPerformanceFrequency(ref freq); // obtain system freq (ticks/sec)  
      QueryPerformanceCounter(ref ctr0); // Start response time clock   
#else
      DT0 = DateTime.Now;
#endif  

      // httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
      // Added exception handling to flag specific cause of errors and terminate dead threads
      try
        {
        httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse(); 
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Error during ds2purchase returned from web server: {0}", e.Message);
        return(false);
        }
      receiveStream = httpWebResponse.GetResponseStream();
      readStream = new StreamReader( receiveStream, encode );
      str_acc = "";
      do  
        {
        count = readStream.Read(read, 0, 256);
        String str = new String(read, 0, count);
        str_acc = str_acc + str;
        } while (count > 0);
        
#if (USE_WIN32_TIMER)
      QueryPerformanceCounter(ref ctr); // Stop response time clock
      rt = (ctr - ctr0)/(double) freq; // Calculate response time
#else
      TS = DateTime.Now - DT0;
      rt = TS.TotalSeconds; // Calculate response time
#endif  
            
//    Console.WriteLine("Thread {0}:  ds2purchase str_acc length: {1}  str_acc: \n{2}\n", 
//      Thread.CurrentThread.Name, str_acc.Length, str_acc);          
      
      if (str_acc.IndexOf("Insufficient stock") > 0)
        {
        IsRollback = true;
        return(true);
        }  

//      String to parse: ...--- ORDER NUMBER:  12028</H2><BR>...
//                              ^              ^    ^
//                              |              |    |
//          IndexOf(ORDER NUMBER)          ind_b    ind_e
 
      ind_b = 15 + str_acc.IndexOf("ORDER NUMBER");
      ind_e = str_acc.Substring(ind_b).IndexOf("<");
      neworderid_out = 0;
//    Console.WriteLine("ind_b= {0}  ind_e= {1}  str_acc[]= {2}, neworderid_out= {3}", 
//      ind_b, ind_e, str_acc.Substring(ind_b, ind_e), neworderid_out);
      
      try
        {
        neworderid_out = Convert.ToInt32(str_acc.Substring(ind_b, ind_e));
        }
      catch (System.Exception e) 
        {
        Console.WriteLine("Error in parsing neworderid: {0}; ind_b= {1}  ind_e= {2}  str_acc[]= {3}", 
          e.Message, ind_b, ind_e, str_acc.Substring(ind_b, ind_e));
        return(false);
        }

      return(true);
      } // end ds2purchase()
    
//
//-------------------------------------------------------------------------------------------------
//
    public bool ds2close()
      {      
      // Release the resources of stream object.
      readStream.Close();
      // Release the resources of response object.
      httpWebResponse.Close();      
      return(true);
      } // end ds2close()
    } // end Class ds2Interface
  } // end namespace ds2xdriver
  
        