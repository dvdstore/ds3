
ds3_oracle_build_readme.txt

Instructions for building and loading the Oracle implementation of the DVD Store Version 3 (DS3) database

DS3 has 3 "standard" sizes:

Database    Size     Customers             Orders   Products
Small      10 MB        20,000        1,000/month     10,000
Medium      1 GB     2,000,000      100,000/month    100,000
Large     100 GB   200,000,000   10,000,000/month  1,000,000

Any custom size can be created using the InstallDVDStore.pl script in /ds3

The ./ds3/oracleds3/build directory contains Oracle scripts to create the DS3
schema, indexes and stored procedures, as well as scripts to restore the
database to its initial state after a run.

The scripts in this directory use data partitions and thus require Oracle
Enterprise Edition. Standard edition files are included in the standard
subdirectory.

The build and load of the Small DS3 database may be accomplished with the
shell script, oracleds3_create_all.sh, in ./ds3/oracleds3:

1) Install Oracle
2) untar ds3.tar.gz from linux.dell.com/dvdstore
3) untar ds3_oracle.tar.gz to the same place
For linux make sure the oracle files are owned by your Oracle installer user/group (by default oracle/oinstall)
4) Modify ./ds/oracleds3/build/oracleds3_create_tablespaces_small.sql to
point to directory where Oracle datafiles go. Samples are included for both
Windows (directory c:\oracledbfiles) and Linux (/oracledbfiles)
Change the password in the connect statement to your sys password
5) In directory ./ds3/oracleds3: sh oracleds3_create_all.sh

# oracleds3_create_all.sh
# start in ./ds3/oracleds3
cd build
sqlplus "/ as sysdba" @oracleds3_create_tablespaces_small.sql
sqlplus "/ as sysdba" @oracleds3_create_db_small.sql
cd ../load/cust
sh oracleds3_cust_sqlldr.sh
cd ../orders
sh oracleds3_orders_sqlldr.sh
sh oracleds3_orderlines_sqlldr.sh
sh oracleds3_cust_hist_sqlldr.sh
cd ../prod
sh oracleds3_prod_sqlldr.sh
sh oracleds3_inv_sqlldr.sh
cd ../reviews
sh oracleds3_reviews_sqlldr.sh
cd ../membership
sh oracleds3_membership_sqlldr.sh
cd ../../build
sqlplus ds3/ds3 @oracleds3_create_seq.sql
sqlplus ds3/ds3 @oracleds3_create_ind.sql
sqlplus ds3/ds3 @oracleds3_create_fulltextindex.sql
sqlplus ds3/ds3 @oracleds3_create_sp.sql

6) An analyze script is needed to have Oracle analyze and gather stats on DS3 to  
get the best performance. Run oracleds3_analyze_all.sql to analyze all tables and indexes:
run sqlplus ds3/ds3@db @oracleds3_analyze_all from command line, then enter /
in sqlplus to execute the script. 


In order to run the sh scripts on a windows system a sh utility of some sort is required. 

To build Medium or Large database you will need to create data files (preferably in
Linux due to larger RAND_MAX) using scripts in ./ds3/data_files, modify the
load programs to point to these files (if necessary), and either modify
oracleds3_create_all.sh or run the appropriate scripts manually. The oracleds3_create_all_large.sh 
will automate the creation of the large database.

oracleds3_create_tablespaces_large_asm.sql is a sample script using Oracle Automated Storage Management (ASM).

oracleds3_cleanup_small.sh (and medium and large versions) will restore the
database to its original condition. The INVENTORY table is completely reloaded
so the scripts will need to be modified if the data directory is not in the
default location. Also, it has been found to be much quicker to clean the
Large database with one foreign key disabled. For this reason
oracleds3_cleanup_large.sh points to oracleds3_cleanup_large_fk_disabled.sql.

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  5/15/15
