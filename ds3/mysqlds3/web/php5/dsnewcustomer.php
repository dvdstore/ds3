
<?php
/*  
 * DVD Store New Customer PHP Page - dsnewcustomer.php
 *
 * Copyright (C) 2005 Dell, Inc. <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
 *
 * Prompts for new customer data; creates new entry in MySQL DVD Store CUSTOMERS table
 *
 * Last Updated 6/9/15
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
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/ 

include("dscommon.inc");

ds_html_header("New Customer Login");

$firstname = isset($_REQUEST["firstname"]) ? $_REQUEST["firstname"] : NULL;
$lastname  = isset($_REQUEST["lastname"]) ? $_REQUEST["lastname"] : NULL;
$address1  = isset($_REQUEST["address1"]) ? $_REQUEST["address1"] : NULL;
$address2  = isset($_REQUEST["address2"]) ? $_REQUEST["address2"] : NULL;
$city      = isset($_REQUEST["city"]) ? $_REQUEST["city"] : NULL;
$state     = isset($_REQUEST["state"]) ? $_REQUEST["state"] : NULL;
$zip       = isset($_REQUEST["zip"]) ? $_REQUEST["zip"] : NULL;
$country   = isset($_REQUEST["country"]) ? $_REQUEST["country"] : NULL;
$email     = isset($_REQUEST["email"]) ? $_REQUEST["email"] : NULL;
$phone     = isset($_REQUEST["phone"]) ? $_REQUEST["phone"] : NULL;
$creditcardtype   = isset($_REQUEST["creditcardtype"]) ? $_REQUEST["creditcardtype"] : NULL;
$creditcard  = isset($_REQUEST["creditcard"]) ? $_REQUEST["creditcard"] : NULL;
$ccexpmon  = isset($_REQUEST["ccexpmon"]) ? $_REQUEST["ccexpmon"] : NULL;
$ccexpyr   = isset($_REQUEST["ccexpyr"]) ? $_REQUEST["ccexpyr"] : NULL;
$username  = isset($_REQUEST["username"]) ? $_REQUEST["username"] : NULL;
$password  = isset($_REQUEST["password"]) ? $_REQUEST["password"] : NULL;
$age       = isset($_REQUEST["age"]) ? $_REQUEST["age"] : NULL;
$income    = isset($_REQUEST["income"]) ? $_REQUEST["income"] : NULL;
$gender    = isset($_REQUEST["gender"]) ? $_REQUEST["gender"] : NULL;

if (!( empty($firstname) OR empty($lastname) OR empty($address1) OR empty($city) OR empty($country) 
  OR empty($username) OR empty($password) ))
  {
  if (!($link_id=mysql_pconnect())) die(mysql_error());
  $query = "select COUNT(*) from DS3.CUSTOMERS where USERNAME='$username';";
/*  mysqli_real_query($link_id, $query);*/
/*  $result = mysqli_store_result($link_id);*/
  $result = mysql_query($query);
  $row = mysql_fetch_row($result);
  mysql_free_result($result);
  if ($row[0] != 0)
    {
    echo "<H2>Username already in use! Please try another username</H2>\n";
    dsnewcustomer_form($firstname,$lastname,$address1,$address2,$city,$state,$zip,$country,
      $email,$phone,$creditcardtype,$creditcard,$ccexpmon,$ccexpyr,$username,$password,$age,$income,$gender);
    }
  else
    {
    $region = 1;
    if ($country != "US") $region = 2;
    $creditcardexpiration = sprintf("%4d/%02d", $ccexpyr, $ccexpmon);
    $new_customer_proc_call = "call DS3.NEW_CUSTOMER(" .
      "'$firstname','$lastname','$address1','$address2','$city','$state','$zip','$country'," . 
      "'$region','$email','$phone','$creditcardtype', '$creditcard','$creditcardexpiration'," .
      "'$username','$password','$age','$income','$gender',@customerid_out);";
    //echo $new_customer_proc_call;
    mysql_query($new_customer_proc_call);
    $query = "select @customerid_out;";
    $result = mysql_query($query);
    $row = mysql_fetch_row($result);
    $customerid = $row[0];
    mysql_free_result($result);

    echo "<H2>New Customer Successfully Added.  Click below to begin shopping<H2>\n";
    echo "<FORM ACTION='./dsbrowse.php' METHOD=GET>\n";
    echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
    echo "<INPUT TYPE=SUBMIT VALUE='Start Shopping'>\n";
    echo "</FORM>\n";
    }
  mysql_close($link_id);
  }
else
  {
  echo "<H2>New Customer - Please Complete All Required Fields Below (marked with *)</H2>\n";
  dsnewcustomer_form($firstname,$lastname,$address1,$address2,$city,$state,$zip,$country,
    $email,$phone,$creditcardtype,$creditcard,$ccexpmon,$ccexpyr,$username,$password,$age,$income,$gender);
  }

ds_html_footer();
  
function dsnewcustomer_form($firstname,$lastname,$address1,$address2,$city,$state,$zip,$country,
  $email,$phone,$creditcardtype,$creditcard,$ccexpmon,$ccexpyr,$username,$password,$age,$income,$gender)
  {
  $countries = array("United States", "Australia", "Canada", "Chile", "China", "France", "Germany", "Japan", 
                           "Russia", "South Africa", "UK");

  $cctypes = array("MasterCard", "Visa", "Discover", "Amex", "Dell Preferred");
  $months = array("Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec");

  echo "<FORM ACTION='./dsnewcustomer.php' METHOD='GET'>\n";
  echo "Firstname <INPUT TYPE=TEXT NAME='firstname' VALUE='$firstname' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "Lastname <INPUT TYPE=TEXT NAME='lastname' VALUE='$lastname' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "Address1 <INPUT TYPE=TEXT NAME='address1' VALUE='$address1' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "Address2 <INPUT TYPE=TEXT NAME='address2' VALUE='$address2' SIZE=16 MAXLENGTH=50> <BR>\n";
  echo "City <INPUT TYPE=TEXT NAME='city' VALUE='$city' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "State <INPUT TYPE=TEXT NAME='state' VALUE='$state' SIZE=16 MAXLENGTH=50> <BR>\n";
  echo "Zipcode <INPUT TYPE=TEXT NAME='zip' VALUE='$zip' SIZE=16 MAXLENGTH='5'> <BR>\n";
  echo "Country <SELECT NAME='country' SIZE=1>\n";
  for ($i=0; $i<count($countries); $i++)
    {
    if ($countries[$i] == $country)
      {echo "  <OPTION VALUE=\"$countries[$i]\" SELECTED>$countries[$i]</OPTION>\n";}
    else
      {echo "  <OPTION VALUE=\"$countries[$i]\">$countries[$i]</OPTION>\n";}
    }
  echo "</SELECT>* <BR>\n";
  echo "Email <INPUT TYPE=TEXT NAME='email' VALUE='$email' SIZE=16 MAXLENGTH=50> <BR>\n";
  echo "Phone <INPUT TYPE=TEXT NAME='phone' VALUE='$phone' SIZE=16 MAXLENGTH=50> <BR>\n";

  echo "Credit Card Type "; 
  echo "<SELECT NAME='creditcardtype' SIZE=1>\n";
  for ($i=0; $i<5; $i++)
    {
    $j = $i + 1;
    if ($j == $creditcardtype)
      {echo "  <OPTION VALUE=\"$j\" SELECTED>$cctypes[$i]</OPTION>\n";}
    else
      {echo "  <OPTION VALUE=\"$j\">$cctypes[$i]</OPTION>\n";}
    }
  echo "</SELECT>\n";

  echo "  Credit Card Number <INPUT TYPE=TEXT NAME='creditcard' VALUE='$creditcard' SIZE=16 MAXLENGTH=50>\n";

  echo "  Credit Card Expiration "; 
  echo "<SELECT NAME='ccexpmon' SIZE=1>\n";
  for ($i=0; $i<12; $i++)
    {
    $j = $i+1;
    if ($j == $ccexpmon)
      {echo "  <OPTION VALUE=\"$j\" SELECTED>$months[$i]</OPTION>\n";}
    else
      {echo "  <OPTION VALUE=\"$j\">$months[$i]</OPTION>\n";}
    }
  echo "</SELECT>\n";
  echo "<SELECT NAME='ccexpyr' SIZE=1>\n";
  for ($i=0; $i<6; $i++)
    {
    $yr = 2008 + $i;
    if ($yr == $ccexpyr)
      {echo "  <OPTION VALUE=\"$yr\" SELECTED>$yr</OPTION>\n";}
    else
      {echo "  <OPTION VALUE=\"$yr\">$yr</OPTION>\n";}
    }
  echo "</SELECT><BR>\n";

  echo "Username <INPUT TYPE=TEXT NAME='username' VALUE='$username' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "Password <INPUT TYPE='PASSWORD' NAME='password' VALUE='$password' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "Age <INPUT TYPE=TEXT NAME='age' VALUE='$age' SIZE=3 MAXLENGTH=3> <BR>\n";
  echo "Income (\$US) <INPUT TYPE=TEXT NAME='income' VALUE='$income' SIZE=16 MAXLENGTH=50> <BR>\n";
  echo "Gender <INPUT TYPE=RADIO NAME='gender' VALUE=\"M\" "; if($gender == 'M') echo "CHECKED"; echo "> Male \n";
  echo "       <INPUT TYPE=RADIO NAME='gender' VALUE=\"F\" "; if($gender == 'F') echo "CHECKED"; echo "> Female \n";
  echo "       <INPUT TYPE=RADIO NAME='gender' VALUE=\"?\" "; if($gender == '?' || $gender == '') echo "CHECKED"; echo "> Don't Know <BR>\n";
  echo "<INPUT TYPE='submit' VALUE='Submit New Customer Data'>\n";
  echo "</FORM>\n";
  }

?>
