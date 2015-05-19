Instructions for building and driving DVD Store Version 3 (DS3)
database

The DVD Store Version 3 (DS3) is a complete online e-commerce test
application, with a backend database component, a web application
layer, and driver programs.  The goal in designing the database
component as well as the midtier application was to utilize many
advanced database features (transactions, stored procedures, triggers,
referential integity) while keeping the database easy to install and
understand. The DS3 workload may be used to test databases or as a
stress tool for any purpose.

The distribution will include code for SQL Server, Oracle, MySQL, and PostGres.
Included in the release are data generation programs, shell scripts to 
build data for 10MB, 1GB and 100 GB versions of the DVD Store, a perl
script to help generate any custom size database, database 
build scripts and stored procedure, PHP web pages, and a C# driver program.

The DS3 files are separated into database-independent data load files
under ./ds3/data_files and driver programs under ./ds3/drivers
and database-specific build scripts, loader programs, and driver
programs in directories
./ds3/mysqlds3
./ds3/oracleds3
./ds3/sqlserverds3
./ds3/pgsqlds3

file ds3.tar.gz contains ./ds3/data_files and ./ds3/drivers
file ds3_mysql.tar.gz contains ./ds3/mysqlds3
file ds3_oracle.tar.gz contains ./ds3/oracleds3
file ds3_sqlserver.tar.gz contains ./ds3/sqlserverds3

To install:

In the directory in which you want to install ds3:
tar -xvzf ds3.tar.gz

and then install the implementation(s) of interest:
tar -xvzf ds3_mysql.tar.gz
tar -xvzf ds3_oracle.tar.gz
tar -xvzf ds3_sqlserver.tar.gz
tar -xvfz ds3_pgsql.tar.gz

The loader programs use relative addressing to reference the data
files. They will need to be changed if the data files are placed
elsewhere.

DS3 comes in 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

ds3.tar.gz contains data files for the Small version.

Most of the directories contain readme's with further instructions

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  5/14/15
