ds3_mysql_readme.txt

DVDStore 3 (and the earlier version 2.1)  allows to create any custom size database. 

User must use perl script InstallDVDStore.pl in the ds3/ directory to create database of any size. To know more 
about how to use perl scripts and general instructions on DVDStore 3,
please go through document /ds3/ds3_Documentation.txt

In order to run the perl scripts on a windows system a perl utility of some sort is required. (Instructions for installing perl utility over windows
is included in document /ds3/ds3_Documentation.txt under prerequisites section)

--------------------------------------------------------------------------------------------------------------------

Instructions for building and loading the MySQL implementation of the DVD Store Version 3 (DS3) database

DS2 comes in 3 standard sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

Directories
-----------
./ds3/mysqlds3
./ds3/mysqlds3/build
./ds3/mysqlds3/load
./ds3/mysqlds3/load/cust
./ds3/mysqlds3/load/orders
./ds3/mysqlds3/load/prod
./ds3/mysqlds3/load/reviews
./ds3/mysqlds3/load/membership
./ds3/mysqlds3/web
./ds3/mysqlds3/web/php

The ./ds3/mysqlds3 directory contains two driver programs:
ds3mysqldriver.exe      (ftp in binary to a Windows machine)
ds3mysqldriver_mono.exe (run under Mono on Linux)
To see the syntax run program with no arguments on a command line.
To compile use ds3mysqlfns.cs with ./ds3/drivers/ds3xdriver.cs (see
that file's header)

The ./ds3/mysqlds3/build directory contains MySQL scripts to create the DS3
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The ./ds3/mysqlds3/load directories contain MySQL load scripts to load the data
from the datafiles under ./ds3/data_files. You will need to modify the scripts
if the data is elsewhere.
 
The ./ds3/mysqlds3/web directories contain a PHP application to drive DS3

The build and load of the Small DS3 database may be accomplished with the
shell script, mysqlds3_create_all.sh, in ./ds3/mysqlds3:

On MySQL machine:

Add user web with default home directory (/home/web); set password
    - As root: useradd web; passwd web
  - Fix permissions on /home directories
    - As root: chmod 755 /home/web;

1) Install MySQL
2) untar ds3.tar.gz from linux.dell.com/dvdstore
3) untar ds3_mysql.tar.gz to the same place
4) cd ./ds3/mysqlds3
5) sh mysqlds3_create_all.sh

# mysqlds3_create_all.sh
# start in ./ds3/mysqlds3
cd build
mysql -u web --password=web < mysqlds3_create_db.sql
mysql -u web --password=web < mysqlds3_create_ind.sql
mysql -u web --password=web < mysqlds3_create_sp.sql
cd ../load/cust
mysql -u web --password=web < mysqlds3_load_cust.sql
cd ../orders
mysql -u web --password=web < mysqlds3_load_orders.sql 
mysql -u web --password=web < mysqlds3_load_orderlines.sql 
mysql -u web --password=web < mysqlds3_load_cust_hist.sql 
cd ../prod
mysql -u web --password=web < mysqlds3_load_prod.sql 
mysql -u web --password=web < mysqlds3_load_inv.sql 
cd ../membership
mysql -u web --password=web < mysqlds3_load_member.sql
cd ../reviews
mysql -u web --password=web < mysqlds3_load_reviews.sql
mysql -u web --password=web < mysqlds3_load_review_helpfulness.sql

my.cnf.example is the MySQL configuration file used in our testing (copy to /etc/my.cnf)
my.cnf.example.diff shows the differences between my.cnf.example and /usr/share/mysql/my-large.cnf

monitor_load.txt describes how to monitor the load of InnoDB tables using showinnodb.sql

Most of the directories contain readme's with further instructions

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  6/2/15
