
-- pgsqlds3_create_db.sql: DVD Store Database Version 3.0 Build Script - Postgres version
-- Copyright (C) 2011 Vmware, Inc. 
-- Last updated 3/31/21


-- Database for PostgreSQL . Not needed for Cloud

DROP DATABASE IF EXISTS DS3;
CREATE DATABASE DS3;
CREATE USER  DS3 WITH SUPERUSER;
ALTER USER DS3 WITH PASSWORD 'ds3';

