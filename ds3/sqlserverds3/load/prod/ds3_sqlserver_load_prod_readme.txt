ds3_sqlserver_load_prod_readme.txt

Instructions for loading DVD Store Version 3 (DS3) database product data
(assumes data files are in directory c:\ds3\data_files\prod)

  osql -Usa -P -i sqlserverds3_load_prod.sql
  osql -Usa -P -i sqlserverds3_load_inv.sql

<davejaffe7@gmail.com> and <tmuirhead@vmware.com>  1/4/16
