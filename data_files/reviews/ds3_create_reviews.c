
/*
 * DVD Store 3 Create Product data - ds3_create_reviews.c
 *
 * Created 8/22/14 and based on previous ds2 create data programs 
 *
 * Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
 *
 * Creates product data files for DVD Store Database V.3
 * Syntax: ds3_create_reviews n_prods avg_reviews_per_prod n_customers n_Sys_Type 
 * Builds DS database reviews load file
 * Run on Linux to use large RAND_MAX (2e31-1)
 * To compile: gcc -o ds3_create_reviews ds3_create_reviews.c -lm
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


#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <math.h>

#include "ds_reviews_data.h"

int    review_id, prod_id, stars_num, cust_id, reviews_tot_num, n_customers, n_reviews_per_prod;
int    i, j, k, i_month, i_day_of_month;
int    review_helpfulness_id, helpfulness_rating, num_helpfulness_reviews;
int    i_review_length, cur_review_length;
char   review_summary[150], review_text[2000], review_date[10];

int    i_days_in_month[] = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

char   fn_reviews[35], fn_review_helpfulness[35];
// FILE*  fp;
FILE   *FP_reviews, *FP_review_helpfulness;

int main(int argc, char* argv[])
  {
  int n_prods, i;
  int i_Sys_Type = 0;	 //0 for Linux, 1 for Windows        

  // Check syntax
    if (argc < 5)    
    {
    fprintf(stderr, "Syntax: ds3_create_reviews n_prods avg_reviews_per_prod n_customers n_Sys_Type\n");  
    fprintf(stderr, "n_Sys_Type can be 0 (Linux) or 1 (Windows) \n"); 
    exit(-1);
    }

  n_prods = atoi(argv[1]);
  n_reviews_per_prod = atoi(argv[2]);
  n_customers = atoi(argv[3]);
  i_Sys_Type = atoi(argv[4]);    

  sprintf(fn_reviews, "reviews.csv");
  sprintf(fn_review_helpfulness, "review_helpfulness.csv");
  
  FP_reviews = fopen(fn_reviews, "wb");
  FP_review_helpfulness = fopen(fn_review_helpfulness, "wb");

  reviews_tot_num = n_prods * n_reviews_per_prod;

  review_id = 0;
  review_helpfulness_id = 0;

  for (i = 0; i < reviews_tot_num; i++)
    {
    review_id = i+1;

    prod_id = random2(1, n_prods);

    i_month = random2 (1,12);
    i_day_of_month = random2 (1, i_days_in_month[i_month-1]);
    sprintf(review_date,"%4d/%02d/%02d", 2013, i_month, i_day_of_month);

    stars_num = random2 (1,5);

    cust_id = random2 (1, n_customers);
    
    // Generate random three word review summary 5000 review word
    
    sprintf(review_summary, "%s %s %s", review_words[random2(0,4950)], review_words[random2(0,4950)], review_words[random2(0,4950)]);
    //sprintf(review_summary,"Sum Sum Sum");

    // Generate random review text based on 5000 review words
    // Length of review is limited by the size of i_review_length which is the number of words in the review
    sprintf(review_text,"");
    i_review_length = random2(7,130);
    for (j = 1; j < i_review_length; j++)
      {
      sprintf(review_text + strlen(review_text), "%s ", review_words[random2(0,4950)]);
      }


    if(i_Sys_Type == 0)   //If System is Linux, Append LF only    
    {	
	fprintf(FP_reviews, "%d,%d,%s,%d,%d,%s,%s%c", review_id, prod_id, review_date, stars_num, cust_id, review_summary, review_text, 10);
    }	
    else if(i_Sys_Type == 1) //If System is Windows, Append CR+LF     
    {
	fprintf(FP_reviews, "%d,%d,%s,%d,%d,%s,%s%c%c", review_id, prod_id, review_date, stars_num, cust_id, review_summary, review_text, 13, 10);
    }
    // Create a random number of review helpfulness ratings for each created review
    num_helpfulness_reviews = random2(3,40);
    for (k = 1; k < num_helpfulness_reviews; k++)
      {
      review_helpfulness_id = review_helpfulness_id + 1;
      helpfulness_rating = random2(1,10);
      if(i_Sys_Type == 0)   //If System is Linux, Append LF only
        {
        fprintf(FP_review_helpfulness, "%d,%d,%d,%d%c", review_helpfulness_id, review_id, cust_id, helpfulness_rating, 10);
        }
        else if(i_Sys_Type == 1) //If System is Windows, Append CR+LF
        {
        fprintf(FP_review_helpfulness, "%d,%d,%d,%d%c%c", review_helpfulness_id, review_id, cust_id, helpfulness_rating, 13, 10);
        }
      }


    }


  }

//---------------------------------------random2---------------------------------------
//
// random2(i,j) - returns a random integer uniformly distributed between i and j,
//  inclusively  (assumes j >= i)
//
int random2(int i, int j)
        {
        return i + floor((1+j-i) * (double) rand()/(((double) RAND_MAX) + 1));
        } //random2
//
