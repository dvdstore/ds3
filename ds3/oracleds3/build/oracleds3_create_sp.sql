  
-- DS3 Stored Procedures Build Scripts
-- Dave Jaffe, Todd Muirhead and Deepak Janakiraman   Last modified 12/03/07
-- Copyright Dell Inc. 2007 


CREATE GLOBAL TEMPORARY TABLE derivedtable1 
  ON COMMIT PRESERVE ROWS
  AS SELECT PRODUCTS.TITLE, PRODUCTS.ACTOR, PRODUCTS.PROD_ID, PRODUCTS.COMMON_PROD_ID
  FROM DS3.CUST_HIST INNER JOIN
    DS3.PRODUCTS ON CUST_HIST.PROD_ID = PRODUCTS.PROD_ID;

  
CREATE OR REPLACE  PROCEDURE "DS3"."NEW_CUSTOMER" 
  (
  firstname_in DS3.CUSTOMERS.FIRSTNAME%TYPE,
  lastname_in DS3.CUSTOMERS.LASTNAME%TYPE,
  address1_in DS3.CUSTOMERS.ADDRESS1%TYPE,
  address2_in DS3.CUSTOMERS.ADDRESS2%TYPE,
  city_in DS3.CUSTOMERS.CITY%TYPE,
  state_in DS3.CUSTOMERS.STATE%TYPE,
  zip_in DS3.CUSTOMERS.ZIP%TYPE,
  country_in DS3.CUSTOMERS.COUNTRY%TYPE,
  region_in DS3.CUSTOMERS.REGION%TYPE,
  email_in DS3.CUSTOMERS.EMAIL%TYPE,
  phone_in DS3.CUSTOMERS.PHONE%TYPE,
  creditcardtype_in DS3.CUSTOMERS.CREDITCARDTYPE%TYPE,
  creditcard_in DS3.CUSTOMERS.CREDITCARD%TYPE,
  creditcardexpiration_in DS3.CUSTOMERS.CREDITCARDEXPIRATION%TYPE,
  username_in DS3.CUSTOMERS.USERNAME%TYPE,
  password_in DS3.CUSTOMERS.PASSWORD%TYPE,
  age_in DS3.CUSTOMERS.AGE%TYPE,
  income_in DS3.CUSTOMERS.INCOME%TYPE,
  gender_in DS3.CUSTOMERS.GENDER%TYPE,
  customerid_out OUT INTEGER
  )
  IS
  rows_returned INTEGER;
  BEGIN

    SELECT COUNT(*) INTO rows_returned FROM CUSTOMERS WHERE USERNAME = username_in;

    IF rows_returned = 0
    THEN
      SELECT CUSTOMERID_SEQ.NEXTVAL INTO customerid_out FROM DUAL;
      INSERT INTO CUSTOMERS
        (
        CUSTOMERID,
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
        customerid_out,
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
      COMMIT;

    ELSE customerid_out := 0;

    END IF;

    END NEW_CUSTOMER;
/

CREATE OR REPLACE  PROCEDURE "DS3"."NEW_MEMBER"
  (
  customerid_in INTEGER,
  membershiplevel_in INTEGER,
  customerid_out OUT INTEGER
  )
  IS
  rows_returned INTEGER;
  BEGIN

    SELECT COUNT(*) INTO rows_returned FROM MEMBERSHIP WHERE CUSTOMERID = customerid_in;

    IF rows_returned = 0
    THEN
      INSERT INTO MEMBERSHIP
        (CUSTOMERID,
         MEMBERSHIPTYPE,
         EXPIREDATE
         )
      VALUES
        (
        customerid_in,
        membershiplevel_in,
        SYSDATE
        );
      customerid_out := customerid_in;
    ELSE
      customerid_out := 0;
    END IF;
    END NEW_MEMBER;
/




CREATE OR REPLACE PROCEDURE "DS3"."NEW_PROD_REVIEW"
  (
  prod_id_in 		IN DS3.REVIEWS.PROD_ID%TYPE,
  stars_in 		IN DS3.REVIEWS.STARS%TYPE,
  customerid_in 	IN DS3.REVIEWS.CUSTOMERID%TYPE,
  review_summary_in 	IN DS3.REVIEWS.REVIEW_SUMMARY%TYPE,
  review_text_in 	IN DS3.REVIEWS.REVIEW_TEXT%TYPE,
  review_id_out 	OUT INTEGER
 )
  IS
  rows_returned INTEGER;
  BEGIN

      SELECT REVIEWID_SEQ.NEXTVAL INTO review_id_out FROM DUAL;
      INSERT INTO REVIEWS
        (
        REVIEW_ID,
        PROD_ID,
        REVIEW_DATE,
        STARS,
        CUSTOMERID,
        REVIEW_SUMMARY,
        REVIEW_TEXT
        )
        VALUES
        (
        review_id_out,
        prod_id_in,
	SYSDATE,
        stars_in,
        customerid_in,
        review_summary_in,
        review_text_in
        )
        ;
      COMMIT;
END NEW_PROD_REVIEW; 
/

CREATE OR REPLACE PROCEDURE "DS3"."NEW_REVIEW_HELPFULNESS"
  (
  review_id_in          	IN DS3.REVIEWS_HELPFULNESS.REVIEW_ID%TYPE,
  customerid_in         	IN DS3.REVIEWS_HELPFULNESS.CUSTOMERID%TYPE,
  review_helpfulness_in 	IN DS3.REVIEWS_HELPFULNESS.HELPFULNESS%TYPE,
  review_helpfulness_id_out     OUT INTEGER
 )
  IS
  rows_returned INTEGER;
  BEGIN

      SELECT REVIEWHELPFULNESSID_SEQ.NEXTVAL INTO review_helpfulness_id_out FROM DUAL;
      INSERT INTO REVIEWS_HELPFULNESS
        (
        REVIEW_HELPFULNESS_ID,
        REVIEW_ID,
        CUSTOMERID,
        HELPFULNESS
        )
        VALUES
        (
        review_helpfulness_id_out,
        review_id_in,
        customerid_in,
        review_helpfulness_in
        )
        ;
      COMMIT;
END NEW_REVIEW_HELPFULNESS;
/


CREATE OR REPLACE  PROCEDURE "DS3"."LOGIN" 
  (
  username_in        IN  DS3.CUSTOMERS.USERNAME%TYPE,
  password_in        IN  DS3.CUSTOMERS.PASSWORD%TYPE,
  batch_size         IN  INTEGER,
  found              OUT INTEGER,
  customerid_out     OUT INTEGER,
  title_out          OUT DS3_TYPES.ARRAY_TYPE,
  actor_out          OUT DS3_TYPES.ARRAY_TYPE,
  related_title_out  OUT DS3_TYPES.ARRAY_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;

  BEGIN
    
    SELECT CUSTOMERID INTO customerid_out FROM CUSTOMERS WHERE USERNAME = username_in AND PASSWORD = password_in;
    
    delete from derivedtable1;

    insert into derivedtable1 select products.title, products.actor, products.prod_id, products.common_prod_id
        from cust_hist inner join products on cust_hist.prod_id = products.prod_id
       where (cust_hist.customerid = customerid_out);
    OPEN result_cv FOR
      SELECT derivedtable1.TITLE, derivedtable1.ACTOR, PRODUCTS.TITLE AS RelatedTitle
        FROM
          derivedtable1 INNER JOIN
            PRODUCTS ON derivedtable1.COMMON_PROD_ID = PRODUCTS.PROD_ID;
    
    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO title_out(i), actor_out(i), related_title_out(i);
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;

  EXCEPTION
    WHEN NO_DATA_FOUND THEN
    customerid_out := 0;
  
  END LOGIN;
/


CREATE OR REPLACE PROCEDURE "DS3"."BROWSE_BY_CATEGORY" 
  (
  batch_size   IN INTEGER,
  found        OUT INTEGER,
  category_in  IN INTEGER,
  prod_id_out  OUT DS3_TYPES.N_TYPE,
  category_out OUT DS3_TYPES.N_TYPE,
  title_out    OUT DS3_TYPES.ARRAY_TYPE,
  actor_out    OUT DS3_TYPES.ARRAY_TYPE,
  price_out    OUT DS3_TYPES.N_TYPE,
  special_out  OUT DS3_TYPES.N_TYPE,
  common_prod_id_out  OUT DS3_TYPES.N_TYPE,
  membership_item_out OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;
  
  BEGIN
  
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CATEGORY = category_in AND SPECIAL = 1;
    END IF;
  
    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i), membership_item_out(i);
      IF result_cv%NOTFOUND THEN 
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_CATEGORY;
/  


CREATE OR REPLACE PROCEDURE "DS3"."BROWSE_BY_CAT_FOR_MEMBERTYPE"
  (
  batch_size   		IN INTEGER,
  found       		OUT INTEGER,
  category_in  		IN INTEGER,
  membershiptype_in 	IN INTEGER,
  prod_id_out  		OUT DS3_TYPES.N_TYPE,
  category_out 		OUT DS3_TYPES.N_TYPE,
  title_out    		OUT DS3_TYPES.ARRAY_TYPE,
  actor_out    		OUT DS3_TYPES.ARRAY_TYPE,
  price_out    		OUT DS3_TYPES.N_TYPE,
  special_out  		OUT DS3_TYPES.N_TYPE,
  common_prod_id_out  	OUT DS3_TYPES.N_TYPE,
  membership_item_out   OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;

  BEGIN

    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CATEGORY = category_in AND SPECIAL = 1 AND MEMBERSHIP_ITEM <= membershiptype_in;
    END IF;

    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i), membership_item_out(i);
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_CAT_FOR_MEMBERTYPE;
/



CREATE OR REPLACE PROCEDURE GET_PROD_REVIEWS
(
   batch_size                  IN INTEGER,
   found                       OUT INTEGER,
   prod_in                     IN  INTEGER,
   review_id_out               OUT DS3_TYPES.N_TYPE,
   prod_id_out                 OUT DS3_TYPES.N_TYPE,
   review_date_out             OUT DS3_TYPES.ARRAY_TYPE,
   review_stars_out            OUT DS3_TYPES.N_TYPE,
   review_customerid_out       OUT DS3_TYPES.N_TYPE,
   review_summary_out          OUT DS3_TYPES.ARRAY_TYPE,
   review_text_out             OUT DS3_TYPES.LONG_ARRAY_TYPE,
   review_helpfulness_sum_out  OUT DS3_TYPES.N_TYPE
  )
AS 
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;
BEGIN
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      WITH s1 AS (SELECT review_id, SUM(helpfulness) AS total FROM 
        (SELECT prod_id, review_id, stars, helpfulness  FROM 
        (SELECT reviews.prod_id, reviews.review_id, reviews_helpfulness.helpfulness, reviews.stars FROM
        reviews INNER JOIN reviews_helpfulness ON reviews.review_id=reviews_helpfulness.review_id WHERE 
        reviews.prod_id = prod_in))
        GROUP BY review_id ORDER BY sum(helpfulness) DESC)
      SELECT s1.review_id, reviews.prod_id, reviews.review_date, reviews.stars, reviews.customerid, reviews.review_summary, reviews.review_text, s1.total FROM
      s1 INNER JOIN reviews on reviews.review_id=s1.review_id;
    END IF;

    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO review_id_out(i), prod_id_out(i), review_date_out(i), review_stars_out(i), review_customerid_out(i), review_summary_out(i), review_text_out(i), review_helpfulness_sum_out(i);
      IF review_helpfulness_sum_out(i) IS NULL THEN
        review_helpfulness_sum_out(i) := 0;
      END IF;
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;

   EXCEPTION
      WHEN NO_DATA_FOUND THEN
        found := 0;

  END GET_PROD_REVIEWS;
/

  
CREATE OR REPLACE  PROCEDURE "DS3"."GET_PROD_REVIEWS_BY_STARS"
  (
   batch_size                  IN INTEGER,
   found                       OUT INTEGER,
   prod_in                     IN  INTEGER,
   stars_in                    IN  INTEGER,
   review_id_out               OUT DS3_TYPES.N_TYPE,
   prod_id_out                 OUT DS3_TYPES.N_TYPE,
   review_date_out             OUT DS3_TYPES.ARRAY_TYPE,
   review_stars_out            OUT DS3_TYPES.N_TYPE,
   review_customerid_out       OUT DS3_TYPES.N_TYPE,
   review_summary_out          OUT DS3_TYPES.ARRAY_TYPE,
   review_text_out             OUT DS3_TYPES.LONG_ARRAY_TYPE,
   review_helpfulness_sum_out  OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;

  BEGIN
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      WITH s1 AS (SELECT review_id, SUM(helpfulness) AS total FROM 
        (SELECT prod_id, review_id, stars, helpfulness  FROM 
        (SELECT reviews.prod_id, reviews.review_id, reviews_helpfulness.helpfulness, reviews.stars FROM
        reviews INNER JOIN reviews_helpfulness ON reviews.review_id=reviews_helpfulness.review_id WHERE 
        reviews.prod_id = prod_in)WHERE stars = stars_in)
        GROUP BY review_id ORDER BY sum(helpfulness) DESC)
      SELECT s1.review_id, reviews.prod_id, reviews.review_date, reviews.stars, reviews.customerid, reviews.review_summary, reviews.review_text, s1.total FROM
      s1 INNER JOIN reviews on reviews.review_id=s1.review_id;
    END IF;

    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO review_id_out(i), prod_id_out(i), review_date_out(i), review_stars_out(i), review_customerid_out(i), review_summary_out(i), review_text_out(i), review_helpfulness_sum_out(i);
       IF review_helpfulness_sum_out(i) IS NULL THEN
        review_helpfulness_sum_out(i) := 0;
      END IF;
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;

    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        found := 0;

  END GET_PROD_REVIEWS_BY_STARS;
/


CREATE OR REPLACE  PROCEDURE "DS3"."GET_PROD_REVIEWS_BY_DATE"
  (
   batch_size                  IN INTEGER,
   found                       OUT INTEGER,
   prod_in                     IN  INTEGER,
   review_id_out               OUT DS3_TYPES.N_TYPE,
   prod_id_out                 OUT DS3_TYPES.N_TYPE,
   review_date_out             OUT DS3_TYPES.ARRAY_TYPE,
   review_stars_out            OUT DS3_TYPES.N_TYPE,
   review_customerid_out       OUT DS3_TYPES.N_TYPE,
   review_summary_out          OUT DS3_TYPES.ARRAY_TYPE,
   review_text_out             OUT DS3_TYPES.LONG_ARRAY_TYPE,
   review_helpfulness_sum_out  OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;

  BEGIN
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM REVIEWS WHERE PROD_ID = prod_in ORDER BY REVIEW_DATE DESC;
    END IF;

    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO review_id_out(i), prod_id_out(i), review_date_out(i), review_stars_out(i), review_customerid_out(i), review_summary_out(i), review_text_out(i);
      SELECT SUM(helpfulness) INTO review_helpfulness_sum_out(i) from reviews_helpfulness where REVIEW_ID = review_id_out(i);
      IF review_helpfulness_sum_out(i) IS NULL THEN
        review_helpfulness_sum_out(i) := 0;
      END IF;
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;

    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        found := 0;

  END GET_PROD_REVIEWS_BY_DATE;
/


CREATE OR REPLACE  PROCEDURE "DS3"."GET_PROD_REVIEWS_BY_ACTOR"
  (
   batch_size                  IN INTEGER,
   found                       OUT INTEGER,
   actor_in                    IN  VARCHAR2,
   title_out		       OUT DS3_TYPES.ARRAY_TYPE,
   actor_out		       OUT DS3_TYPES.ARRAY_TYPE,
   review_id_out               OUT DS3_TYPES.N_TYPE,
   prod_id_out                 OUT DS3_TYPES.N_TYPE,
   review_date_out             OUT DS3_TYPES.ARRAY_TYPE,
   review_stars_out            OUT DS3_TYPES.N_TYPE,
   review_customerid_out       OUT DS3_TYPES.N_TYPE,
   review_summary_out          OUT DS3_TYPES.ARRAY_TYPE,
   review_text_out             OUT DS3_TYPES.LONG_ARRAY_TYPE,
   review_helpfulness_sum_out  OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;

  BEGIN

    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
	WITH T1 AS 
          (SELECT PRODUCTS.TITLE, PRODUCTS.ACTOR, PRODUCTS.PROD_ID, REVIEWS.REVIEW_DATE, REVIEWS.STARS, REVIEWS.REVIEW_ID,
           REVIEWS.CUSTOMERID, REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT 
           FROM PRODUCTS INNER JOIN REVIEWS on PRODUCTS.PROD_ID = REVIEWS.PROD_ID where CONTAINS (ACTOR, actor_in) > 0 AND ROWNUM<=500 )
         select T1.title, T1.actor, T1.REVIEW_ID, T1.prod_id, T1.review_date, T1.stars, 
                T1.customerid, T1.review_summary, T1.review_text, SUM(helpfulness) AS totalhelp from REVIEWS_HELPFULNESS 
         inner join T1 on REVIEWS_HELPFULNESS.REVIEW_ID = T1.review_id
	 GROUP BY T1.REVIEW_ID, T1.prod_id, t1.title, t1.actor, t1.review_date, t1.stars, t1.customerid, t1.review_summary, t1.review_text
	 ORDER BY totalhelp DESC;       
    END IF;

    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO title_out(i), actor_out(i),review_id_out(i), prod_id_out(i), review_date_out(i), review_stars_out(i), review_customerid_out(i), review_summary_out(i), review_text_out(i), review_helpfulness_sum_out(i);
       IF review_helpfulness_sum_out(i) IS NULL THEN
        review_helpfulness_sum_out(i) := 0;
      END IF;
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END GET_PROD_REVIEWS_BY_ACTOR;
/


CREATE OR REPLACE  PROCEDURE "DS3"."GET_PROD_REVIEWS_BY_TITLE"
  (
   batch_size                  IN INTEGER,
   found                       OUT INTEGER,
   title_in                    IN  VARCHAR2,
   title_out                   OUT DS3_TYPES.ARRAY_TYPE,
   actor_out                   OUT DS3_TYPES.ARRAY_TYPE,
   review_id_out               OUT DS3_TYPES.N_TYPE,
   prod_id_out                 OUT DS3_TYPES.N_TYPE,
   review_date_out             OUT DS3_TYPES.ARRAY_TYPE,
   review_stars_out            OUT DS3_TYPES.N_TYPE,
   review_customerid_out       OUT DS3_TYPES.N_TYPE,
   review_summary_out          OUT DS3_TYPES.ARRAY_TYPE,
   review_text_out             OUT DS3_TYPES.LONG_ARRAY_TYPE,
   review_helpfulness_sum_out  OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;

  BEGIN

    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
	WITH T1 AS
          (SELECT PRODUCTS.TITLE, PRODUCTS.ACTOR, PRODUCTS.PROD_ID, REVIEWS.REVIEW_DATE, REVIEWS.STARS, REVIEWS.REVIEW_ID,
           REVIEWS.CUSTOMERID, REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT
           FROM PRODUCTS INNER JOIN REVIEWS on PRODUCTS.PROD_ID = REVIEWS.PROD_ID where CONTAINS (TITLE, title_in) > 0 AND ROWNUM<=500 )
         select T1.title, T1.actor, T1.REVIEW_ID, T1.prod_id, T1.review_date, T1.stars,
                T1.customerid, T1.review_summary, T1.review_text, SUM(helpfulness) AS totalhelp from REVIEWS_HELPFULNESS
         inner join T1 on REVIEWS_HELPFULNESS.REVIEW_ID = T1.review_id
         GROUP BY T1.REVIEW_ID, T1.prod_id, t1.title, t1.actor, t1.review_date, t1.stars, t1.customerid, t1.review_summary, t1.review_text
         ORDER BY totalhelp DESC;
    END IF;

    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO title_out(i), actor_out(i),review_id_out(i), prod_id_out(i), review_date_out(i), review_stars_out(i), review_customerid_out(i), review_summary_out(i), review_text_out(i), review_helpfulness_sum_out(i);
      IF review_helpfulness_sum_out(i) IS NULL THEN
        review_helpfulness_sum_out(i) := 0;
      END IF;
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END GET_PROD_REVIEWS_BY_TITLE;
/




CREATE OR REPLACE  PROCEDURE "DS3"."BROWSE_BY_ACTOR"
  (
  batch_size   IN INTEGER,
  found        OUT INTEGER,
  actor_in     IN  VARCHAR2,
  prod_id_out  OUT DS3_TYPES.N_TYPE,
  category_out OUT DS3_TYPES.N_TYPE,
  title_out    OUT DS3_TYPES.ARRAY_TYPE,
  actor_out    OUT DS3_TYPES.ARRAY_TYPE,
  price_out    OUT DS3_TYPES.N_TYPE,
  special_out  OUT DS3_TYPES.N_TYPE,
  common_prod_id_out  OUT DS3_TYPES.N_TYPE,
  membership_item_out OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;
  
  BEGIN
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CONTAINS(ACTOR, actor_in) > 0;
    END IF;
  
    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i), membership_item_out(i);
      IF result_cv%NOTFOUND THEN 
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_ACTOR;
/


CREATE OR REPLACE  PROCEDURE "DS3"."BROWSE_BY_ACTOR_FOR_MEMBERTYPE"
  (
  batch_size   		IN INTEGER,
  found        		OUT INTEGER,
  actor_in     		IN  VARCHAR2,
  membershiptype_in  	IN INTEGER,
  prod_id_out  		OUT DS3_TYPES.N_TYPE,
  category_out 		OUT DS3_TYPES.N_TYPE,
  title_out    		OUT DS3_TYPES.ARRAY_TYPE,
  actor_out    		OUT DS3_TYPES.ARRAY_TYPE,
  price_out    		OUT DS3_TYPES.N_TYPE,
  special_out  		OUT DS3_TYPES.N_TYPE,
  common_prod_id_out  	OUT DS3_TYPES.N_TYPE,
  membership_item_out   OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;

  BEGIN
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CONTAINS(ACTOR, actor_in) > 0 AND MEMBERSHIP_ITEM <= membershiptype_in;
    END IF;

    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i), membership_item_out(i);
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_ACTOR_FOR_MEMBERTYPE;
/

  
  
CREATE OR REPLACE  PROCEDURE "DS3"."BROWSE_BY_TITLE"
  (
  batch_size   IN  INTEGER,
  found        OUT INTEGER,
  title_in     IN  VARCHAR2,
  prod_id_out  OUT DS3_TYPES.N_TYPE,
  category_out OUT DS3_TYPES.N_TYPE,
  title_out    OUT DS3_TYPES.ARRAY_TYPE,
  actor_out    OUT DS3_TYPES.ARRAY_TYPE,
  price_out    OUT DS3_TYPES.N_TYPE,
  special_out  OUT DS3_TYPES.N_TYPE,
  common_prod_id_out  OUT DS3_TYPES.N_TYPE,
  membership_item_out OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;
  
  BEGIN
  
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CONTAINS(TITLE, title_in) > 0;
    END IF;
  
    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i), membership_item_out(i);
      IF result_cv%NOTFOUND THEN 
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_TITLE;
/
 

CREATE OR REPLACE  PROCEDURE "DS3"."BROWSE_BY_TITLE_FOR_MEMBERTYPE"
  (
  batch_size            IN INTEGER,
  found                 OUT INTEGER,
  title_in              IN VARCHAR2,
  membershiptype_in     IN INTEGER,
  prod_id_out           OUT DS3_TYPES.N_TYPE,
  category_out          OUT DS3_TYPES.N_TYPE,
  title_out             OUT DS3_TYPES.ARRAY_TYPE,
  actor_out             OUT DS3_TYPES.ARRAY_TYPE,
  price_out             OUT DS3_TYPES.N_TYPE,
  special_out           OUT DS3_TYPES.N_TYPE,
  common_prod_id_out    OUT DS3_TYPES.N_TYPE,
  membership_item_out   OUT DS3_TYPES.N_TYPE
  )
  AS
  result_cv DS3_TYPES.DS3_CURSOR;
  i INTEGER;

  BEGIN
    IF NOT result_cv%ISOPEN THEN
      OPEN result_cv FOR
      SELECT * FROM PRODUCTS WHERE CONTAINS(TITLE, title_in) > 0 AND MEMBERSHIP_ITEM <= membershiptype_in;
    END IF;

    found := 0;
    FOR i IN 1..batch_size LOOP
      FETCH result_cv INTO prod_id_out(i), category_out(i), title_out(i), actor_out(i), price_out(i), special_out(i), common_prod_id_out(i), membership_item_out(i);
      IF result_cv%NOTFOUND THEN
        CLOSE result_cv;
        EXIT;
      ELSE
        found := found + 1;
      END IF;
    END LOOP;
  END BROWSE_BY_TITLE_FOR_MEMBERTYPE;
/


 
  
CREATE OR REPLACE  PROCEDURE "DS3"."PURCHASE"
  (
  customerid_in   IN INTEGER,
  number_items    IN INTEGER,
  netamount_in    IN NUMBER,
  taxamount_in    IN NUMBER,
  totalamount_in  IN NUMBER,
  neworderid_out  OUT INTEGER,
  prod_id_in      IN DS3_TYPES.N_TYPE,
  qty_in          IN DS3_TYPES.N_TYPE
  )
  AS
  date_in        DATE;
  item_id        INTEGER;
  price          NUMBER;
  cur_quan       NUMBER;
  new_quan       NUMBER;
  cur_sales      NUMBER;
  new_sales      NUMBER;
  prod_id_temp   DS3_TYPES.N_TYPE;

  BEGIN

    SELECT ORDERID_SEQ.NEXTVAL INTO neworderid_out FROM DUAL;

    date_in := SYSDATE;
--  date_in := TO_DATE('2005/1/1', 'YYYY/MM/DD');

    COMMIT;

  -- Start Transaction
    SET TRANSACTION NAME 'FillOrder';

  

  -- CREATE NEW ENTRY IN ORDERS TABLE
    INSERT INTO ORDERS
      (
      ORDERID,
      ORDERDATE,
      CUSTOMERID,
      NETAMOUNT,
      TAX,
      TOTALAMOUNT
      )
    VALUES
      (
      neworderid_out,
      date_in,
      customerid_in,
      netamount_in,
      taxamount_in,
      totalamount_in
      )
      ;

    -- ADD LINE ITEMS TO ORDERLINES

    FOR item_id IN 1..number_items LOOP
      INSERT INTO ORDERLINES
        (
        ORDERLINEID,
        ORDERID,
        PROD_ID,
        QUANTITY,
        ORDERDATE
        )
      VALUES
        (
        item_id,
        neworderid_out,
        prod_id_in(item_id),
        qty_in(item_id),
        date_in
        )
        ;
   -- Check and update quantity in stock
      SELECT QUAN_IN_STOCK, SALES into cur_quan, cur_sales FROM INVENTORY WHERE PROD_ID=prod_id_in(item_id);
      new_quan := cur_quan - qty_in(item_id);
      new_sales := cur_sales + qty_in(item_id);
      IF new_quan < 0 THEN
        ROLLBACK;
        neworderid_out := 0;
        RETURN;
      ELSE
        IF new_quan < 3 THEN  -- this is kluge to keep rollback rate constant - assumes 1, 2 or 3 quan ordered
          UPDATE INVENTORY SET SALES= new_sales WHERE PROD_ID=prod_id_in(item_id);
        ELSE
          UPDATE INVENTORY SET QUAN_IN_STOCK = new_quan, SALES= new_sales WHERE PROD_ID=prod_id_in(item_id);
        END IF;
        INSERT INTO CUST_HIST
          (
          CUSTOMERID,
          ORDERID,
          PROD_ID
          )
        VALUES
          (
          customerid_in,
          neworderid_out,
          prod_id_in(item_id)
          );
      END IF;
    END LOOP;

    COMMIT;

  END PURCHASE;
/

CREATE OR REPLACE TRIGGER "DS3"."RESTOCK" 
AFTER UPDATE OF "QUAN_IN_STOCK" ON "DS3"."INVENTORY" 
FOR EACH ROW WHEN (NEW.QUAN_IN_STOCK < 10) 

DECLARE
  X NUMBER;
BEGIN 
  SELECT COUNT(*) INTO X FROM DS3.REORDER WHERE PROD_ID = :NEW.PROD_ID;
  IF x = 0 THEN
    INSERT INTO DS3.REORDER(PROD_ID, DATE_LOW, QUAN_LOW) VALUES(:NEW.PROD_ID, SYSDATE, :NEW.QUAN_IN_STOCK);
  END IF;
END RESTOCK;
/


exit;
