sqlldr ds3/ds3 CONTROL=us_cust.ctl, LOG=us.log, BAD=us.bad, DATA=../../../data_files/cust/us_cust.csv &
sqlldr ds3/ds3 CONTROL=row_cust.ctl, LOG=row.log, BAD=row.bad, DATA=../../../data_files/cust/row_cust.csv 
