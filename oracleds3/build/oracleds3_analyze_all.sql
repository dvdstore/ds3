-- run from commmand line (sqlplus ds3/ds3@db @oracleds3_analyze_all), then enter / in sqlplus to execute
declare
begin
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'CATEGORIES', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_CATEGORIES', partname=> NULL );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'PRODUCTS', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_PROD_ID', partname=> NULL );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'INVENTORY', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_INV_PROD_ID', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_ACTOR_TEXT', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_TITLE_TEXT', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_PROD_CATEGORY', partname=> NULL );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_PROD_SPECIAL', partname=> NULL );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'CUSTOMERS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_CUSTOMERS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'CUST_HIST', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_CUST_HIST', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_CUST_USERNAME', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'ORDERS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_ORDERS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'ORDERLINES', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_ORDERLINES', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'REVIEWS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'REVIEWS_HELPFULNESS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_table_stats(ownname=> 'DS3', tabname=> 'MEMBERSHIP', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_REVIEWS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_REVIEWS_HELPFULNESS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'PK_MEMBERSHIP', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_REVIEWS_HELP_CUSTID', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_REVIEWS_HELP_REVID', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_REVIEWS_PROD_ID', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_REVIEWS_PRODSTARS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_REVIEWS_STARS', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_PROD_MEMBERSHIP', partname=> NULL , estimate_percent=> 18 );
dbms_stats.gather_index_stats(ownname=> 'DS3', indname=> 'IX_REORDER_PRODID', partname=> NULL , estimate_percent=> 18 );
end;
.
