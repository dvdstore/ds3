
-- sqlserverds3_create_all_med.sql: 
-- DVD Store Database Version 3 Build, Load and Create Index Script - SQL Server version - Small DB
-- Copyright (C) 2007 Dell, Inc. <davejaffe7@gmail.com> and <tmuirhead@vmware.com>
-- Last updated 11/05/15


-- sqlserverds3_create_db.sql

IF EXISTS (SELECT * FROM SYSDATABASES WHERE NAME='DS3')
DROP DATABASE DS3
GO

CREATE DATABASE DS3 ON 
  PRIMARY
    (
    NAME = 'primary', 
    FILENAME = 'c:\sql\dbfiles\ds.mdf'
    ),
  FILEGROUP DS_MISC_FG
    (
    NAME = 'ds_misc', 
    FILENAME = 'c:\sql\dbfiles\ds_misc.ndf',
    SIZE = 200MB
    ),
  FILEGROUP DS_CUST_FG
    (
    NAME = 'cust1', 
    FILENAME = 'c:\sql\dbfiles\cust1.ndf',
    SIZE = 600MB
    ),
    (
    NAME = 'cust2', 
    FILENAME = 'c:\sql\dbfiles\cust2.ndf',
    SIZE = 600MB
    ),
  FILEGROUP DS_ORDERS_FG
    (
    NAME = 'orders1', 
    FILENAME = 'c:\sql\dbfiles\orders1.ndf',
    SIZE = 300MB
    ),
    (
    NAME = 'orders2', 
    FILENAME = 'c:\sql\dbfiles\orders2.ndf',
    SIZE = 300MB
    ),
  FILEGROUP DS_IND_FG
    (
    NAME = 'ind1', 
    FILENAME = 'c:\sql\dbfiles\ind1.ndf',
    SIZE = 150MB
    ),
    (
    NAME = 'ind2', 
    FILENAME = 'c:\sql\dbfiles\ind2.ndf',
    SIZE = 150MB
    ),
  FILEGROUP DS_MEMBER_FG
    (
    NAME = 'member1',
    FILENAME = 'c:\sql\dbfiles\member1.ndf',
    SIZE = 100MB
    ),
    (
    NAME = 'member2', 
    FILENAME = 'c:\sql\dbfiles\member2.ndf',
    SIZE = 100MB
    ),
FILEGROUP DS_REVIEW_FG
    (
    NAME = 'review1',
    FILENAME = 'c:\sql\dbfiles\review1.ndf',
    SIZE = 300MB
    ),
    (
    NAME = 'review2', 
    FILENAME = 'c:\sql\dbfiles\review2.ndf',
    SIZE = 300MB
    )
  LOG ON
    (
    NAME = 'ds_log', 
    FILENAME = 'c:\sql\dbfiles\ds_log.ldf',
    SIZE = 1000MB
    )
GO

USE DS3
GO

-- Tables

CREATE TABLE CUSTOMERS
  (
  CUSTOMERID INT IDENTITY NOT NULL, 
  FIRSTNAME VARCHAR(50) NOT NULL, 
  LASTNAME VARCHAR(50) NOT NULL, 
  ADDRESS1 VARCHAR(50) NOT NULL, 
  ADDRESS2 VARCHAR(50), 
  CITY VARCHAR(50) NOT NULL, 
  STATE VARCHAR(50), 
  ZIP INT, 
  COUNTRY VARCHAR(50) NOT NULL, 
  REGION TINYINT NOT NULL,
  EMAIL VARCHAR(50),
  PHONE VARCHAR(50),
  CREDITCARDTYPE TINYINT NOT NULL,
  CREDITCARD VARCHAR(50) NOT NULL, 
  CREDITCARDEXPIRATION VARCHAR(50) NOT NULL, 
  USERNAME VARCHAR(50) NOT NULL, 
  PASSWORD VARCHAR(50) NOT NULL, 
  AGE TINYINT, 
  INCOME INT,
  GENDER VARCHAR(1)
  )
  ON DS_CUST_FG
GO  
  
CREATE TABLE CUST_HIST
  (
  CUSTOMERID INT NOT NULL, 
  ORDERID INT NOT NULL, 
  PROD_ID INT NOT NULL 
  )
  ON DS_CUST_FG
GO
  
CREATE TABLE MEMBERSHIP
  (
  CUSTOMERID INT NOT NULL, 
  MEMBERSHIPTYPE INT NOT NULL, 
  EXPIREDATE DATETIME NOT NULL 
  )
  ON DS_MEMBER_FG
GO


CREATE TABLE ORDERS
  (
  ORDERID INT IDENTITY NOT NULL, 
  ORDERDATE DATETIME NOT NULL, 
  CUSTOMERID INT NOT NULL, 
  NETAMOUNT MONEY NOT NULL, 
  TAX MONEY NOT NULL, 
  TOTALAMOUNT MONEY NOT NULL
  ) 
  ON DS_ORDERS_FG
GO

CREATE TABLE ORDERLINES
  (
  ORDERLINEID SMALLINT NOT NULL, 
  ORDERID INT NOT NULL, 
  PROD_ID INT NOT NULL, 
  QUANTITY SMALLINT NOT NULL, 
  ORDERDATE DATETIME NOT NULL
  ) 
  ON DS_ORDERS_FG
GO

CREATE TABLE PRODUCTS
  (
  PROD_ID INT IDENTITY NOT NULL, 
  CATEGORY TINYINT NOT NULL, 
  TITLE VARCHAR(50) NOT NULL, 
  ACTOR VARCHAR(50) NOT NULL, 
  PRICE MONEY NOT NULL, 
  SPECIAL TINYINT,
  COMMON_PROD_ID INT NOT NULL,
  MEMBERSHIP_ITEM INT NOT NULL
  )
  ON DS_MISC_FG
GO 

CREATE TABLE REVIEWS
  (
  REVIEW_ID INT IDENTITY NOT NULL, 
  PROD_ID INT NOT NULL,
  REVIEW_DATE DATETIME NOT NULL,
  STARS INT NOT NULL,
  CUSTOMERID INT NOT NULL, 
  REVIEW_SUMMARY VARCHAR(50) NOT NULL, 
  REVIEW_TEXT VARCHAR(1000) NOT NULL
  )
  ON DS_REVIEW_FG
GO 

CREATE TABLE REVIEWS_HELPFULNESS
  (
  REVIEW_HELPFULNESS_ID INT IDENTITY NOT NULL, 
  REVIEW_ID INT NOT NULL,
  CUSTOMERID INT NOT NULL,  
  HELPFULNESS INT NOT NULL
  )
  ON DS_REVIEW_FG
GO 

CREATE TABLE INVENTORY
  (
  PROD_ID INT NOT NULL,
  QUAN_IN_STOCK INT NOT NULL,
  SALES INT NOT NULL
  )
  ON DS_MISC_FG
GO

CREATE TABLE CATEGORIES
  (
  CATEGORY TINYINT IDENTITY NOT NULL, 
  CATEGORYNAME VARCHAR(50) NOT NULL, 
  )
  ON DS_MISC_FG
GO 

  SET IDENTITY_INSERT CATEGORIES ON 
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (1,'Action')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (2,'Animation')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (3,'Children')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (4,'Classics')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (5,'Comedy')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (6,'Documentary')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (7,'Drama')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (8,'Family')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (9,'Foreign')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (10,'Games')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (11,'Horror')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (12,'Music')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (13,'New')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (14,'Sci-Fi')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (15,'Sports')
  INSERT INTO CATEGORIES (CATEGORY, CATEGORYNAME) VALUES (16,'Travel')
  GO

CREATE TABLE REORDER
  (
  PROD_ID INT NOT NULL,
  DATE_LOW DATETIME NOT NULL,
  QUAN_LOW INT NOT NULL,
  DATE_REORDERED DATETIME,
  QUAN_REORDERED INT,
  DATE_EXPECTED DATETIME
  )
  ON DS_MISC_FG
GO

-- This keeps the number of items with low QUAN_IN_STOCK constant so that the rollback rate is constant 
CREATE TRIGGER RESTOCK ON INVENTORY AFTER UPDATE
AS
  DECLARE @changedPROD_ID INT, @oldQUAN_IN_STOCK INT, @newQUAN_IN_STOCK INT;
  IF UPDATE(QUAN_IN_STOCK)
    BEGIN
      SELECT @changedPROD_ID = i.PROD_ID, @oldQUAN_IN_STOCK = d.QUAN_IN_STOCK, @newQUAN_IN_STOCK = i.QUAN_IN_STOCK
        FROM inserted i INNER JOIN deleted d ON i.PROD_ID = d.PROD_ID
      IF @newQUAN_IN_STOCK < 3    -- assumes quantity ordered is 1, 2, or 3 - change if different
        BEGIN
          INSERT INTO REORDER
            (
            PROD_ID,
            DATE_LOW,
            QUAN_LOW
            )
          VALUES
            (
            @changedPROD_ID,
            GETDATE(),
            @newQUAN_IN_STOCK
            )
          UPDATE INVENTORY SET QUAN_IN_STOCK  = @oldQUAN_IN_STOCK WHERE PROD_ID = @changedPROD_ID
        END
    END
  RETURN
GO

DECLARE @db_id int, @tbl_id int
USE DS3
SET @db_id = DB_ID('DS3')
SET @tbl_id = OBJECT_ID('DS3..CATEGORIES')
DBCC PINTABLE (@db_id, @tbl_id)

SET @db_id = DB_ID('DS3')
SET @tbl_id = OBJECT_ID('DS3..PRODUCTS')
DBCC PINTABLE (@db_id, @tbl_id)
USE DS3
GO

-- sqlserverds2_load_cust.sql

use DS3
go
alter database DS3 set recovery bulk_logged
go
bulk insert CUSTOMERS from 'c:\ds3\data_files\cust\us_cust.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUSTOMERS from 'c:\ds3\data_files\cust\row_cust.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds3_load_orders.sql

set dateformat ymd
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\jan_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\feb_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\mar_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\apr_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\may_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\jun_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\jul_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\aug_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\sep_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\oct_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\nov_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'c:\ds3\data_files\orders\dec_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds3_load_orderlines.sql

bulk insert ORDERLINES from 'c:\ds3\data_files\orders\jan_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\feb_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\mar_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\apr_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\may_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\jun_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\jul_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\aug_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\sep_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\oct_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\nov_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'c:\ds3\data_files\orders\dec_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds3_load_cust_hist.sql

bulk insert CUST_HIST from 'c:\ds3\data_files\orders\jan_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\feb_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\mar_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\apr_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\may_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\jun_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\jul_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\aug_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\sep_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\oct_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\nov_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'c:\ds3\data_files\orders\dec_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds3_load_prod.sql

bulk insert PRODUCTS from 'c:\ds3\data_files\prod\prod.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds3_load_inv.sql

bulk insert INVENTORY from 'c:\ds3\data_files\prod\inv.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds3_load_memebership.sql

bulk insert MEMBERSHIP from 'c:\ds3\data_files\membership\membership.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

-- sqlserverds3_load_reviews.sql

bulk insert REVIEWS from 'c:\ds3\data_files\reviews\reviews.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert REVIEWS_HELPFULNESS from 'c:\ds3\data_files\reviews\review_helpfulness.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go

alter database DS3 set recovery full
go

-- sqlserverds3_create_ind.sql

USE DS3
GO

ALTER TABLE CATEGORIES ADD CONSTRAINT PK_CATEGORIES PRIMARY KEY CLUSTERED 
  (
  CATEGORY
  )  
  ON DS_MISC_FG 
GO

ALTER TABLE CUSTOMERS ADD CONSTRAINT PK_CUSTOMERS PRIMARY KEY CLUSTERED 
  (
  CUSTOMERID
  )  
  ON DS_CUST_FG 
GO

CREATE UNIQUE INDEX IX_CUST_UN_PW ON CUSTOMERS 
  (
  USERNAME, 
  PASSWORD
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_CUST_HIST_CUSTOMERID ON CUST_HIST
  (
  CUSTOMERID
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_CUST_HIST_CUSTOMERID_PRODID ON CUST_HIST 
  (
  CUSTOMERID ASC,
  PROD_ID ASC
  )
  ON DS_IND_FG
GO

ALTER TABLE CUST_HIST
  ADD CONSTRAINT FK_CUST_HIST_CUSTOMERID FOREIGN KEY (CUSTOMERID)
  REFERENCES CUSTOMERS (CUSTOMERID)
  ON DELETE CASCADE
GO

ALTER TABLE ORDERS ADD CONSTRAINT PK_ORDERS PRIMARY KEY CLUSTERED 
  (
  ORDERID
  )  
  ON DS_ORDERS_FG 
GO

CREATE INDEX IX_ORDER_CUSTID ON ORDERS
  (
  CUSTOMERID
  )
  ON DS_IND_FG
GO

ALTER TABLE ORDERLINES ADD CONSTRAINT PK_ORDERLINES PRIMARY KEY CLUSTERED 
  (
  ORDERID,
  ORDERLINEID
  )  
  ON DS_ORDERS_FG 
GO

ALTER TABLE ORDERLINES ADD CONSTRAINT FK_ORDERID FOREIGN KEY (ORDERID)
  REFERENCES ORDERS (ORDERID)
  ON DELETE CASCADE
GO

ALTER TABLE INVENTORY ADD CONSTRAINT PK_INVENTORY PRIMARY KEY CLUSTERED 
  (
  PROD_ID
  )  
  ON DS_MISC_FG 
GO

ALTER TABLE PRODUCTS ADD CONSTRAINT PK_PRODUCTS PRIMARY KEY CLUSTERED 
  (
  PROD_ID
  )  
  ON DS_MISC_FG 
GO

CREATE INDEX IX_PROD_PRODID ON PRODUCTS 
  (
  PROD_ID ASC
  )
  INCLUDE (TITLE)
  ON DS_IND_FG
GO

CREATE INDEX IX_PROD_PRODID_COMMON_PRODID ON PRODUCTS 
  (
  PROD_ID ASC,
  COMMON_PROD_ID ASC
  )
  INCLUDE (TITLE, ACTOR)
  ON DS_IND_FG
GO

CREATE INDEX IX_PROD_SPECIAL_CATEGORY_PRODID ON PRODUCTS 
  (
  SPECIAL ASC,
  CATEGORY ASC,
  PROD_ID ASC
  )
  INCLUDE (TITLE, ACTOR, PRICE, COMMON_PROD_ID)
  ON DS_IND_FG
GO


EXEC sp_fulltext_database 'enable'
EXEC sp_fulltext_catalog  'FULLTEXTCAT_DSPROD', 'create', 'c:\sql\dbfiles'
EXEC sp_fulltext_table    'PRODUCTS',           'create', 'FULLTEXTCAT_DSPROD', 'PK_PRODUCTS'
EXEC sp_fulltext_column   'PRODUCTS',           'ACTOR', 'add'
EXEC sp_fulltext_column   'PRODUCTS',           'TITLE', 'add'
EXEC sp_fulltext_table    'PRODUCTS',           'activate'
EXEC sp_fulltext_catalog  'FULLTEXTCAT_DSPROD', 'start_full'
GO

CREATE INDEX IX_PROD_CATEGORY ON PRODUCTS 
  (
  CATEGORY
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_PROD_SPECIAL ON PRODUCTS
  (
  SPECIAL
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_PROD_MEMBERSHIP ON PRODUCTS
  (
  MEMBERSHIP_ITEM
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_INV_PROD_ID on INVENTORY
  (
  PROD_ID
  )
  ON DS_IND_FG
GO

ALTER TABLE MEMBERSHIP ADD CONSTRAINT PK_MEMBERSHIP PRIMARY KEY CLUSTERED 
  (
  CUSTOMERID
  )  
  ON DS_IND_FG 
GO

ALTER TABLE MEMBERSHIP
  ADD CONSTRAINT FK_MEMBERSHIP_CUSTID FOREIGN KEY (CUSTOMERID)
  REFERENCES CUSTOMERS (CUSTOMERID)
  ON DELETE CASCADE
GO

ALTER TABLE REVIEWS ADD CONSTRAINT PK_REVIEWS PRIMARY KEY CLUSTERED 
  (
  REVIEW_ID
  )  
  ON DS_REVIEW_FG 
GO

ALTER TABLE REVIEWS
  ADD CONSTRAINT FK_REVIEWS_PROD_ID FOREIGN KEY (PROD_ID)
  REFERENCES PRODUCTS (PROD_ID)
  ON DELETE CASCADE
GO

ALTER TABLE REVIEWS
  ADD CONSTRAINT FK_REVIEWS_CUSTOMERID FOREIGN KEY (CUSTOMERID)
  REFERENCES CUSTOMERS (CUSTOMERID)
  ON DELETE CASCADE
GO

CREATE INDEX IX_REVIEWS_PROD_ID ON REVIEWS
  (
  PROD_ID
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_REVIEWS_STARS ON REVIEWS
  (
  STARS
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_REVIEWS_PRODSTARS ON REVIEWS
  (
  PROD_ID,STARS
  )
  ON DS_IND_FG
GO

ALTER TABLE REVIEWS_HELPFULNESS ADD CONSTRAINT PK_REVIEWS_HELPFULNESS PRIMARY KEY CLUSTERED 
  (
  REVIEW_HELPFULNESS_ID
  )  
  ON DS_REVIEW_FG 
GO

ALTER TABLE REVIEWS_HELPFULNESS
  ADD CONSTRAINT FK_REVIEW_ID FOREIGN KEY (REVIEW_ID)
  REFERENCES REVIEWS (REVIEW_ID)
  ON DELETE CASCADE
GO

CREATE INDEX IX_REVIEWS_HELP_REVID ON REVIEWS_HELPFULNESS
  (
  REVIEW_ID
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_REVIEWS_HELP_CUSTID ON REVIEWS_HELPFULNESS
  (
  CUSTOMERID
  )
  ON DS_IND_FG
GO

CREATE INDEX IX_REORDER_PRODID ON REORDER
  (
  PROD_ID
  )
  ON DS_IND_FG
GO

CREATE NONCLUSTERED INDEX IX_REVIEWS_PRODID_REVID_DATE ON REVIEWS
  (
  PROD_ID ASC,
  REVIEW_ID ASC,
  REVIEW_DATE ASC
  )
  INCLUDE (STARS,CUSTOMERID,REVIEW_SUMMARY,REVIEW_TEXT)
  WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
  ON DS_IND_FG
go

CREATE NONCLUSTERED INDEX IX_REVIEWSHELPFULNESS_ID_HELPID ON [dbo].[REVIEWS_HELPFULNESS]
  (
  REVIEW_ID ASC,
  REVIEW_HELPFULNESS_ID ASC
  )
  INCLUDE (HELPFULNESS)
  WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
  ON DS_IND_FG
go



CREATE STATISTICS stat_cust_cctype_username ON CUSTOMERS(CREDITCARDTYPE, USERNAME)
GO
CREATE STATISTICS stat_cust_cctype_customerid ON CUSTOMERS(CREDITCARDTYPE, CUSTOMERID)
GO
CREATE STATISTICS stat_prod_prodid_special ON PRODUCTS(PROD_ID, SPECIAL)
GO
CREATE STATISTICS stat_prod_category_prodid ON PRODUCTS(CATEGORY, PROD_ID)
GO
CREATE STATISTICS stat_reviews_reviewid_stars ON REVIEWS(REVIEW_ID, STARS)
GO
CREATE STATISTICS stat_reviews_prodid_custid ON REVIEWS(PROD_ID, CUSTOMERID)
GO
CREATE STATISTICS stat_reviews_reviewid_date ON REVIEWS(REVIEW_ID, REVIEW_DATE)
GO
CREATE STATISTICS stat_reviews_date_prodid ON REVIEWS(REVIEW_DATE, PROD_ID)
GO
CREATE STATISTICS stat_reviews_prodid_stars_reviewid ON REVIEWS(PROD_ID, STARS, REVIEW_ID)
GO

-- sqlserverds3_create_sp.sql

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


--Added by GSK Create Login and then add users and their specific roles for database
USE [master]
GO
IF NOT EXISTS(SELECT name FROM sys.server_principals WHERE name = 'ds3user')
BEGIN
	CREATE LOGIN [ds3user] WITH PASSWORD=N'',
	DEFAULT_DATABASE=[master],
	DEFAULT_LANGUAGE=[us_english],
	CHECK_EXPIRATION=OFF,
	CHECK_POLICY=OFF


	EXEC master..sp_addsrvrolemember @loginame = N'ds3user', @rolename = N'sysadmin'

	USE [DS3]
	CREATE USER [ds3DS3user] FOR LOGIN [ds3user]

	USE [DS3]
	EXEC sp_addrolemember N'db_owner', N'ds3DS3user'

	USE [master]
	CREATE USER [ds3masteruser] FOR LOGIN [ds3user]

	USE [master]
	EXEC sp_addrolemember N'db_owner', N'ds3masteruser'

	USE [model]
	CREATE USER [ds3modeluser] FOR LOGIN [ds3user]

	USE [model]
	EXEC sp_addrolemember N'db_owner', N'ds3modeluser'

	USE [msdb]
	CREATE USER [ds3msdbuser] FOR LOGIN [ds3user]

	USE [msdb]
	EXEC sp_addrolemember N'db_owner', N'ds3msdbuser'

	USE [tempdb]
	CREATE USER [ds3tempdbuser] FOR LOGIN [ds3user]

	USE [tempdb]
	EXEC sp_addrolemember N'db_owner', N'ds3tempdbuser'

END
GO
