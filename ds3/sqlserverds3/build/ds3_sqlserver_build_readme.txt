

ds3_sqlserver_build_readme.txt

DVDStore 3 allows to create any custom size database. 

User must use perl scripts in DVDStore 3 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 3,
please go through document /ds3/ds3_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds3/ds3_Documentation.txt under prerequisites section)

-------------------------------------------------------------------------------------------------------------------------------------


Instructions for building and loading the SQL Server implementation of the DVD Store Version 3 (DS3) database

DS3 can be created in any size, but supports three historical standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

The ./ds3/sqlserverds3/build directory contains SQL Server scripts to create the DS3
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

Instructions for building the small (10MB) DS SQL Server database 
(assumes create files and data under c: and SQL Server files under c:\sql\dbfiles)

On SQL Server machine:

 1) Install SQL Server (be sure full-text search is enabled)
 2) untar ds3.tar.gz to c:
 3) untar ds3_sqlserver.tar.gz to c:
 4) Create directory c:\sql\dbfiles 

 5) in c:\ds3\sqlserverds3\build:       osql -Usa -P -i sqlserverds3_create_db_small.sql
 6) in c:\ds3\sqlserverds3\load\cust:   osql -Usa -P -i sqlserverds3_load_cust.sql
 7) in c:\ds3\sqlserverds3\load\orders: osql -Usa -P -i sqlserverds3_load_orders.sql
 8) in c:\ds3\sqlserverds3\load\orders: osql -Usa -P -i sqlserverds3_load_orderlines.sql
 9) in c:\ds3\sqlserverds3\load\orders: osql -Usa -P -i sqlserverds3_load_cust_hist.sql
10) in c:\ds3\sqlserverds3\load\prod:   osql -Usa -P -i sqlserverds3_load_prod.sql
11) in c:\ds3\sqlserverds3\load\prod:   osql -Usa -P -i sqlserverds3_load_inv.sql
12) in c:\ds3\sqlserverds3\load\membership: osql -Usa -P -i sqlserverds3_load_members.sql
13) in c:\ds3\sqlserverds3\load\reviews:osql -Usa -P -i sqlserverds3_load_reviews.sql
14) in c:\ds3\sqlserverds3\build:       osql -Usa -P -i sqlserverds3_create_ind.sql
15) in c:\ds3\sqlserverds3\build:       osql -Usa -P -i sqlserverds3_create_sp.sql
16) in c:\ds3\sqlserverds3\build:       osql -Usa -P -i sqlserverds3_create_user.sql


14) to run statistics:
SQL Server 2000:
C:\Program Files\Microsoft SQL Server\MSSQL\Binn\sqlmaint.exe -U sa -P -S localhost -D DS3 -UpdOptiStats 18
SQL Server 2005:
C:\Program Files\Microsoft SQL Server\MSSQL.1\MSSQL\Binn\sqlmaint.exe -U sa -P -S localhost -D DS3 -UpdOptiStats 18

Steps 5 - 16 can be done in one call:
in c:\ds3\sqlserverds3: osql -Usa -P -i sqlserverds3_create_all.sql
   

To build large database you will need to create data files (preferably in
Linux due to larger RAND_MAX) using scripts in ./ds3/data_files, modify the
load programs to point to these files, and modify sqlserverds3_create_db_large.sql 
to point to where you want the SQL Server files to reside

Note: you can run osql from client machine with SQL Server Client Tools installed
using either hostname or IP address:
osql -Usa -P -S hostname -i sds_create_db.sql
  or
osql -Usa -P -S IPaddress -i sds_create_db.sql
but notice that directories referenced in the called sql scripts will refer to the 
directory structure of the target machine

Most of the directories contain readme's with further instructions

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  1/5/16

