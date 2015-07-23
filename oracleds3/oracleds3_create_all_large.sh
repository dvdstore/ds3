# oracleds3_create_all_large.sh
# start in ./ds3/oracleds3
# mount large datafiles over load/* directories or modify this script
cd ./build
sqlplus "/ as sysdba" @oracleds3_create_tablespaces_large_asm.sql
sqlplus "/ as sysdba" @oracleds3_create_db_large.sql
cd ../load/cust
sh oracleds3_cust_sqlldr.sh
cd ../orders
sh oracleds3_orders_sqlldr.sh
sh oracleds3_orderlines_sqlldr.sh
sh oracleds3_cust_hist_sqlldr.sh
cd ../prod
sh oracleds3_prod_sqlldr.sh
sh oracleds3_inv_sqlldr.sh
cd ../../build
sqlplus ds3/ds3 @oracleds3_create_seq.sql
sqlplus ds3/ds3 @oracleds3_create_ind.sql
sqlplus ds3/ds3 @oracleds3_create_fulltextindex.sql
sqlplus ds3/ds3 @oracleds3_create_sp.sql

