
/*
 * DVD Store 3 Create Order data - ds3_create_orders.c
 *
 * Copyright (C) 2005 Dell, Inc. <dave_jaffe@dell.com> and <tmuirhead@vmware.com>
 *
 * Creates order data files for DVD Store Database V.2
 * Syntax: ds3_create_orders n_first n_last filename S|M|L <i_month> n_Sys_Type n_Max_Prod_Id n_Max_Cust_Id
 *         (see details below)
 * Creates <filename>_orders.csv, <filename>_orderlines.csv and <filename>_cust_hist.csv
 *   for month if specified, otherwise for randomly selected month, based on small, medium or large database
 * Run on Linux to use large RAND_MAX (2e31-1)
 * To compile: gcc -o ds3_create_orders ds3_create_orders.c -lm
 * Last Updated 5/12/05
 * Last Updated 6/1/2010 by GSK
 * Last Updated 06/25/2010 by GSK (Newly created data will have latest orderdates from 2009 year)
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


#define MAX_CUST_ID_SMALL 20000
#define MAX_CUST_ID_MED   2000000
#define MAX_CUST_ID_LARGE 200000000

#define MAX_PROD_ID_SMALL 10000
#define MAX_PROD_ID_MED   100000
#define MAX_PROD_ID_LARGE 1000000


int main(int argc, char* argv[])
  {
  int i, j, n_first, n_last, orderid, i_month, i_month_in=0, i_day_of_month;
  int customerid, n_items_in_order, prod_id, quantity, orderlineid;
  int adder=0, max_prod_id, max_cust_id;
  double netamount, tax, totalamount;
  char* ind;
  char filename[20], fn_orders[35], fn_orderlines[35], fn_cust_hist[35], orderdate[10];
  char size, n_first_str[20], n_last_str[20];
  FILE *FP_orders, *FP_orderlines, *FP_cust_hist;
  time_t tptr;
  
  int i_days_in_month[] = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

  int i_Sys_Type = 0;	 //0 for Linux, 1 for Windows        //Added by GSK
  char str_Sys_Type[20]; //Added by GSK
  int i_Max_Prod_Id = 0; // This parameter = No of Rows in Product table //Added by GSK
  char str_Max_Prod_Id[50]; //Added by GSK
  int i_Max_Cust_Id = 0; // This parameter = No of Rows in Product table //Added by GSK
  char str_Max_Cust_Id[50]; //Added by GSK

  // Check syntax
  //if (argc < 5)
    if (argc < 8)    //Changed by GSK
    {
    fprintf(stderr, "Syntax: ds3_create_orders n_first n_last filename S|M|L <i_month> n_Sys_Type n_Max_Prod_Id n_Max_Cust_Id \n");     //Changed by GSK
    fprintf(stderr, "        where n_first, n_last can contain M or m for millions\n");
    fprintf(stderr, "        the M or m can be followed by a +n\n");
    fprintf(stderr, "Creates orders data files for DS2 database\n");
    fprintf(stderr, "Creates three files: <filename>_orders.csv, <filename>_orderlines.csv ",
      "and <filename>_cust_hist.csv\n");
    fprintf(stderr, "S,M,L uses product ids up to %d, %d, and %d, respectively\n", 
      MAX_PROD_ID_SMALL, MAX_PROD_ID_MED, MAX_PROD_ID_LARGE);
    fprintf(stderr, "       and customer ids up to %d, %d, and %d, respectively\n", 
      MAX_CUST_ID_SMALL, MAX_CUST_ID_MED, MAX_CUST_ID_LARGE);
    fprintf(stderr, "i_month is optional; if not specified will be generated randomly\n");
    fprintf(stderr, "n_Sys_Type can be 0 (Linux) or 1 (Windows) \n");                             //Added by GSK
    fprintf(stderr, "n_Max_Prod_Id can be max number of rows in Product table \n");		//Changed Syntax by GSK
    fprintf(stderr, "n_Max_Cust_Id can be max number of rows in Customer table \n");		//Changed Syntax by GSK
    fprintf(stderr, "Examples: ds3_create_orders  1 1000 filename S 1 0 10000 20000 =>  orders        1 -> 1000\n");
    fprintf(stderr, "          ds3_create_orders    1 1M filename L 1 0 1000000 200000000 =>  orders        1 -> 1000000\n");
    fprintf(stderr, "          ds3_create_orders 1M+1 2M filename L 1 0 1000000 200000000 =>  orders  1000001 -> 2000000\n");
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

  strcpy(filename, argv[3]);
  size = toupper(argv[4][0]);

  //if (argc > 5) i_month_in  = atoi(argv[5]);
  if (argc > 5)                   //Changed by GSK
  {
  	i_month_in  = atoi(argv[5]);   

	strcpy(str_Sys_Type,argv[6]);    //Added by GSK
	i_Sys_Type = atoi(str_Sys_Type); //Added by GSK

        strcpy(str_Max_Prod_Id,argv[7]); //Added by GSK
	i_Max_Prod_Id = atoi(str_Max_Prod_Id); //Added by GSK

        strcpy(str_Max_Cust_Id,argv[8]); //Added by GSK
	i_Max_Cust_Id = atoi(str_Max_Cust_Id); //Added by GSK
  }

  max_prod_id = i_Max_Prod_Id;	//Added by GSK
  max_cust_id = i_Max_Cust_Id;	//Added by GSK

  //Commented by GSK
  //From now on Parameter S|M|L will be useless but still will be passed, since randomizer function will calculate id's on the fly acc to input parameters of C Program
  /*
  switch (size)
    {
    case 'S':
      max_prod_id = MAX_PROD_ID_SMALL;
      max_cust_id = MAX_CUST_ID_SMALL;
      break;
    case 'M':
      max_prod_id = MAX_PROD_ID_MED;
      max_cust_id = MAX_CUST_ID_MED;
      break;
    case 'L':
      max_prod_id = MAX_PROD_ID_LARGE;
      max_cust_id = MAX_CUST_ID_LARGE;
      break;
    }
  */


  sprintf(fn_orders, "%s_orders.csv", filename);
  sprintf(fn_orderlines, "%s_orderlines.csv", filename);
  sprintf(fn_cust_hist, "%s_cust_hist.csv", filename);

  FP_orders = fopen(fn_orders, "wb");
  FP_orderlines = fopen(fn_orderlines, "wb");
  FP_cust_hist = fopen(fn_cust_hist, "wb");

  srand(n_first); //Seed rand() with n_first


  for (i=n_first; i <= n_last; i++) 
    {
    orderid = i;

    // orderdate
    if (i_month_in>0 && i_month<=12) i_month = i_month_in;
      else i_month = random2(1,12);
    i_day_of_month = random2(1, i_days_in_month[i_month-1]);
    //sprintf(orderdate,"%4d/%02d/%02d", 2004, i_month, i_day_of_month);
	//changed by GSK (All data newly created using this C program will have new dates)
	sprintf(orderdate,"%4d/%02d/%02d", 2013, i_month, i_day_of_month);

    // customerid
    customerid = random2(1, max_cust_id);

    // netamount, tax, totalamount
    netamount = 0.01 * random2(1, 40000);
    tax = 0.0825 * netamount;
    totalamount = netamount + tax;
    
    //fprintf(FP_orders, "%d,%s,%d,%.2f,%.2f,%.2f\n", orderid, orderdate, customerid, netamount, tax, totalamount);
    //Changed by GSK	
    if(i_Sys_Type == 0)   //If System is Linux, Append LF only     //Added by GSK
    {
	fprintf(FP_orders, "%d,%s,%d,%.2f,%.2f,%.2f%c", orderid, orderdate, customerid, netamount, tax, totalamount, 10);
    }
    else if(i_Sys_Type == 1)  //If System is Windows, Append CR and then LF	//Added by GSK
    {
	fprintf(FP_orders, "%d,%s,%d,%.2f,%.2f,%.2f%c%c", orderid, orderdate, customerid, netamount, tax, totalamount, 13, 10);
    }

    n_items_in_order = random2(1, 9);
    
    // Add Cart item, Randomize productID
    for (j=1; j <= n_items_in_order; j++)
      {
      orderlineid = j;

      // prod_id
      prod_id = random2(1, max_prod_id);

      // quantity
      quantity = random2(1, 3);

      //fprintf(FP_orderlines, "%d,%d,%d,%d,%s\n", orderlineid, orderid, prod_id, quantity, orderdate);
      //Changed by GSK	
      if(i_Sys_Type == 0)   //If System is Linux, Append LF only     //Added by GSK
      {
	  fprintf(FP_orderlines, "%d,%d,%d,%d,%s%c", orderlineid, orderid, prod_id, quantity, orderdate, 10);
      }
      else if(i_Sys_Type == 1)  //If System is Windows, Append CR and then LF	//Added by GSK
      {
	  fprintf(FP_orderlines, "%d,%d,%d,%d,%s%c%c", orderlineid, orderid, prod_id, quantity, orderdate, 13, 10);
      }	

      //fprintf(FP_cust_hist, "%d,%d,%d\n", customerid, orderid, prod_id);
      //Changed by GSK	
      if(i_Sys_Type == 0)   //If System is Linux, Append LF only     //Added by GSK
      {
	  fprintf(FP_cust_hist, "%d,%d,%d%c", customerid, orderid, prod_id, 10);
      }
      else if(i_Sys_Type == 1)  //If System is Windows, Append CR and then LF	//Added by GSK
      {
	  fprintf(FP_cust_hist, "%d,%d,%d%c%c", customerid, orderid, prod_id, 13, 10);
      }	
	

      } // end for on orderlines file
    } // end for on orders file
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
