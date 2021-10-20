
-- pgsqlds3_create_trig.sql: DVD Store Database Version 3.0 Build Script - Postgres version
-- Copyright (C) 2011 Vmware, Inc. 
-- Last updated 3/31/21

-- This keeps the number of items with low QUAN_IN_STOCK constant so that the rollback rate is constant


CREATE OR REPLACE FUNCTION RESTOCK_ORDER()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $RESTOCK_ORDER$
DECLARE
  restockto INTEGER;
BEGIN
  IF ( NEW.QUAN_IN_STOCK < 3) THEN
    restockto = 250;
    IF ( ( NEW.PROD_ID +1) % 10000 = 0 ) THEN
      restockto = 2500;
    END IF;
    INSERT INTO REORDER ( PROD_ID, DATE_LOW, QUAN_LOW)
    VALUES ( NEW.PROD_ID, current_timestamp , restockto - NEW.QUAN_IN_STOCK);
    NEW.QUAN_IN_STOCK = restockto;
    -- UPDATE INVENTORY SET QUAN_IN_STOCK = OLD.QUAN_IN_STOCK WHERE PROD_ID = NEW.PROD_ID;
  END IF;
RETURN NEW;
END;
$RESTOCK_ORDER$;

CREATE TRIGGER RESTOCK BEFORE UPDATE ON INVENTORY
FOR EACH ROW
WHEN (OLD.QUAN_IN_STOCK IS DISTINCT FROM NEW.QUAN_IN_STOCK )
EXECUTE PROCEDURE  RESTOCK_ORDER();
