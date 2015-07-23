
<?php
/*  
 * DVD Store New Member PHP Page - dsnewmember.php
 *
 * Copyright (C) 2005 Dell, Inc. <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
 *
 * Prompts for new member data; creates new entry in MySQL DVD Store MEMBERSHIP table
 *
 * Last Updated 6/8/15
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

ds_html_header("New Premium Membership Signup");

$customerid = $_REQUEST["customerid"];
$membershiplevel = $_REQUEST["membershiplevel"];

if (empty($customerid))
  {
  echo "<H2>You have not logged in - Please click below to Login to DVD Store</H2>\n";
  echo "<FORM ACTION='./dslogin.php' METHOD=GET>\n";
  echo "<INPUT TYPE=SUBMIT VALUE='Login'>\n";
  echo "</FORM>\n";
  ds_html_footer();
  exit;
  }

if (!(empty($membershiplevel)))
  {
  if (!($link_id=mysql_pconnect())) die(mysql_error());
  $query = "select COUNT(*) from DS3.MEMBERSHIP where CUSTOMERID='$customerid';";
/*  mysqli_real_query($link_id, $query);*/
/*  $result = mysqli_store_result($link_id);*/
  $result = mysql_query($query);
  $row = mysql_fetch_row($result);
  mysql_free_result($result);
  if ($row[0] != 0)
    {
    echo "<H2>You are already a Premium Member! Enjoy Shopping the DVD Store!</H2>\n";
    echo "<FORM ACTION='./dsbrowse.php\' METHOD=GET>\n";
    echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
    echo "<INPUT TYPE=SUBMIT VALUE='Browse'>\n";
    echo "</FORM>\n";
    ds_html_footer();
    exit;
    }
  else
    {
    if (($membershiplevel >= 1) AND ($membershiplevel <= 3))
      {
      $new_member_proc_call = "call DS3.NEW_MEMBER(" .
      "'$customerid','$membershiplevel', @customerid_out);";
      //echo $new_member_proc_call;
      mysql_query($new_member_proc_call);
      $query = "select @customerid_out;";
      $result = mysql_query($query);
      $row = mysql_fetch_row($result);
      $customerid = $row[0];
      mysql_free_result($result);

      echo "<H2>New Premium Membership Successful.  Click below to begin shopping<H2>\n";
      echo "<FORM ACTION='./dsbrowse.php' METHOD=GET>\n";
      echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
      echo "<INPUT TYPE=SUBMIT VALUE='Start Shopping'>\n";
      echo "</FORM>\n";
      ds_html_footer();
      mysql_close($link_id);
      exit;
      }
    }
  }
else
  {
  echo "<H2>New Premium Membership - Select Desired Level</H2>\n";
  echo "<FORM ACTION='./dsnewmember.php' METHOD='GET'>\n";
  echo "<INPUT TYPE='radio' name='membershiplevel' value='1'>Gold <BR>\n";
  echo "<INPUT TYPE='radio' name='membershiplevel' value='2'>Silver <BR>\n";
  echo "<INPUT TYPE='radio' name='membershiplevel' value='3'>Bronze <BR>\n";
  echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
  echo "<INPUT TYPE=submit value='Submit'>\n";
  echo "</FORM>\n";
  }
ds_html_footer();
?>
