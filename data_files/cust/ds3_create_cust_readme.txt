ds3_create_cust_readme.txt

The data creation programs (ds3_create_cust.c, etc.) work best when compiled and run on Linux (or a Linux-like Windows 
environment such as Cygwin) due to the larger RAND_MAX. The Windows binaries (ds3_create_cust.exe, etc.) provided in the
kit will run as is but will not provide a good degree of randomness to the data.

DVDStore 2.1 and later allows for any custom size database to be created. 

User must use perl scripts in DVDStore 2.1 and later to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 3,
please go through document /ds3/ds3_Documentation.txt

DVDStore 3 will provide all compiled linux and windows executables for data generation C programs.

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds3/ds3_Documentation.txt under prerequisites section)

-------------------------------------------------------------------------------------------------------------
Instructions for creating DVD Store Version 3 (DS3) database customer data
(for CUSTOMERS table)

  compile ds3_create_cust.c (see compilation directions in program)
  sh ds3_create_cust_small.sh (or medium or large) 

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  5/15/15
