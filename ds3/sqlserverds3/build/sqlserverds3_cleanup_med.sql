
--sqlserverds3_cleanup_med.sql: cleans up new users and orders and resets IDENTITY columns; reloads INV and re-creates trigger; cleans reorder table

use DS3
delete from REORDER 
go
delete from CUSTOMERS where CUSTOMERID > 2000000
go
delete from ORDERS where ORDERID > 1200000
go
delete from ORDERLINES where ORDERID > 1200000
go
delete from CUST_HIST where ORDERID > 1200000
go
delete from REVIEWS where REVIEWID > 2000000
go
dbcc checkident('CUSTOMERS', RESEED, 2000000)
go
dbcc checkident('CUSTOMERS', NORESEED)
go
dbcc checkident('ORDERS', RESEED, 1200000)
go
dbcc checkident('ORDERS', NORESEED)
go
dbcc checkident('REVIEWS', RESEED, 2000000)
go
dbcc checkident('REVIEWS', NORESEED)
go

alter database DS3 set recovery bulk_logged
go
drop table INVENTORY
go
CREATE TABLE INVENTORY
  (
  PROD_ID INT NOT NULL,
  QUAN_IN_STOCK INT NOT NULL,
  SALES INT NOT NULL
  )
  ON DS_MISC_FG
GO
bulk insert INVENTORY from 'c:\ds3\data_files\prod\inv.csv' with (KEEPIDENTITY, FIELDTERMINATOR = ',')
go
alter database DS3 set recovery full
go
ALTER TABLE INVENTORY ADD CONSTRAINT PK_INVENTORY PRIMARY KEY CLUSTERED 
  (
  PROD_ID
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
