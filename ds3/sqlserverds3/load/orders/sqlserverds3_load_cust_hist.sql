use DS3
go
alter database DS3 set recovery bulk_logged
go
set dateformat ymd
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\jan_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\feb_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\mar_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\apr_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\may_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\jun_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\jul_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\aug_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\sep_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\oct_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\nov_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert CUST_HIST from 'e:\ds3\data_files\orders\dec_cust_hist.csv' with (TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS3 set recovery full
go
