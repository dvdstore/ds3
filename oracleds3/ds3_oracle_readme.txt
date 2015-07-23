ds3_oracle_readme.txt

DVDStore 3 allows to create any custom size database. 

User must use perl scripts in DVDStore 3 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 3,
please go through document /ds3/ds3.1_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. 
(Instructions for installing perl utility over windowsis included in document 
/ds3/ds3.1_Documentation.txt under prerequisites section)

-------------------------------------------------------------------------------------------------------------------------------------

Instructions for building and loading the Oracle implementation of the DVD Store Version 2 (DS2) database

DS3 has 3 "standard" sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

Directories
-----------
./ds3/oracleds3
./ds3/oracleds3/build
./ds3/oracleds3/load
./ds3/oracleds3/load/cust
./ds3/oracleds3/load/orders
./ds3/oracleds3/load/prod
./ds3/oracleds3/load/membership
./ds3/oracleds3/load/reviews
./ds3/oracleds3/web
./ds3/oracleds3/web/jsp

The ./ds3/oracleds3/build directory contains Oracle scripts to create the DS3
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The ./ds3/oracleds3/load directories contain Oracle load scripts to load the data
from the datafiles under ./ds3/data_files. You will need to modify the scripts
if the data is elsewhere.
 
The ./ds3/oracleds3/web/jsp directory contains a Java Server Pages application to 
drive DS3.

The build and load of the Small DS3 database may be accomplished with the
shell script, oracleds3_create_all.sh, in ./ds3/oracleds3. For details see 
build/ds3_oracle_build_readme.txt
                                                                            
In order to run the sh scripts on a windows system a sh utility of some sort 
is required. 

A C# .NET driver program is available:
ds3oracledriver.exe      (ftp in binary to a Windows machine)
To see the syntax run program with no arguments on a command line.
To compile use ds3oraclefns.cs with ./ds3/drivers/ds3xdriver.cs (see that file's header)

ds3oracledriver.exe is now compiled with the 64b Oracle 11g Oracle Data Provider for .NET
		
Most of the directories contain readme's with further instructions

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  5/15/15
