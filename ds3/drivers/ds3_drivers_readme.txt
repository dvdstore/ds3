ds3_drivers_readme.txt


In this directory are the components necessary to create a database-independent web driver as well as
instructions for compiling database-dependent direct drivers.

When compiled with the appropriate set of web- or database-dependent functions, ds3xdriver generates 
orders against the DVD Store Database V.3 through web interface or directly against database.
Simulates users logging in to store or creating new customer data; browsing for DVDs by title, actor or 
category, and purchasing selected DVDs

Compile with appropriate functions file to generate driver for web, SQL Server, MySQL or Oracle target:
 csc /out:ds3webdriver.exe       ds3xdriver.cs ds3webfns.cs       /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS
 csc /out:ds3sqlserverdriver.exe ds3xdriver.cs ds3sqlserverfns.cs /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS
 csc /out:ds3mysqldriver.exe     ds3xdriver.cs ds3mysqlfns.cs     /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  /r:<path>MySql.Data.dll
 csc /out:ds3oracledriver.exe    ds3xdriver.cs ds3oraclefns.cs    /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  /r:<path>Oracle.DataAccess.dll
 csc /out:ds3pgsqldriver.exe     ds3xdriver.cs ds3pgsqlfns.cs     /d:USE_WIN32_TIMER /d:GEN_PERF_CTRS  "/r:<path>Npgsql.dll

 USE_WIN32_TIMER: if defined, program will use high resolution WIN32 timers (not supported in mono)
 GEN_PERF_CTRS: if defined, program will generate Windows Perfmon performance counters (not supported in mono) 

The database functions files are found in the database directories, i.e.
./ds2/mysqlds3
./ds2/sqlserverds3
./ds2/oracleds3
./ds2/pgsqlds3

csc is installed with Microsoft.NET   Typical location: C:\WINNT\Microsoft.NET\Framework\v2.0.50727

To see syntax, type program name by itself on a command line.

Note: the MySQL direct driver requires the MySQL Connector.NET
      the Oracle direct driver requires the Oracle Data Provider for .NET
      the PostgreSQL drirect driver requires Npgsql, the Postgresql Data Provider for .Net

Directory structure:

./ds3/drivers/ds3xdriver.cs          main driver program
./ds3/drivers/ds3xdriver.cs2003      main driver program for Visual Studio 2003
./ds3/drivers/ds3webfns.cs           web driver functions
./ds3/drivers/ds3webdriver.exe       web driver compiled as above
./ds3/drivers/ds3webdriver_mono.exe  web driver compiled without defines, for mono

To know more about DVDStore v3 and how to use driver programs in DVDStore 3, please go through document ds3_Documentation.txt 
under /ds3/ folder.

<davejaffe7@dell.com> and <tmuirhead@vmware.com>  5/15/15
