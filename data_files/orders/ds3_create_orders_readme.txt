ds3_create_orders_readme.txt


The data creation programs (ds3_create_orders.c, etc.) work best when compiled and run on Linux (or a Linux-like Windows 
environment such as Cygwin) due to the larger RAND_MAX. The Windows binaries (ds3_create_orders.exe, etc.) provided in the
kit will run as is but will not provide a good degree of randomness to the data.

DVDStore 2.1 and later allows to create any custom size database. 

User must use perl scripts in DVDStore 2.1 and later to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 2.1,
please go through document /ds2/ds2.1_Documentation.txt

DVDStore 2.1 and later will provide all compiled linux and windows executables for data generation C programs.

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds2/ds2.1_Documentation.txt under prerequisites section)

-------------------------------------------------------------------------------------------------------------

Instructions for creating and loading DVD Store Version 3 (DS3) database orders data
(for ORDERS, ORDERLINES and CUST_HIST tables)

  compile ds3_create_orders.c (see compilation directions in program)
  sh ds3_create_orders_small.sh (or medium or large) 

<dave_jaffe@dell.com> and <tmuirhead@vmware.com>  10/16/14
