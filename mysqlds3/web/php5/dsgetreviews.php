
<?php
/*  
 * DVD Store Get Reviews PHP Page - dsgetreviews.php
 *
 * Copyright (C) 2005 Dell, Inc. <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
 *
 * Gets Revuewsof products in MySQL DVD store by author, title, product id, date, and star ranking
 *
 * Last Updated 6/16/15
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

ds_html_header("DVD Store Get Product Reviews Page");

$customerid = $_REQUEST["customerid"];
$review_title = isset($_REQUEST["review_title"]) ? $_REQUEST["review_title"] : NULL;
$date_order = isset($_REQUEST["date_order"]) ? $_REQUEST["date_order"] : NULL;
$review_stars = isset($_REQUEST["review_stars"]) ? $_REQUEST["review_stars"] : NULL;
$limit_num = isset($_REQUEST["limit_num"]) ? $_REQUEST["limit_num"] : NULL;
$getreviewtype = isset($_REQUEST["getreviewtype"]) ? $_REQUEST["getreviewtype"] : NULL;
$productid = isset($_REQUEST["productid"]) ? $_REQUEST["productid"] : NULL;

if (empty($customerid))
  {
  echo "<H2>You have not logged in - Please click below to Login to DVD Store</H2>\n";
  echo "<FORM ACTION='./dslogin.php' METHOD=GET>\n";
  echo "<INPUT TYPE=SUBMIT VALUE='Login'>\n";
  echo "</FORM>\n";
  ds_html_footer();
  exit;
  }

echo "<H2>Select Type of Search for $review_title </H2>\n";

echo "<FORM ACTION='./dsgetreviews.php' METHOD='GET'>\n";
echo "<INPUT NAME='getreviewtype' TYPE=RADIO VALUE='noorder'"; if($getreviewtype == 'noorder') echo "CHECKED";
echo ">Title  <INPUT NAME='review_title' VALUE='$review_title' readonly TYPE=TEXT SIZE=15> <BR>\n";
echo "<INPUT NAME='getreviewtype' TYPE=RADIO VALUE='date'"; if($getreviewtype == 'date') echo "CHECKED";
echo ">Date Order <BR>\n";
echo "<INPUT NAME='getreviewtype' TYPE=RADIO VALUE='star'"; if($getreviewtype == 'star') echo "CHECKED"; echo ">Star Level\n";
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

echo "Number of search results to return\n";
echo "<SELECT NAME='limit_num'>\n";
for ($i=1; $i<11; $i++)
  {
  if ($i == $limit_num)
    {echo "  <OPTION VALUE=$i SELECTED>$i</OPTION>\n";}
  else
    {echo "  <OPTION VALUE=$i>$i</OPTION>\n";}
  }
echo "</SELECT><BR>\n";

echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE='$customerid'>\n";
echo "<INPUT TYPE=HIDDEN NAME=productid VALUE='$productid'>\n";
echo "<INPUT TYPE=SUBMIT VALUE='Search'>\n";
echo "</FORM>\n";

if (!empty($getreviewtype))
  {
  if (!($link_id = mysql_pconnect())) die(mysql_error());

  switch ($getreviewtype)
    {
    case "noorder":
      $getreview_query = "SELECT REVIEWS.review_id, REVIEWS.prod_id, REVIEWS.review_date, REVIEWS.stars, " .
                    "REVIEWS.customerid,REVIEWS.review_summary, REVIEWS.review_text, SUM(REVIEWS_HELPFULNESS.helpfulness) " .
                    "as total FROM DS3.REVIEWS INNER JOIN DS3.REVIEWS_HELPFULNESS " .
		    "on REVIEWS.review_id=REVIEWS_HELPFULNESS.review_id " .
                    "WHERE PROD_ID = " . $productid . " GROUP BY REVIEWS.review_id ORDER BY total DESC LIMIT $limit_num;";
      break;
    case "date":
      $getreview_query ="SELECT REVIEWS.review_id, REVIEWS.prod_id, REVIEWS.review_date, REVIEWS.stars, " .
                    "REVIEWS.customerid,REVIEWS.review_summary, REVIEWS.review_text, SUM(REVIEWS_HELPFULNESS.helpfulness) " .
                    "as total FROM DS3.REVIEWS INNER JOIN DS3.REVIEWS_HELPFULNESS " .
		    "on REVIEWS.review_id=REVIEWS_HELPFULNESS.review_id " .
                    "WHERE PROD_ID = " . $productid . " GROUP BY REVIEWS.review_id ORDER BY REVIEW_DATE DESC LIMIT $limit_num;"; 
      break;
    case "star":
      $getreview_query = "SELECT REVIEWS.review_id, REVIEWS.prod_id, REVIEWS.review_date, REVIEWS.stars, " .
                    "REVIEWS.customerid,REVIEWS.review_summary, REVIEWS.review_text, SUM(REVIEWS_HELPFULNESS.helpfulness) " .
                    "as total FROM DS3.REVIEWS INNER JOIN DS3.REVIEWS_HELPFULNESS " . 
		    "on REVIEWS.review_id=REVIEWS_HELPFULNESS.review_id " .
                    "WHERE PROD_ID = " . $productid . " AND STARS = " . $review_stars . 
                    " GROUP BY REVIEWS.review_id ORDER BY total DESC LIMIT $limit_num;";
      break;
    }

  $getreviews_result = mysql_query($getreview_query);

  if (mysql_num_rows($getreviews_result) == 0)
    {
    echo "<H2>No Reviews Found</H2>\n";
    }
  else
    {
    echo "<BR>\n";
    echo "<H2> Most Helpful Reviews for $review_title </H2>\n";
    while ($getreviews_result_row = mysql_fetch_row($getreviews_result))
      {
      echo "----------------------------------------------------------------------------------------------<BR>";
      echo " Review Summary - $getreviews_result_row[5]<BR>\n";
      echo " Rated $getreviews_result_row[3] stars<BR>\n";
      echo " Review Created By $getreviews_result_row[4] on $getreviews_result_row[2]<BR>\n";
      echo " $getreviews_result_row[6]<BR>\n";
      echo "<FORM ACTION='./dsnewhelpfulness.php' METHOD='GET'>\n";
      echo "Helpfulness ranking of this review (10 is most helpful) \n";
      $helpfulness_levels = array("1","2","3","4","5","6","7","8","9","10");

      echo "<SELECT NAME='review_helpfulness'>\n";
      for ($i=0; $i<count($helpfulness_levels); $i++)
        {
        $j=$i+1;
        echo "  <OPTION VALUE=$j>$helpfulness_levels[$i]</OPTION>\n";
        }
      echo "</SELECT><BR>\n";
      echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
      echo "<INPUT TYPE=HIDDEN NAME=reviewid VALUE=$getreviews_result_row[0]>\n";
      echo "<INPUT TYPE=HIDDEN NAME=productid VALUE=$getreviews_result_row[1]>\n";
      echo "<INPUT TYPE=HIDDEN NAME=helpfulness_sum VALUE=$getreviews_result_row[7]>\n";
      echo "<INPUT TYPE='submit' VALUE='Submit Helpfulness Rating'>\n";
      echo "</FORM>\n";
      echo "<FORM ACTION='./dsnewreview.php' METHOD='GET'>\n";
      echo "OR \n";
      echo "<INPUT TYPE=HIDDEN NAME=customerid VALUE=$customerid>\n";
      echo "<INPUT TYPE=HIDDEN NAME=productid VALUE=$getreviews_result_row[1]>\n";
      echo "<INPUT TYPE=HIDDEN NAME=review_title VALUE='$review_title'>\n";
      echo "<INPUT TYPE='submit' VALUE='Create a New Review'>\n";
      echo "</FORM>\n";
      }      
    mysql_free_result($getreviews_result);

    echo "</FORM>\n";
    }
  mysql_close($link_id);
  }

ds_html_footer();
?>
