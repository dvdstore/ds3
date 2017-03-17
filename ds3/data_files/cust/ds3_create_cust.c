/*
 * DVD Store 2 Create Customer data - ds2_create_cust.c
 *
 * Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
 *
 * Creates customer data files for DVD Store Database V.2
 * Syntax: ds2_create_cust n_first n_last region_str S|M|L n_Sys_Type > output_file
 *         (see details below)
 * Run on Linux to use large RAND_MAX (2e31-1)
 * To compile: gcc -o ds3_create_cust ds3_create_cust.c -lm
 * Last Updated 5/12/05
 * Last Updated 6/11/2010 by GSK
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

#define MAX_PROD_ID_SMALL 10000
#define MAX_PROD_ID_MED   100000
#define MAX_PROD_ID_LARGE 1000000

// Functions
int random2(int i, int j);
double random2d(double i, double j);

main(int argc, char* argv[])
  {
  int n_first, n_last, adder=0, max_prod_id;
  char* ind;
  char n_first_str[20], n_last_str[20], region_str[3], lower_region_str[3], size;
  int i, j, m, k, i_user, n_prev_orders;
  int customerid, state_index, country_index, zip, region, creditcard_type, i_month, i_year, age, income;
  double phone, creditcard;
  char   firstname[7], lastname[11], city[10], a[6], b[10], c[10], state[20], country[50], creditcard_exp[25], email[25];
  char   username[25], password[25], address[25], gender[5];
  char   fn_cust[35];
  FILE   *FP_cust;


  char *states[] = {"AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "HI", "IA", "ID", "IL", "IN", 
                    "KS", "KY", "LA", "MA", "MD", "ME", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE", "NH", "NJ",
                    "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VA", "VT", "WA",
                    "WI", "WV", "WY"};

  char *countries[] = {"Australia", "Canada", "Chile", "China", "France", "Germany", "Japan", "Russia", "South Africa", "UK"};

  int i_days_in_month[] = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

  int i_Sys_Type = 0;	//0 for Linux, 1 for Windows        //Added by GSK
  char str_Sys_Type[20]; //Added by GSK

  // Check syntax
  //if (argc < 5)
    if (argc < 6)     //Changed By GSK
    {
    fprintf(stderr, "Syntax: ds3_create_cust n_first n_last region_str S|M|L n_Sys_Type \n");  //Changed Syntax by GSK
    fprintf(stderr, "        where n_first, n_last can contain M or m for millions\n");
    fprintf(stderr, "        the M or m can be followed by a +n\n");
    fprintf(stderr, "Creates customer data files for DS2 database\n");
    fprintf(stderr, "Region_str can be US or ROW\n");
    fprintf(stderr, "S,M,L uses product ids up to %d, %d, and %d, respectively\n", 
      MAX_PROD_ID_SMALL, MAX_PROD_ID_MED, MAX_PROD_ID_LARGE);
    fprintf(stderr, "n_Sys_Type can be 0 (Linux) or 1 (Windows)\n");		//Changed Syntax by GSK    
    fprintf(stderr, "Examples: ds2_create_cust  1 1000  US  S 0  =>  US custs        1 -> 1000\n");
    fprintf(stderr, "          ds2_create_cust    1 1M  US  L 0  =>  US custs        1 -> 1000000\n");
    fprintf(stderr, "          ds2_create_cust 1M+1 2M  ROW L 0  =>  ROW custs 1000001 -> 2000000\n");
    exit(-1);
    }
  
  strcpy(n_first_str,  argv[1]);
  if (!(ind = strpbrk(n_first_str, "Mm")))
    { 
    n_first = atoi(n_first_str);
    }
  else
    {
    if (*(ind+1) == '+') adder = atoi(ind+2);
    *ind = '\0';
    n_first = 1000000 * atoi(n_first_str) + adder;
    }

  strcpy(n_last_str,  argv[2]);
  adder = 0;
  if (!(ind = strpbrk(n_last_str, "Mm")))
    { 
    n_last = atoi(n_last_str);
    }
  else
    {
    if (*(ind+1) == '+') adder = atoi(ind+2);
    *ind = '\0';
    n_last = 1000000 * atoi(n_last_str) + adder;
    }

  strcpy(region_str, argv[3]); 
  size = toupper(argv[4][0]);

  strcpy(str_Sys_Type,argv[5]);                //Added by GSK
  i_Sys_Type = atoi(str_Sys_Type);             //Added by GSK

  switch (size)
    {
    case 'S':
      max_prod_id = MAX_PROD_ID_SMALL;
      break;
    case 'M':
      max_prod_id = MAX_PROD_ID_MED;
      break;
    case 'L':
      max_prod_id = MAX_PROD_ID_LARGE;
      break;
    }

  strcpy(lower_region_str, region_str);
  for(i = 0; i < strlen(lower_region_str); i++){
     lower_region_str[i] = tolower(lower_region_str[i]);
     }
  sprintf(fn_cust, "%s_cust.csv", lower_region_str);
  
  FP_cust = fopen(fn_cust, "wb");

  srand(n_first); // Seed rand() with n_first

  for (i=n_first; i<=n_last; i++) 
    {
    for (j=0; j<6; j++)
      {
      a[j] = random2(65, 90);
      } //End of For 

    sprintf(firstname, "%c%c%c%c%c%c", a[0], a[1], a[2], a[3], a[4], a[5]);
   
    for (j=0; j<10; j++)
      {
      b[j] = random2(65, 90);
      } //End of For 

    sprintf(lastname, "%c%c%c%c%c%c%c%c%c%c", b[0], b[1], b[2], b[3], b[4], b[5], b[6], b[7], b[8], b[9]);

    for (j=0; j<7; j++)
      {
      c[j] = random2(65, 90);
      } //End of For
    
    sprintf(city, "%c%c%c%c%c%c%c", c[0], c[1], c[2], c[3], c[4], c[5], c[6]);
    
    if (strncmp(region_str,"US", 2) == 0)
      {
      region = 1;
      state_index    = random2(0, 50);
      strcpy(state,states[state_index]);
      strcpy(country,"US");
      zip = random2(10000, 99999); 
      }
    else 
      {
      region = 2;
      country_index  = random2(0,9);
      strcpy(country,countries[country_index]);
      strcpy(state,"");
      zip = 0;
      } //End Else
    
    customerid     =  i;
    
    phone          =  random2d(1000000000, 9999999999);

    creditcard_type =  random2(1,5);
    creditcard      =  random2d(1000000000000000, 9999999999999999);
    
    i_year  = 2014 + random2(1, 5);
    i_month = random2(1, 12);
    sprintf(creditcard_exp,"%4d/%02d", i_year, i_month);
    
    sprintf(address, "%10.0f Dell Way", phone);
    
    sprintf(email, "%s@dell.com", lastname);      
    
    sprintf(username, "user%d", i);
    
    strcpy(password, "password");  
    
    age  = random2(18, 90); // 18 - 90 yrs of age
    
    income  = 20000*random2(1, 5); // >$20,000, >$40,000, >$60,000, >$80,000, >$100,000

    if (random2(0,1)) strcpy(gender, "M");
        else strcpy(gender, "F");

    if(i_Sys_Type == 0)   //If System is Linux, Append LF only    //Added by GSK
    {	
	    fprintf(FP_cust, "%d,%s,%s,%s,,%s,%s,%05d,%s,%1d,%s,%10.0f,%1d,%16.0f,%s,%s,%s,%d,%d,%s%c",
	      customerid, firstname, lastname, address, city, state, zip, country, region, email, phone, creditcard_type, 
	      creditcard, creditcard_exp, username, password, age, income, gender, 10);
    }	
    else if(i_Sys_Type == 1) //If System is Windows, Append CR+LF   //Added by GSK   
    {
	    fprintf(FP_cust, "%d,%s,%s,%s,,%s,%s,%05d,%s,%1d,%s,%10.0f,%1d,%16.0f,%s,%s,%s,%d,%d,%s%c%c",
	      customerid, firstname, lastname, address, city, state, zip, country, region, email, phone, creditcard_type, 
	      creditcard, creditcard_exp, username, password, age, income, gender, 13, 10);	
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
