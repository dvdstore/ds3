/*
 * DVD Store 3 Create Customer data - ds3_create_membership.c
 *
 * Copyright (C) 2014 VMware, Inc. <tmuirhead@vmware.com>
 *
 * Creates premier membership data files for DVD Store Database V.3
 * Syntax: ds3_create_membership n_customers n_pct S|M|L n_Sys_Type > output_file
 *         (see details below)
 * Run on Linux to use large RAND_MAX (2e31-1)
 * To compile: gcc -o ds3_create_membership ds3_create_membership.c -lm
 * 
 *  Adapted from ds2_create_cust.c originally written by Dave Jaffe, Todd Muirhead, and enhanced by Girish Khandke
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
#include <time.h>

// Functions
int random2(int i, int j);
double random2d(double i, double j);

main(int argc, char* argv[])
  {
  int n_cust, n_pct, n_cust_members, n_interval_size, adder=0;
  int r_custid, r_membership_type, r_year, r_month;
  char* ind;

  char n_cust_str[20], n_pct_str[20], r_membership_exp[25];
  int i, prev_interval, next_n_interval_size;
  char   fn_member[35];
  FILE   *FP_member;

  float flt_interval, cur_flt_interval = 0;


  int i_Sys_Type = 0;	//0 for Linux, 1 for Windows        
  char str_Sys_Type[20]; 

  // Check syntax
    if (argc < 3)     
    {
    fprintf(stderr, "Syntax: ds3_create_membership n_cust n_pct n_Sys_Type > output_file\n");  //Changed Syntax by GSK
    fprintf(stderr, "        where n_cust is the total number of customers and can contain M or m for millions\n");
    fprintf(stderr, "Creates customer data membership files for DS3 database\n");
    fprintf(stderr, "n_Sys_Type can be 0 (Linux) or 1 (Windows)\n");		//Changed Syntax by GSK    
    fprintf(stderr, "Examples: ds3_create_membership  1000  20  0  =>  20% of 1000 users are premier members \n");
    fprintf(stderr, "          ds3_create_membership  1M    50  0  =>  50% of 1 million users are premier members\n");
    exit(-1);
    }
  
  strcpy(n_cust_str,  argv[1]);
  if (!(ind = strpbrk(n_cust_str, "Mm")))
    { 
    n_cust = atoi(n_cust_str);
    }
  else
    {
    n_cust = 1000000 * atoi(n_cust_str) + adder;
    }

  strcpy(n_pct_str, argv[2]);          
  n_pct = atoi(n_pct_str);            

  strcpy(str_Sys_Type,argv[3]);   
  i_Sys_Type = atoi(str_Sys_Type);

  sprintf(fn_member, "membership.csv");
  
  FP_member = fopen(fn_member, "wb");

  srand(n_cust); // Seed rand() with n_cust

  n_cust_members = floor((n_cust / 100) * n_pct);
  n_interval_size = floor(n_cust / n_cust_members);
  flt_interval = (float)n_cust / (float)n_cust_members;

  prev_interval = n_interval_size;
  cur_flt_interval = flt_interval;

  for (i=1; i<=n_cust; (i=i+n_interval_size) ) 
    {
      next_n_interval_size = floor((cur_flt_interval - n_interval_size) + flt_interval);      

      r_custid = random2(i, i+next_n_interval_size-1);
      r_custid = abs(r_custid);

      cur_flt_interval = (cur_flt_interval - n_interval_size) + flt_interval; 
      
      n_interval_size = floor(cur_flt_interval);


      r_membership_type = random2(1, 3);

      r_year  = 2014 + random2(1, 5);
      r_month = random2(1, 12);
      sprintf(r_membership_exp,"%4d/%02d/15", r_year, r_month);


      if(i_Sys_Type == 0)   //If System is Linux, Append LF only    
      {  	
	    fprintf(FP_member, "%d,%d,%s%c",
	      r_custid, r_membership_type, r_membership_exp, 10);
      }  	
      else if(i_Sys_Type == 1) //If System is Windows, Append CR+LF      
      {
	    fprintf(FP_member, "%d,%d,%s%c%c",
	      r_custid, r_membership_type, r_membership_exp, 13, 10);	
      }

    } //End of For
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
//
//---------------------------------------random2d---------------------------------------
//
// random2d(i,j) - returns a random double uniformly distributed between i and j,
//  inclusively  (assumes j >= i)                        
//
double random2d(double i, double j)
    {
    return i + floor((1+j-i) * (double) rand()/(((double) RAND_MAX) + 1));
    } //random2
//
