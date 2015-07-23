Delimiter $

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

