
-- sqlserverds3_create_sp.sql: DS3 Stored Procedures Build Scripts - SQL Server version
-- Copyright (C) 2005 Dell, Inc. <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
-- Last updated 1/4/16


-- NEW_CUSTOMER

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'NEW_CUSTOMER' AND type = 'P')
  DROP PROCEDURE NEW_CUSTOMER
GO

USE DS3
GO

CREATE PROCEDURE NEW_CUSTOMER
  (
  @firstname_in             VARCHAR(50),
  @lastname_in              VARCHAR(50),
  @address1_in              VARCHAR(50),
  @address2_in              VARCHAR(50),
  @city_in                  VARCHAR(50),
  @state_in                 VARCHAR(50),
  @zip_in                   INT,
  @country_in               VARCHAR(50),
  @region_in                TINYINT,
  @email_in                 VARCHAR(50),
  @phone_in                 VARCHAR(50),
  @creditcardtype_in        TINYINT,
  @creditcard_in            VARCHAR(50),
  @creditcardexpiration_in  VARCHAR(50),
  @username_in              VARCHAR(50),
  @password_in              VARCHAR(50),
  @age_in                   TINYINT,
  @income_in                INT,
  @gender_in                VARCHAR(1)
  )

  AS 

  IF (SELECT COUNT(*) FROM CUSTOMERS WHERE USERNAME=@username_in) = 0
  BEGIN
    INSERT INTO CUSTOMERS 
      (
      FIRSTNAME,
      LASTNAME,
      ADDRESS1,
      ADDRESS2,
      CITY,
      STATE,
      ZIP,
      COUNTRY,
      REGION,
      EMAIL,
      PHONE,
      CREDITCARDTYPE,
      CREDITCARD,
      CREDITCARDEXPIRATION,
      USERNAME,
      PASSWORD,
      AGE,
      INCOME,
      GENDER
      ) 
    VALUES 
      ( 
      @firstname_in,
      @lastname_in,
      @address1_in,
      @address2_in,
      @city_in,
      @state_in,
      @zip_in,
      @country_in,
      @region_in,
      @email_in,
      @phone_in,
      @creditcardtype_in,
      @creditcard_in,
      @creditcardexpiration_in,
      @username_in,
      @password_in,
      @age_in,
      @income_in,
      @gender_in
      )
    SELECT @@IDENTITY
  END
  ELSE 
    SELECT 0
GO

-- NEW_MEMBER

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'NEW_MEMBER' AND type = 'P')
  DROP PROCEDURE NEW_MEMBER
GO

USE DS3
GO

CREATE PROCEDURE NEW_MEMBER
  (
  @customerid_in            INT,
  @membershiplevel_in       INT
  )

  AS 

  DECLARE
  @date_in                  DATETIME

  SET DATEFORMAT ymd

  SET @date_in = GETDATE()

  IF (SELECT COUNT(*) FROM MEMBERSHIP WHERE CUSTOMERID=@customerid_in) = 0
  BEGIN
    INSERT INTO MEMBERSHIP
      (
      CUSTOMERID,
      MEMBERSHIPTYPE,
      EXPIREDATE
      ) 
    VALUES 
      ( 
      @customerid_in,
      @membershiplevel_in,
      @date_in
      )
    SELECT @customerid_in
  END
  ELSE 
    SELECT 0
GO

-- NEW_PROD_REVIEW

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'NEW_PROD_REVIEW' AND type = 'P')
  DROP PROCEDURE NEW_PROD_REVIEW
GO

USE DS3
GO

CREATE PROCEDURE NEW_PROD_REVIEW
  (
  @prod_id_in            INT,
  @stars_in			     INT,
  @customerid_in		 INT,
  @review_summary_in	 VARCHAR(50),
  @review_text_in		 VARCHAR(1000)
  )

  AS 

  DECLARE
  @date_in                  DATETIME

  SET DATEFORMAT ymd

  SET @date_in = GETDATE()

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
      @prod_id_in,
      @date_in,
      @stars_in,
	  @customerid_in,
	  @review_summary_in,
	  @review_text_in
      )
    SELECT @@IDENTITY
 GO


-- New review helpfulness rating

 USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'NEW_REVIEW_HELPFULNESS' AND type = 'P')
  DROP PROCEDURE NEW_REVIEW_HELPFULNESS
GO

USE DS3
GO

CREATE PROCEDURE NEW_REVIEW_HELPFULNESS
  (
  @review_id_in            INT,
  @customerid_in			     INT,
  @review_helpfulness_in		 INT
  )

  AS 

  INSERT INTO REVIEWS_HELPFULNESS
      (
      REVIEW_ID,
      CUSTOMERID,
	  HELPFULNESS
	  ) 
    VALUES 
      ( 
      @review_id_in,
   	  @customerid_in,
	  @review_helpfulness_in
      )
    SELECT @@IDENTITY
 GO


-- LOGIN

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'LOGIN' AND type = 'P')
  DROP PROCEDURE LOGIN
GO

USE DS3
GO

CREATE PROCEDURE LOGIN
  (
  @username_in              VARCHAR(50),
  @password_in              VARCHAR(50)
  )

  AS
DECLARE @customerid_out INT
  
  SELECT @customerid_out=CUSTOMERID FROM CUSTOMERS WHERE USERNAME=@username_in AND PASSWORD=@password_in

  IF (@@ROWCOUNT > 0)
    BEGIN
      SELECT @customerid_out
      SELECT derivedtable1.TITLE, derivedtable1.ACTOR, PRODUCTS_1.TITLE AS RelatedPurchase
        FROM (SELECT PRODUCTS.TITLE, PRODUCTS.ACTOR, PRODUCTS.PROD_ID, PRODUCTS.COMMON_PROD_ID
          FROM CUST_HIST INNER JOIN
             PRODUCTS ON CUST_HIST.PROD_ID = PRODUCTS.PROD_ID
          WHERE (CUST_HIST.CUSTOMERID = @customerid_out)) AS derivedtable1 INNER JOIN
             PRODUCTS AS PRODUCTS_1 ON derivedtable1.COMMON_PROD_ID = PRODUCTS_1.PROD_ID
    END
  ELSE 
    SELECT 0 
GO

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'BROWSE_BY_CATEGORY' AND type = 'P')
  DROP PROCEDURE BROWSE_BY_CATEGORY
GO

USE DS3
GO

CREATE PROCEDURE BROWSE_BY_CATEGORY
  (
  @batch_size_in            INT,
  @category_in              INT
  )

  AS 
  SET ROWCOUNT @batch_size_in
  SELECT * FROM PRODUCTS WHERE CATEGORY=@category_in and SPECIAL=1
  RETURN @@ROWCOUNT
GO

-- Browse by category for membertype

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'BROWSE_BY_CATEGORY_FOR_MEMBERTYPE' AND type = 'P')
  DROP PROCEDURE BROWSE_BY_CATEGORY_FOR_MEMBERTYPE
GO

USE DS3
GO

CREATE PROCEDURE BROWSE_BY_CATEGORY_FOR_MEMBERTYPE
  (
  @batch_size_in            INT,
  @category_in              INT,
  @membershiptype_in	    INT
  )

  AS 
  SET ROWCOUNT @batch_size_in
  SELECT * FROM PRODUCTS WHERE CATEGORY=@category_in and SPECIAL=1 and MEMBERSHIP_ITEM<=@membershiptype_in
  SET ROWCOUNT 0
GO

-- get prod reviews

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'GET_PROD_REVIEWS' AND type = 'P')
  DROP PROCEDURE GET_PROD_REVIEWS
GO

USE DS3
GO

CREATE PROCEDURE GET_PROD_REVIEWS
  (
  @batch_size_in            INT,
  @prod_in              INT
  )

  AS 
  SET ROWCOUNT @batch_size_in

  SELECT REVIEWS.REVIEW_ID, REVIEWS.PROD_ID, REVIEWS.REVIEW_DATE, REVIEWS.STARS, REVIEWS.CUSTOMERID, 
  REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT, SUM(REVIEWS_HELPFULNESS.helpfulness) as total
  FROM REVIEWS 
  INNER JOIN REVIEWS_HELPFULNESS on REVIEWS.REVIEW_ID=REVIEWS_HELPFULNESS.REVIEW_ID
  WHERE REVIEWS.PROD_ID = @prod_in GROUP BY REVIEWS.REVIEW_ID, REVIEWS.PROD_ID, REVIEWS.REVIEW_DATE, 
  REVIEWS.STARS, REVIEWS.CUSTOMERID, REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT
  ORDER BY total DESC
  SET ROWCOUNT 0
GO

-- get prod reviews by stars

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'GET_PROD_REVIEWS_BY_STARS' AND type = 'P')
  DROP PROCEDURE GET_PROD_REVIEWS_BY_STARS
GO

USE DS3
GO

CREATE PROCEDURE GET_PROD_REVIEWS_BY_STARS
  (
  @batch_size_in            INT,
  @prod_in					INT,
  @stars_in					INT
  )

  AS 
  SET ROWCOUNT @batch_size_in

  SELECT REVIEWS.REVIEW_ID, REVIEWS.PROD_ID, REVIEWS.REVIEW_DATE, REVIEWS.STARS, REVIEWS.CUSTOMERID, 
  REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT, SUM(REVIEWS_HELPFULNESS.helpfulness) as total
  FROM REVIEWS 
  INNER JOIN REVIEWS_HELPFULNESS on REVIEWS.REVIEW_ID=REVIEWS_HELPFULNESS.REVIEW_ID
  WHERE REVIEWS.PROD_ID = @prod_in AND REVIEWS.STARS = @stars_in GROUP BY REVIEWS.REVIEW_ID, REVIEWS.PROD_ID, REVIEWS.REVIEW_DATE, 
  REVIEWS.STARS, REVIEWS.CUSTOMERID, REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT
  ORDER BY total DESC
  SET ROWCOUNT 0
GO

-- get prod reviews by date

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'GET_PROD_REVIEWS_BY_DATE' AND type = 'P')
  DROP PROCEDURE GET_PROD_REVIEWS_BY_DATE
GO

USE DS3
GO

CREATE PROCEDURE GET_PROD_REVIEWS_BY_DATE
  (
  @batch_size_in            INT,
  @prod_in					INT
  )

  AS 
  SET ROWCOUNT @batch_size_in

  SELECT REVIEWS.REVIEW_ID, REVIEWS.PROD_ID, REVIEWS.REVIEW_DATE, REVIEWS.STARS, REVIEWS.CUSTOMERID, 
  REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT, SUM(REVIEWS_HELPFULNESS.helpfulness) as total
  FROM REVIEWS 
  INNER JOIN REVIEWS_HELPFULNESS on REVIEWS.REVIEW_ID=REVIEWS_HELPFULNESS.REVIEW_ID
  WHERE REVIEWS.PROD_ID = @prod_in GROUP BY REVIEWS.REVIEW_ID, REVIEWS.PROD_ID, REVIEWS.REVIEW_DATE, 
  REVIEWS.STARS, REVIEWS.CUSTOMERID, REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT
  ORDER BY REVIEWS.REVIEW_DATE DESC
  SET ROWCOUNT 0
GO

-- get prod reviews by actor

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'GET_PROD_REVIEWS_BY_ACTOR' AND type = 'P')
  DROP PROCEDURE GET_PROD_REVIEWS_BY_ACTOR
GO

USE DS3
GO

CREATE PROCEDURE GET_PROD_REVIEWS_BY_ACTOR
  (
  @batch_size_in            INT,
  @actor_in					VARCHAR(50)
  )

  AS 
  SET ROWCOUNT @batch_size_in;

  WITH T1 (title, actor, prod_id, review_date, stars, review_id, customerid, review_summary, review_text) 
AS (SELECT TOP (500) PRODUCTS.TITLE, PRODUCTS.ACTOR, PRODUCTS.PROD_ID, REVIEWS.REVIEW_DATE, REVIEWS.STARS, REVIEWS.REVIEW_ID,
           REVIEWS.CUSTOMERID, REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT 
    FROM PRODUCTS INNER JOIN REVIEWS on PRODUCTS.PROD_ID = REVIEWS.PROD_ID where CONTAINS (ACTOR, @actor_in))
select T1.prod_id, T1.title, T1.actor, REVIEWS_HELPFULNESS.REVIEW_ID, T1.review_date, T1.stars, 
                    T1.customerid, T1.review_summary, T1.review_text, SUM(helpfulness) AS totalhelp from REVIEWS_HELPFULNESS 
                    inner join T1 on REVIEWS_HELPFULNESS.REVIEW_ID = T1.review_id
					GROUP BY T1.REVIEW_ID, T1.prod_id, t1.title, t1.actor, REVIEWS_HELPFULNESS.REVIEW_ID, t1.review_date, t1.stars, t1.customerid, t1.review_summary, t1.review_text
					ORDER BY totalhelp DESC;
  SET ROWCOUNT 0
GO

-- get prod reviews by title

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'GET_PROD_REVIEWS_BY_TITLE' AND type = 'P')
  DROP PROCEDURE GET_PROD_REVIEWS_BY_TITLE
GO

USE DS3
GO

CREATE PROCEDURE GET_PROD_REVIEWS_BY_TITLE
  (
  @batch_size_in            INT,
  @title_in					VARCHAR(50)
  )

  AS 
  SET ROWCOUNT @batch_size_in;

  WITH T1 (title, actor, prod_id, review_date, stars, review_id, customerid, review_summary, review_text) 
AS (SELECT TOP (500) PRODUCTS.TITLE, PRODUCTS.ACTOR, PRODUCTS.PROD_ID, REVIEWS.REVIEW_DATE, REVIEWS.STARS, REVIEWS.REVIEW_ID,
           REVIEWS.CUSTOMERID, REVIEWS.REVIEW_SUMMARY, REVIEWS.REVIEW_TEXT 
    FROM PRODUCTS INNER JOIN REVIEWS on PRODUCTS.PROD_ID = REVIEWS.PROD_ID where CONTAINS (TITLE, @title_in))
select T1.prod_id, T1.title, T1.actor, REVIEWS_HELPFULNESS.REVIEW_ID, T1.review_date, T1.stars, 
                    T1.customerid, T1.review_summary, T1.review_text, SUM(helpfulness) AS totalhelp from REVIEWS_HELPFULNESS 
                    inner join T1 on REVIEWS_HELPFULNESS.REVIEW_ID = T1.review_id
					GROUP BY T1.REVIEW_ID, T1.prod_id, t1.title, t1.actor, REVIEWS_HELPFULNESS.REVIEW_ID, t1.review_date, t1.stars, t1.customerid, t1.review_summary, t1.review_text
					ORDER BY totalhelp DESC;
  SET ROWCOUNT 0
GO




-- Browse by Actor

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'BROWSE_BY_ACTOR' AND type = 'P')
  DROP PROCEDURE BROWSE_BY_ACTOR
GO

USE DS3
GO

CREATE PROCEDURE BROWSE_BY_ACTOR
  (
  @batch_size_in            INT,
  @actor_in                 VARCHAR(50)
  )

  AS 

  SET ROWCOUNT @batch_size_in
  SELECT * FROM PRODUCTS WITH(FORCESEEK) WHERE CONTAINS(ACTOR, @actor_in)
  SET ROWCOUNT 0
GO

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'BROWSE_BY_TITLE' AND type = 'P')
  DROP PROCEDURE BROWSE_BY_TITLE
GO

USE DS3
GO

CREATE PROCEDURE BROWSE_BY_TITLE
  (
  @batch_size_in            INT,
  @title_in                 VARCHAR(50)
  )

  AS 

  SET ROWCOUNT @batch_size_in
  SELECT * FROM PRODUCTS WITH(FORCESEEK) WHERE CONTAINS(TITLE, @title_in)
  SET ROWCOUNT 0
GO

USE DS3
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'PURCHASE' AND type = 'P')
  DROP PROCEDURE PURCHASE
GO

USE DS3
GO

CREATE PROCEDURE PURCHASE
  (
  @customerid_in            INT,
  @number_items             INT,
  @netamount_in             MONEY,
  @taxamount_in             MONEY,
  @totalamount_in           MONEY,
  @prod_id_in0              INT = 0,     @qty_in0     INT = 0,
  @prod_id_in1              INT = 0,     @qty_in1     INT = 0,
  @prod_id_in2              INT = 0,     @qty_in2     INT = 0,
  @prod_id_in3              INT = 0,     @qty_in3     INT = 0,
  @prod_id_in4              INT = 0,     @qty_in4     INT = 0,
  @prod_id_in5              INT = 0,     @qty_in5     INT = 0,
  @prod_id_in6              INT = 0,     @qty_in6     INT = 0,
  @prod_id_in7              INT = 0,     @qty_in7     INT = 0,
  @prod_id_in8              INT = 0,     @qty_in8     INT = 0,
  @prod_id_in9              INT = 0,     @qty_in9     INT = 0
  )

  AS 

  DECLARE
  @date_in                  DATETIME,
  @neworderid               INT,
  @item_id                  INT,
  @prod_id                  INT,
  @qty                      INT,
  @cur_quan		    INT,
  @new_quan		    INT,
  @cur_sales                INT,
  @new_sales                INT
  

  SET DATEFORMAT ymd

  SET @date_in = GETDATE()
--SET @date_in = '2005/10/31'

  BEGIN TRANSACTION
  -- CREATE NEW ENTRY IN ORDERS TABLE
  INSERT INTO ORDERS
    (
    ORDERDATE,
    CUSTOMERID,
    NETAMOUNT,
    TAX,
    TOTALAMOUNT
    )
  VALUES
    (
    @date_in,
    @customerid_in,
    @netamount_in,
    @taxamount_in,
    @totalamount_in
    )

  SET @neworderid = @@IDENTITY


  -- ADD LINE ITEMS TO ORDERLINES

  SET @item_id = 0

  WHILE (@item_id < @number_items)
  BEGIN
    SELECT @prod_id = CASE @item_id WHEN 0 THEN @prod_id_in0
	                                WHEN 1 THEN @prod_id_in1
	                                WHEN 2 THEN @prod_id_in2
	                                WHEN 3 THEN @prod_id_in3
	                                WHEN 4 THEN @prod_id_in4
	                                WHEN 5 THEN @prod_id_in5
	                                WHEN 6 THEN @prod_id_in6
	                                WHEN 7 THEN @prod_id_in7
	                                WHEN 8 THEN @prod_id_in8
	                                WHEN 9 THEN @prod_id_in9
    END

    SELECT @qty = CASE @item_id WHEN 0 THEN @qty_in0
	                            WHEN 1 THEN @qty_in1
	                            WHEN 2 THEN @qty_in2
	                            WHEN 3 THEN @qty_in3
	                            WHEN 4 THEN @qty_in4
	                            WHEN 5 THEN @qty_in5
	                            WHEN 6 THEN @qty_in6
	                            WHEN 7 THEN @qty_in7
	                            WHEN 8 THEN @qty_in8
	                            WHEN 9 THEN @qty_in9
    END

    SELECT @cur_quan=QUAN_IN_STOCK, @cur_sales=SALES FROM INVENTORY WHERE PROD_ID=@prod_id

    SET @new_quan = @cur_quan - @qty
    SET @new_sales = @cur_Sales + @qty

    IF (@new_quan < 0)
      BEGIN
        ROLLBACK TRANSACTION
        SELECT 0
        RETURN
      END
    ELSE
      BEGIN
        UPDATE INVENTORY SET QUAN_IN_STOCK=@new_quan, SALES=@new_sales WHERE PROD_ID=@prod_id
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
          @item_id + 1,
          @neworderid,
          @prod_id,
          @qty,
          @date_in
          )
        
        INSERT INTO CUST_HIST
          (
          CUSTOMERID,
          ORDERID,
          PROD_ID
          )
        VALUES
          (
          @customerid_in,
          @neworderid,
          @prod_id
          )
      
        SET @item_id = @item_id + 1
      END    
  END

  COMMIT

  SELECT @neworderid
GO

