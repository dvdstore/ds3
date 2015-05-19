ds3_create_prod_readme.txt


The data creation programs (ds3_create_prod.c, etc.) work best when compiled and run on Linux (or a Linux-like Windows 
environment such as Cygwin) due to the larger RAND_MAX. The Windows binaries (ds3_create_prod.exe, etc.) provided in the
kit will run as is but will not provide a good degree of randomness to the data.

DVDStore 2.1 and later allows to create any custom size database. 

User must use perl scripts in DVDStore 3 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 3,
please go through document /ds3/ds3_Documentation.txt

DVDStore 3 will provide all compiled linux and windows executables for data generation C programs.

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds3/ds3_Documentation.txt under prerequisites section)

-------------------------------------------------------------------------------------------------------------

Instructions for creating and loading DVD Store Version 3 (DS3) Oracle database product data

Small (10  MB) database: follow instructions below for 10,000 titles    (n_prods = 10000)
Med   (1   GB) database: follow instructions below for 100,000 titles   (n_prods = 100000)
Large (100 GB) database: follow instructions below for 1,000,000 titles (n_prods = 1000000)

In ../orders, after creating orderlines files,
run ds3_create_inv to total up sales by product and create inventory load file:
ds3_create_inv n_prods n_Sys_Type > ../prod/inv.csv

Then in this directory:
ds3_create_prod n_prods n_Sys_Type > prod.csv


<davejaffe7@dell.com> and <tmuirhead@vmware.com>  5/15/15
