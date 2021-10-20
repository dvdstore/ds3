
alter table CUSTOMERS DISABLE TRIGGER ALL;
alter table ORDERS DISABLE TRIGGER ALL;
alter table ORDERLINES DISABLE TRIGGER ALL;
alter table CUST_HIST DISABLE TRIGGER ALL;
alter table INVENTORY DISABLE TRIGGER ALL;
alter table PRODUCTS DISABLE TRIGGER ALL;
alter table REVIEWS DISABLE TRIGGER ALL;
alter table REVIEWS_HELPFULNESS DISABLE TRIGGER ALL;
alter table MEMBERSHIP DISABLE TRIGGER ALL;

drop table IF EXISTS  INVENTORY, CUSTOMERS, ORDERS, ORDERLINES, CUST_HIST cascade;
drop table IF EXISTS PRODUCTS, CATEGORIES, REORDER cascade;
drop table IF EXISTS REVIEWS, REVIEWS_HELPFULNESS, MEMBERSHIP cascade;

DROP TRIGGER RESTOCK ON INVENTORY;
DROP OWNED BY web CASCADE;
DROP USER web;

