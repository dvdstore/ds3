use DS3
go
alter database DS3 set recovery bulk_logged
go
bulk insert CUSTOMERS from 'e:\ds3\data_files\cust\us_cust.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUSTOMERS from 'e:\ds3\data_files\cust\row_cust.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS3 set recovery full
go
