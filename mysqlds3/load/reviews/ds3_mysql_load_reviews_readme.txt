ds3_mysql_load_reviews_readme.txt

Instructions for loading DVD Store Version 3 (DS3) database product reviews data
(assumes data files are in directory ../../../data_files/reviews)

  mysql --password=pw < mysqlds3_load_reviews.sql
  mysql --password=pw < mysqlds3_load_review_helpfulness.sql

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  5/28/15
