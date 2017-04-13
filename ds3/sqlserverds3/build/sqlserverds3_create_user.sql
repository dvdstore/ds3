--Added by GSK Create Login and then add users and their specific roles for database
USE [master]
GO
IF NOT EXISTS(SELECT name FROM sys.server_principals WHERE name = 'ds3user')
BEGIN
	CREATE LOGIN [ds3user] WITH PASSWORD=N'',
	DEFAULT_DATABASE=[master],
	DEFAULT_LANGUAGE=[us_english],
	CHECK_EXPIRATION=OFF,
	CHECK_POLICY=OFF


	EXEC master..sp_addsrvrolemember @loginame = N'ds3user', @rolename = N'sysadmin'

	USE [DS3]
	CREATE USER [ds3DS3user] FOR LOGIN [ds3user]

	USE [DS3]
	EXEC sp_addrolemember N'db_owner', N'ds3DS3user'

	USE [master]
	CREATE USER [ds3masteruser] FOR LOGIN [ds3user]

	USE [master]
	EXEC sp_addrolemember N'db_owner', N'ds3masteruser'

	USE [model]
	CREATE USER [ds3modeluser] FOR LOGIN [ds3user]

	USE [model]
	EXEC sp_addrolemember N'db_owner', N'ds3modeluser'

	USE [msdb]
	CREATE USER [ds3msdbuser] FOR LOGIN [ds3user]

	USE [msdb]
	EXEC sp_addrolemember N'db_owner', N'ds3msdbuser'

	USE [tempdb]
	CREATE USER [ds3tempdbuser] FOR LOGIN [ds3user]

	USE [tempdb]
	EXEC sp_addrolemember N'db_owner', N'ds3tempdbuser'

END
GO
