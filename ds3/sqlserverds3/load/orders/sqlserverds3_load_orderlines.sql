use DS3
go
alter database DS3 set recovery bulk_logged
go
set dateformat ymd
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\jan_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\feb_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\mar_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\apr_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\may_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\jun_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\jul_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\aug_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\sep_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\oct_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\nov_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERLINES from 'e:\ds3\data_files\orders\dec_orderlines.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS3 set recovery full
go
