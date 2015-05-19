#!/bin/sh
sqlplus ds3/ds3 @{SQL_FNAME}
sqlldr ds3/ds3 CONTROL=../load/prod/inv.ctl, LOG=inv.log, BAD=inv.bad, DATA=../../data_files/prod/inv.csv 
