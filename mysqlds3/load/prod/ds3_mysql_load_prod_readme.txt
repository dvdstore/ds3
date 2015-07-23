ds3_mysql_load_prod_readme.txt

Instructions for creating and loading DVD Store Version 3 (DS3) database product data
(assumes data files are in directory ../../../data_files/prod)

  mysql --password=pw < mysqlds3_load_prod.sql
  mysql --password=pw < mysqlds3_load_inv.sql

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  5/28/15
