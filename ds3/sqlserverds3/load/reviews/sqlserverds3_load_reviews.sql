use DS3
go
alter database DS3 set recovery bulk_logged
go
bulk insert REVIEWS from 'e:\ds3\data_files\reviews\reviews.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
bulk insert REVIEWS_HELPFULNESS from 'e:\ds3\data_files\reviews\review_helpfulness.csv' with (KEEPIDENTITY, TABLOCK, FIELDTERMINATOR = ',')
go
alter database DS3 set recovery full
go
