
<?php
/*  
 * DVD Store New Product Review PHP Page - dsnewrevuew.php
 *
 * Copyright (C) 2005 Dell, Inc. <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
 *
 * Prompts for new reivew content; creates new entry in MySQL DVD Store REVIEWS table
 *
 * Last Updated 6/10/15
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

ds_html_header("New Review Entry");

$customerid = $_REQUEST["customerid"];
$productid = $_REQUEST["productid"];
$review_stars = $_REQUEST["review_stars"];
$review_summary = $_REQUEST["review_summary"];
$review_text = $_REQUEST["review_text"];

if (empty($customerid))
  {
  echo "<H2>You have not logged in - Please click below to Login to DVD Store</H2>\n";
  echo "<FORM ACTION='./dslogin.php' METHOD=GET>\n";
  echo "<INPUT TYPE=SUBMIT VALUE='Login'>\n";
  echo "</FORM>\n";
  ds_html_footer();
  exit;
  }

if (empty($productid))
  {
  echo "<H2>You have not selected a product - Please click below to Browse DVD Store</H2>\n";
  echo "<FORM ACTION='./dsbrowse.php' METHOD=GET>\n";
  echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
  echo "<INPUT TYPE=SUBMIT VALUE='Login'>\n";
  echo "</FORM>\n";
  ds_html_footer();
  exit;
  }

if (!(empty($productid) OR empty($review_stars) OR empty($review_summary) OR empty($review_text)))
  {
  if (!($link_id=mysql_pconnect())) die(mysql_error());
  $new_review_proc_call = "call DS3.NEW_PROD_REVIEW(" .
  "'$productid','$review_stars','$customerid','$review_summary','$review_text', @reviewid_out);";
  mysql_query($new_review_proc_call);
  $query = "select @reviewid_out;";
  $result = mysql_query($query);
  $row = mysql_fetch_row($result);
  $reviewid = $row[0];
  mysql_free_result($result);
      echo "<H2>New Review Added.  Click below to return to shopping<H2>\n";
      echo "<FORM ACTION='./dsbrowse.php' METHOD=GET>\n";
      echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
      echo "<INPUT TYPE=HIDDEN NAME=reviewid VALUE=$reviewid>\n";
      echo "<INPUT TYPE=SUBMIT VALUE='Return to Shopping'>\n";
      echo "</FORM>\n";
      ds_html_footer();
      mysql_close($link_id);
      exit;
  }
else
  {
  if (!($link_id = mysql_pconnect())) die(mysql_error());
  $get_product_title_query = "select title from DS3.PRODUCTS where PROD_ID = '" . $productid . "';";
  $get_title_result = mysql_query($get_product_title_query);
  $get_title_result_row = mysql_fetch_row($get_title_result);
  $producttitle = $get_title_result_row[0];
  mysql_free_result($get_title_result);
  echo "<H2>New Product Review  - Please Complete All Fields Below (marked with *)</H2>\n";
  dsnewreview_form($productid,$review_stars,$customerid,$review_summary,$review_text,$producttitle);
  }

ds_html_footer();

function dsnewreview_form($productid,$review_stars,$customerid,$review_summary,$review_text,$producttitle)
  {
  echo "<FORM ACTION='./dsnewreview.php' METHOD='GET'>\n";
  echo "Product ID <INPUT TYPE=TEXT NAME='productid' VALUE='$productid' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "Movie Title <INPUT TYPE=TEXT NAME='producttitle' VALUE='$producttitle' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "Review Summary <INPUT TYPE=TEXT NAME='review_summary' VALUE='$review_summary' SIZE=16 MAXLENGTH=50>* <BR>\n";
  echo "Review </BR><TEXTAREA NAME='review_text' COLS='70' ROWS='5'>$review_text </TEXTAREA> <BR>\n";
  echo "Stars Ranking \n";  
  $star_levels = array("*", "**", "***", "****", "*****");

  echo "<SELECT NAME='review_stars'>\n";
  for ($i=0; $i<count($star_levels); $i++)
    {
    $j=$i+1;
    if ($j == $review_stars)
      {echo "  <OPTION VALUE=$j SELECTED>$star_levels[$i]</OPTION>\n";}
    else
      {echo "  <OPTION VALUE=$j>$star_levels[$i]</OPTION>\n";}
    }
  echo "</SELECT><BR>\n";
  echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE='$customerid'>\n";
  echo "<INPUT TYPE='submit' VALUE='Submit New Product Review'>\n";
  echo "</FORM>\n";
  }

?>
