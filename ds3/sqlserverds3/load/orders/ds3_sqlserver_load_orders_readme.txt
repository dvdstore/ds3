ds3_sqlserver_load_orders_readme.txt

Instructions for loading DVD Store Version 3 (DS3) database orders data
(assumes data files are in directory c:\ds3\data_files\orders)

  osql -Usa -P -i sqlserverds3_load_orders.sql
  osql -Usa -P -i sqlserverds3_load_orderlines.sql
  osql -Usa -P -i sqlserverds3_load_cust_hist.sql

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  1/4/16
