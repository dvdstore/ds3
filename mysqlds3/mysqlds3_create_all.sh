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
