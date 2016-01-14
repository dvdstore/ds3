use DS3
go
alter database DS3 set recovery bulk_logged
go
bulk insert MEMBERSHIP from 'e:\ds3\data_files\membership\membership.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS3 set recovery full
go
