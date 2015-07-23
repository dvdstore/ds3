
<?php
/*  
 * DVD Store Review Helpfulness Rating PHP Page - dsnewrevuew.php
 *
 * Copyright (C) 2005 Dell, Inc. <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
 *
 * Allows for a product review to be rated for it' s helpfulness on a scale of 1 to 10. 
 * Updates are made to the REVIEWS_HELPFULNESS table via a stored procedure
 *
 * Last Updated 6/23/15
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

ds_html_header("Review Helpfulness Rating");

$customerid = $_REQUEST["customerid"];
$reviewid = $_REQUEST["reviewid"];
$review_helpfulness = $_REQUEST["review_helpfulness"];

if (empty($customerid))
  {
  echo "<H2>You have not logged in - Please click below to Login to DVD Store</H2>\n";
  echo "<FORM ACTION='./dslogin.php' METHOD=GET>\n";
  echo "<INPUT TYPE=SUBMIT VALUE='Login'>\n";
  echo "</FORM>\n";
  ds_html_footer();
  exit;
  }

if (empty($reviewid))
  {
  echo "<H2>You have not selected a review to rate for helpfulness - Please click below to Browse Reviews</H2>\n";
  echo "<FORM ACTION='./dsgetreviews.php' METHOD=GET>\n";
  echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
  echo "<INPUT TYPE=SUBMIT VALUE='Browse Reviews'>\n";
  echo "</FORM>\n";
  ds_html_footer();
  exit;
  }

if (!(empty($reviewid) OR empty($review_helpfulness)))
  {
  if (!($link_id=mysql_pconnect())) die(mysql_error());
  $new_helpfulness_proc_call = "call DS3.NEW_REVIEW_HELPFULNESS(" .
  "'$reviewid','$customerid','$review_helpfulness', @review_helpfulness_out);";
  mysql_query($new_helpfulness_proc_call);
  $query = "select @review_helpfulness_out;";
  $result = mysql_query($query);
  $row = mysql_fetch_row($result);
  $review_helpfulness_out = $row[0];
  mysql_free_result($result);
      echo "<H2>Review Helpfulness Rating Added.  Click below to return to shopping<H2>\n";
      echo "<FORM ACTION='./dsbrowse.php' METHOD=GET>\n";
      echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
      echo "<INPUT TYPE=HIDDEN NAME=helpfulnessid VALUE=$review_helpfulness_out>\n";
      echo "<INPUT TYPE=SUBMIT VALUE='Return to Shopping'>\n";
      echo "</FORM>\n";
      ds_html_footer();
      mysql_close($link_id);
      exit;
  }
else
  {
  if (!($link_id = mysql_pconnect())) die(mysql_error());
  $get_review_query = "select * from DS3.REVIEWS where REVIEW_ID = '" . $reviewid . "';";
  $get_review_result = mysql_query($get_review_query);
  $get_review_result_row = mysql_fetch_row($get_review_result);
  echo "----------------------------------------------------------------------------------------------<BR>";
  echo " Review Summary - $get_review_result_row[5]<BR>\n";
  echo " Rated $get_review_result_row[3] stars<BR>\n";
  echo " Review Created By $get_review_result_row[4] on $get_review_result_row[2]<BR>\n";
  echo " $get_review_result_row[6]<BR>\n";
  mysql_free_result($get_review_esult);
  echo "<H2>Your Helpfulness Rating for This Reveiew</H2>\n";
  echo "<FORM ACTION='./dsnewhelpfulness.php' METHOD='GET'>\n";
  echo "Helpfulness Ranking (10 is most helpful) \n";
  $helpfulness_levels = array("1","2","3","4","5","6","7","8","9","10");

  echo "<SELECT NAME='review_helpfulness'>\n";
  for ($i=0; $i<count($helpfulness_levels); $i++)
    {
    $j=$i+1;
    if ($j == $review_helpfulness)
      {echo "  <OPTION VALUE=$j SELECTED>$helpfulness_levels[$i]</OPTION>\n";}
    else
      {echo "  <OPTION VALUE=$j>$helpfulness_levels[$i]</OPTION>\n";}
    }
  echo "</SELECT><BR>\n";
  echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE='$customerid'>\n";
  echo "<INPUT TYPE=HIDDEN NAME=reviewid VALUE='$reviewid'>\n";
  echo "<INPUT TYPE='submit' VALUE='Submit Review Helpfulness Rating'>\n";
  echo "</FORM>\n";
  }

ds_html_footer();

?>
