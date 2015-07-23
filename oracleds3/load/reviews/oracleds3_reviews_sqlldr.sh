sqlldr ds3/ds3 CONTROL=reviews.ctl, LOG=reviews.log, BAD=reviews.bad, DATA=../../../data_files/reviews/reviews.csv &
sqlldr ds3/ds3 CONTROL=reviewhelpfulness.ctl, LOG=reviewhelp.log, BAD=reviewhelp.bad, DATA=../../../data_files/reviews/review_helpfulness.csv
