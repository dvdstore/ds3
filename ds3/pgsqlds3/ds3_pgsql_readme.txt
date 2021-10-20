ds3_pgsql_readme.txt


DVDStore 3 allows you to create any custom size database. 

User can use perl scripts in DVDStore 3 to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 3,
please go through document /ds3/ds3_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds3/ds3_Documentation.txt under prerequisites section)

--------------------------------------------------------------------------------------------------------------------

Instructions for building and loading the PostgreSQL implementation of the DVD Store Version 3 (DS3) database

DS3 can be created in any size by has 3 "standard" sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

Directories
-----------
./ds3/pgsqlds3
./ds3/pgsqlds3/dotnet
./ds3/pgsqlds3/build
./ds3/pgsqlds3/load
./ds3/pgsqlds3/load/cust
./ds3/pgsqlds3/load/orders
./ds3/pgsqlds3/load/prod
./ds3/pgsqlds3/web
./ds3/pgsqlds3/web/jsp
./ds3/pgsqlds3/web/php

The ./ds3/pgsqlds3 directory contains the windows x64 based driver program:
ds3pgsqldriver.exe      (ftp in binary to a Windows machine)

To see the syntax run program with no arguments on a command line.
To re-compile use Microsoft .net 5.x dotnet publish command with ds3pgsqlfns.cs with and ./ds3/drivers/ds3xdriver.cs
(see ./ds3/pgsqlds3/dotnet for more details)

The ./ds3/pgsqlds3/build directory contains PostgreSQL scripts to create the DS3
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The ./ds3/pgsqlds3/load directories contain PostgreSQL load scripts to load the data
from the datafiles under ./ds3/data_files. You will need to modify the scripts
if the data is elsewhere.
 
The ./ds3/pgsqlds3/web directories contain PHP and JSP applications to drive DS3

The build and load of the Small DS2 database may be accomplished with the
shell script, pgsqlds3_create_all.sh, in ./ds3/pgsqlds3:

On PostgreSQL machine:

Install PostgresSQL 13.x or later, for more details please reference postgres-setup.txt.

By default the data_files that are included with the ds3 download are for the small DVDStore database.  
To create custom size database you can also use the Install_DVDStore.pl script in the ds3/ directory to create 
the load script and data files for any size database. When you run Install_DVDStore.pl it will ask you a series of
questions that allow you to specify the size and database type.  It then will produce the needed sripts and data files.

postgresql.conf.example is the PostgreSQL configuration file used in our testing (append to $PGDATA/postgresql.conf)

In order to enable remote systems to be able to access the PostgreSQL database it is necessary to make two changes.
These two changes will enable completely open access, if you need more a restrictive policy these settings would be different:
1) In postgres.conf - listen_Addresses = '*'
2) In pg_hba.conf add a line:
host	all	all	0.0.0.0/0	trust

Driver Program
--------------

The ds3pgsqldriver.exe is the Postgresql implementation of the DS3 driver. It is 
based on ds3pgsqlfns.cs (in this directory) and ds3xdriver.cs (in ds3/drivers). It is a C# .Net program that uses the 
Microsoft .Net runtime which can be run on most platforms.  ds3pgsqldriver.exe is binary built for windows x86_64. 
Other platorms can be built.  Please see the readme in the /ds3/ds3pgslq/dotnet diretory.  This direct driver that 
generates load against the database that simulates users logging on, browsing, and purchasing items from the 
DVDStore website. 

The included windows binary is standalone and can be run without anything additional. 

If you want to build for another platform like Linux or MacOS, you will need to install .net 5.0 and then do the
following from the /ds3/pgsqlds3/dotnet directory:
dotnet new console
dotnet add package Npgsql --version 5.0.4
dotnet run
(or dotnet publish to build a binary - see notes in pgsqlds3/dotnet directory for full command details)

You can run ds3pgsqldriver.exe --help to get a full listing of parameters, a description of each, and their default values.

In addition to this direct driver there is ds3webdriver (located the ds3/drivers) that can be used to drive load though
the PHP or JSP version of the DVDStore webtier that have been implemented to use the Postgresql version of the DVDStore
datbase.  Please see the pgsqlds2/web directory for more info on these webtier versions.

<tmuirhead@vmware.com>  5/13/21
