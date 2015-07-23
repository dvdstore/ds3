ds3_mysql_load_orders_readme.txt

Instructions for creating and loading DVD Store Version 3 (DS3) database orders data
(assumes data files are in directory ../../../data_files/orders)

  mysql --password=pw < mysqlds3_load_orders.sql
  mysql --password=pw < mysqlds3_load_orderlines.sql
  mysql --password=pw < mysqlds3_load_cust_hist.sql

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  5/28/15
