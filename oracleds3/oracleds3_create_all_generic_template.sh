#!/bin/sh
cd ./build
sqlplus "/ as sysdba" @{TBLSPACE_SQLFNAME}
sqlplus "/ as sysdba" @{CREATEDB_SQLFNAME}
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

