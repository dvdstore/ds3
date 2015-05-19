ds3_data_files_readme.txt

The data creation programs (ds3_create_cust.c, etc.) work best when compiled and run on Linux (or a Linux-like Windows 
environment such as Cygwin) due to the larger RAND_MAX. The Windows binaries (ds3_create_cust.exe, etc.) provided in the
kit will run as is but will not provide a good degree of randomness to the data.

DVDStore 3 allows to create any custom size database. 

User must use perl scripts in DVDStore 3 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 3,
please go through document /ds3/ds3_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. 
(Instructions for installing perl utility over windows
is included in document /ds3/ds3_Documentation.txt under prerequisites section)

---------------------------------------------------------------------------------------


Instructions for building DVD Store Version 3 (DS3) database data files

DS3 has 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

But can also be created in any size using the InstallDVDStore.pl perl script.

The directories below contain programs and associated shell scripts to build
the data files used by all three sizes. Additionally, the Small data files are
included in the directories. Scripts to load the data into a specific database
are found in that database's directory, eg. ./ds3/mysql.

Be sure to ftp, copy or unzip the data files with the appropriate settings to
include the CR/LF line delimiter in DOS files but not Linux files. 


Directory structure:

./ds3/data_files
./ds3/data_files/cust    Customer table
./ds3/data_files/orders  Orders, Orderlines and Cust_Hist table
./ds3/data_files/prod    Products, Inv table


Please see the readme's in some of the other directories.



<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  5/15/15
