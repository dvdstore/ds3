sqlplus ds3/ds3 @oracleds3_cleanup_large_fk_disabled.sql
sqlldr ds3/ds3 CONTROL=../load/prod/inv.ctl, LOG=inv.log, BAD=inv.bad, DATA=../../data_files/prod/inv.csv 
