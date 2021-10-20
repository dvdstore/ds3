REM pgsqlds3_create_all.bat
REM start in ./ds3/pgsqlds3
set CONNSTR=-h 10.159.209.237 -p 5432
set DBNAME=ds3
set SYSDBA=ds3
set PGPASSWORD=ds3
REM createlang plpgsql ds3
cd build
REM Assumes DB and SYSDBA are already created
psql %CONNSTR% -U postgres -d postgres < pgsqlds3_create_db.sql
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_delete_all.sql
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_create_tbl.sql
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_create_sp.sql
cd ../load/cust
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_cust.sql
cd ../orders
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_orders.sql 
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_orderlines.sql 
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_cust_hist.sql 
cd ../prod
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_prod.sql 
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_inv.sql 
cd ../membership
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_membership.sql
cd ../reviews
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_reviews.sql 
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_load_review_helpfulness.sql 
cd ../../build
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_create_ind.sql
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_create_trig.sql
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_reset_seq.sql
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% < pgsqlds3_create_user.sql
psql %CONNSTR% -U %SYSDBA% -d %DBNAME% -c "VACUUM ANALYZE;"
cd ..


