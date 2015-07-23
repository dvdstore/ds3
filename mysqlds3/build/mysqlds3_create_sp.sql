-- mysqlds3_create_sp.sql: DVD Store Database Version 3 Create Stored Procedures Script - MySQL version
-- <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
-- Last updated 5/27/15

Delimiter $
DROP PROCEDURE IF EXISTS DS3.NEW_CUSTOMER $
CREATE PROCEDURE DS3.NEW_CUSTOMER ( IN firstname_in varchar(50), IN lastname_in varchar(50), IN address1_in varchar(50), IN address2_in varchar(50), IN city_in varchar(50), IN state_in varchar(50), IN zip_in int, IN country_in varchar(50), IN region_in int, IN email_in varchar(50), IN phone_in varchar(50), IN creditcardtype_in int, IN creditcard_in varchar(50), IN creditcardexpiration_in varchar(50), IN username_in varchar(50), IN password_in varchar(50), IN age_in int, IN income_in int, IN gender_in varchar(1), OUT customerid_out INT)
  BEGIN
  DECLARE rows_returned INT;
  SELECT COUNT(*) INTO rows_returned FROM CUSTOMERS WHERE USERNAME = username_in;
  IF rows_returned = 0 
  THEN
    INSERT INTO CUSTOMERS
      (
      FIRSTNAME,
      LASTNAME,
      EMAIL,
      PHONE,
      USERNAME,
      PASSWORD,
      ADDRESS1,
      ADDRESS2,
      CITY,
      STATE,
      ZIP,
      COUNTRY,
      REGION,
      CREDITCARDTYPE,
      CREDITCARD,
      CREDITCARDEXPIRATION,
      AGE,
      INCOME,
      GENDER
      )
    VALUES
      (
      firstname_in,
      lastname_in,
      email_in,
      phone_in,
      username_in,
      password_in,
      address1_in,
      address2_in,
      city_in,
      state_in,
      zip_in,
      country_in,
      region_in,
      creditcardtype_in,
      creditcard_in,
      creditcardexpiration_in,
      age_in,
      income_in,
      gender_in
      )
      ;
    select last_insert_id() into customerid_out;
  ELSE SET customerid_out = 0;
  END IF;
  END; $


DROP PROCEDURE IF EXISTS DS3.NEW_MEMBER $
CREATE PROCEDURE DS3.NEW_MEMBER ( IN customerid_in int, IN membershiplevel_in int, OUT customerid_out int)
BEGIN
  DECLARE rows_returned INT;
  SELECT COUNT(*) INTO rows_returned FROM MEMBERSHIP WHERE CUSTOMERID = customerid_in;
  IF rows_returned = 0
  THEN
    INSERT INTO MEMBERSHIP
      (
      CUSTOMERID,
      MEMBERSHIPTYPE,
      EXPIREDATE
      )
      VALUES
      (
      customerid_in,
      membershiplevel_in,
      SYSDATE()
      )
      ;
    SET customerid_out = customerid_in;
  ELSE
    SET customerid_out = 0;
  END IF;
  END; $ 

DROP PROCEDURE IF EXISTS DS3.NEW_PROD_REVIEW $
CREATE PROCEDURE DS3.NEW_PROD_REVIEW
  (
  IN  prod_id_in            int,
  IN  stars_in              int,
  IN  customerid_in         int,
  IN  review_summary_in     VARCHAR(50),
  IN  review_text_in        VARCHAR(1000),
  OUT review_id_out         int
 )
BEGIN
  DECLARE rows_retunred int;
    INSERT INTO REVIEWS
      (
      PROD_ID,
      REVIEW_DATE,
      STARS,
      CUSTOMERID,
      REVIEW_SUMMARY,
      REVIEW_TEXT
      )
      VALUES
      (
      prod_id_in,
      SYSDATE(),
      stars_in,
      customerid_in,
      review_summary_in,
      review_text_in
      )
      ;
    COMMIT;
    select last_insert_id() into review_id_out;
END; $

DROP PROCEDURE IF EXISTS DS3.NEW_REVIEW_HELPFULNESS $
CREATE PROCEDURE DS3.NEW_REVIEW_HELPFULNESS
  (
  IN  review_id_in         	int,
  IN  customerid_in         	int,
  IN  review_helpfulness_in 	int,
  OUT review_helpfulness_id_out int
 )
BEGIN
  DECLARE rows_retunred int;
    INSERT INTO REVIEWS_HELPFULNESS
      (
      REVIEW_ID,
      CUSTOMERID,
      HELPFULNESS
      )
      VALUES
      (
      review_id_in,
      customerid_in,
      review_helpfulness_in
      )
      ;
    COMMIT;
    select last_insert_id() into review_helpfulness_id_out;
END; $

 
