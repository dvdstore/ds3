
-- pgsqlds3_create_db.sql: DVD Store Database Version 3.0 Build Script - Postgres version
-- Copyright (C) 2011 Vmware, Inc. 
-- Last updated 3/31/21


-- Reset Sequences after load

SELECT setval('categories_category_seq',max(category)) FROM categories;
SELECT setval('customers_customerid_seq',max(customerid)) FROM customers;
SELECT setval('orders_orderid_seq',max(orderid)) FROM orders;
SELECT setval('products_prod_id_seq',max(prod_id)) FROM products;
SELECT setval('reviews_review_id_seq',max(review_id)) from reviews;
SELECT setval('reviews_helpfulness_review_helpfulness_id_seq',max(review_helpfulness_id)) from reviews_helpfulness;


