use DS3
go
alter database DS3 set recovery bulk_logged
go
set dateformat ymd
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\jan_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\feb_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\mar_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\apr_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\may_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\jun_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\jul_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\aug_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\sep_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\oct_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\nov_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert ORDERS from 'e:\ds3\data_files\orders\dec_orders.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS3 set recovery full
go
