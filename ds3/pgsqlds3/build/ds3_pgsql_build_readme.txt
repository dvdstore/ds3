ds3_pgsql_build_readme.txt

Instructions for building DVD Store Version 3 (DS3) database

It is recommended to run pgsqlds3_create_all in the ds3/pgsqlds3 directory.  That script uses the
scripts in this build directory.  Additionally, the Install_DVDStore.pl script will place additional
versions of the cleanup script in this directory that are specific to the size.

The pgsqlds3_cleanup_XXXXXXX scripts are run to get the database back to the state it was after initial 
creation.  It does this by removing all of the new customers, new orders, new order history, and resetting
the inventory table to it's initial state.  

<tmuirhead@vmware.com>  11/2/21

