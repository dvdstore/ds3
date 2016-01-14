use DS3
go
alter database DS3 set recovery bulk_logged
go
bulk insert PRODUCTS from 'e:\ds3\data_files\prod\prod.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS3 set recovery full
go
