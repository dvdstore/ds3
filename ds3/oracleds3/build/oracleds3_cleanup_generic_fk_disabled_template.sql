
ALTER TABLE "DS3"."ORDERS" DISABLE CONSTRAINT "FK_CUSTOMERID";

--Changed by GSK to consider cleaning new dates after 2009 (dates are now partitioned from 2009-2011)
select count(*) as "New Orderlines" from DS3.ORDERLINES where TO_DATE('2009-12-31', 'yyyy-mm-dd') < ORDERDATE;
delete from DS3.ORDERLINES where TO_DATE('2009-12-31', 'yyyy-mm-dd') < ORDERDATE;

--Changed by GSK to consider cleaning new dates after 2009 (dates are now partitioned from 2009-2011)
select count(*) as "New Orders" from DS3.ORDERS where TO_DATE('2009-12-31', 'yyyy-mm-dd') < ORDERDATE;
delete from DS3.ORDERS where TO_DATE('2009-12-31', 'yyyy-mm-dd') < ORDERDATE;

select count(*) as "New Customers" from DS3.CUSTOMERS where customerid > {CUST_ROW};
delete from DS3.CUSTOMERS where customerid > {CUST_ROW};


select count(*) as "Reorder table size" from DS3.REORDER;
delete from DS3.REORDER;

delete from DS3.INVENTORY;

-- Sequences


DROP SEQUENCE "DS3"."CUSTOMERID_SEQ";
CREATE SEQUENCE "DS3"."CUSTOMERID_SEQ" 
  INCREMENT BY 1 
  START WITH {CUST_ROW_PLUS_ONE}
  MAXVALUE 1.0E28 
  MINVALUE 1 
  NOCYCLE 
  CACHE 1000000     --Change needed for this parameter??? This might impact performance
  NOORDER
  ;

DROP SEQUENCE "DS3"."ORDERID_SEQ";
CREATE SEQUENCE "DS3"."ORDERID_SEQ" 
  INCREMENT BY 1 
  START WITH {CUST_ROW_PLUS_ONE} 
  MAXVALUE 1.0E28 
  MINVALUE 1 
  NOCYCLE 
  CACHE 1000000    --Change needed for this parameter??? This might impact performance
  NOORDER
  ;

DROP SEQUENCE "DS3"."REVIEWID_SEQ";
CREATE SEQUENCE "DS3"."REVIEWID_SEQ"
  INCREMENT BY 1
  START WITH {REVIEWID_PLUS_ONE)
  MAXVALUE 1.0E28
  MINVALUE 1
  NOCYCLE
  CACHE 100000
  NOORDER
  ;

DROP SEQUENCE "DS3"."REVIEWHELPFULNESSID_SEQ";
CREATE SEQUENCE "DS3"."REVIEWHELPFULNESSID_SEQ"
  INCREMENT BY 1
  START WITH {REVIEWHELPFULNESSID_PLUS_ONE}
  MAXVALUE 1.0E28
  MINVALUE 1
  NOCYCLE
  CACHE 100000
  NOORDER
  ;

commit;

ALTER TABLE "DS3"."ORDERS" ENABLE CONSTRAINT "FK_CUSTOMERID";

exit;
