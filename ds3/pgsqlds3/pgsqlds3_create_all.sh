# pgsqlds3_create_all.sh
# Start in ./ds3/pgsqlds3
CONNSTR="-h 10.159.209.237 -p 5432" 
DBNAME=ds3
SYSDBA=ds3
export PGPASSWORD=ds3

# createlang plpgsql ds2
cd build
# Assumes DB and SYSDBA are already created
# If building on vFabric Data Director vPostgres then you will need to comment out
#     pgsqlds2_create_db.sql line becuase the DB is already created

psql $CONNSTR -U postgres -d postgres < pgsqlds3_create_db.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_delete_all.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_create_tbl.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_create_sp.sql
cd ../load/cust
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_cust.sql
cd ../orders
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_orders.sql 
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_orderlines.sql 
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_cust_hist.sql 
cd ../prod
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_prod.sql 
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_inv.sql 
cd ../membership
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_membership.sql
cd ../reviews
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_reviews.sql 
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_load_review_helpfulness.sql 
cd ../../build
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_create_ind.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_create_trig.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_reset_seq.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME < pgsqlds3_create_user.sql
psql $CONNSTR -U $SYSDBA -d $DBNAME -c "VACUUM ANALYZE;"
