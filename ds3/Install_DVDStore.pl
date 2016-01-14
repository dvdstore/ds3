#!/usr/bin/perl
use strict;
#Perl script created by GSK 
#Last updated: 1/4/16

#Purpose of perl script: 
#			This perl script will achieve following things: 
#			1)  Calculate number of rows for each of customers, orders and products table according to database size.
#			2)  Call data generation C programs to create CSV files for each table in their respective folders.
#			3)  Create database build and cleanup *.sql scripts for the specified database size.
#Prerequisites for Perl script:
#			To run this perl script on windows machines, user needs to install cygwin with perl on windows machine.
#			To run this perl script on linux machines, user just needs to install perl package on linux machine.
#			To understand how to use this perl script and what parameter values should be given to this perl script,
#			please go through section 6 of documentation ds2.1_Documentation.txt in /ds2 folder


#Here We are assuming default values for 10mb small SQL Server Database instance on Windows
#Though user can specify any valid value  
my $database_size = 10;					#Database Size
my $database_size_str = "mb";   		#String to indicate size: MB / GB or mb / gb
my $database_type = "mssql";    		#Type = mssql / mysql / pgsql / oracle / MSSQL / MYSQL / ORACLE /PGSQL
my $db_sys_type = "win";   				#System : win / linux / WIN / LINUX
my $db_file_path = "c:/";				#User can give any path to store DBFiles
										# For windows : Path should be C:\sqldbfiles\
my @arr_db_file_paths = ();
my @arr1_db_file_paths = ();

my $is_GB_Size_S = "gb";
my $is_MB_Size_S = "mb";
my $is_Sys_Win_S = "win";
my $is_Sys_Linux_S = "linux";
my $is_DB_SQLServer_S = "mssql";
my $is_DB_MySQL_S = "mysql";
my $is_DB_PGSQL_S = "pgsql";
my $is_DB_Oracle_S = "oracle";

my $bln_is_Large_DB = 0;
my $bln_is_Small_DB = 0;
my $bln_is_Medium_DB = 0;
my $bln_is_Sys_Win = 0;
my $bln_is_Sys_Linux = 0;
my $bln_is_DB_MSSQL = 0;
my $bln_is_DB_MYSQL = 0;
my $bln_is_DB_PGSQL = 0;
my $bln_is_DB_ORACLE = 0;
my $str_is_Small_DB = "";
my $str_is_Medium_DB = "";
my $str_is_Large_DB = "";
my $str_file_name = "";				#Store name of file to be created from template 


print "Please enter following parameters: \n";
print "***********************************\n";
print "Please enter database size (integer expected) : "; 
chomp($database_size = <STDIN>);
print "Please enter whether above database size is in (MB / GB) : ";
chomp($database_size_str = <STDIN>);
print "Please enter database type (MSSQL / MYSQL / PGSQL / ORACLE) : ";
chomp($database_type = <STDIN>); 
print "Please enter system type on which DB Server is installed (WIN / LINUX) : ";
chomp($db_sys_type = <STDIN>);
print "***********************************\n";
#***************************************************************************************

#Set the flags according to parameters passed. These flags will be used further
if(lc($database_size_str) eq lc($is_GB_Size_S))
{
	if($database_size == 1)
	{
		$bln_is_Medium_DB = 1;	
		$str_is_Medium_DB = "M";
	}
	elsif($database_size > 1 && $database_size < 1024)
	{
		$bln_is_Large_DB = 1;
		$str_is_Large_DB = "L";
	}
}
elsif(lc($database_size_str) eq lc($is_MB_Size_S))
{
	if($database_size >= 1 && $database_size < 1024)
	{
		$bln_is_Small_DB = 1;
		$str_is_Small_DB = "S";
	}	
}

#Set the flags according to parameters passed. These flags will be used further
if(lc($db_sys_type) eq lc($is_Sys_Win_S))
{	
	$bln_is_Sys_Win = 1;
}
elsif(lc($db_sys_type) eq lc($is_Sys_Linux_S))
{
	$bln_is_Sys_Linux = 1;
}

#Set the flags according to parameters passed. These flags will be used further
if(lc($database_type) eq lc($is_DB_SQLServer_S))
{
	$bln_is_DB_MSSQL = 1;
}
elsif(lc($database_type) eq lc($is_DB_MySQL_S))
{
	$bln_is_DB_MYSQL = 1;	
}
elsif(lc($database_type) eq lc($is_DB_PGSQL_S))
{
	$bln_is_DB_PGSQL = 1;	
}
elsif(lc($database_type) eq lc($is_DB_Oracle_S))
{	
	$bln_is_DB_ORACLE = 1;
}

#***************************************************************************************

if($bln_is_DB_ORACLE == 1 || $bln_is_DB_MSSQL == 1)
{		
	if($bln_is_DB_ORACLE == 1)
	{
		#Only four paths needed - Cust table dbfile, Index dbfile, DS_MISC dbfile, Order table dbfile respectively
		#Just paths where datafiles will be stored are needed
		#Paths for oracle on windows should be like this : c:\oracledbfiles\
		#Paths for oracle on linux should be like this : /oracledbfiles/
		
		print "\nFor Oracle database scripts, total 6 paths needed to specify where cust, index, member, reviews,\n";
		print "\nds_misc and order dbfiles are stored. \n";
		print "\nIf only one path is specified, it will be assumed same for all dbfiles. \n";
		print "\nFor specifying multiple paths use ; character as seperator to specify multiple paths \n";
		
		print "\nPlease enter path(s) (; seperated if more than one path) where Database Files will be stored (ensure that path exists) : ";
		chomp($db_file_path = <STDIN>); 
		@arr1_db_file_paths = split(";",$db_file_path);   #Split tokenized string and store paths in array

		#If number of paths specified are between 2 and 6 or greater than 6 , its an error						 
		if((scalar(@arr1_db_file_paths) != 1 && scalar(@arr1_db_file_paths) < 6) || (scalar(@arr1_db_file_paths) > 6))
		{
			print "\nWrong number of paths entered!!!! \n";
			print "\nSix paths for following are needed:  Cust table dbfile, Index dbfile, Membership dbfile, Reviews dbfile, \n";
			print "\n DS_MISC dbfile, Order table dbfile respectively \n";
			exit(0);
		}
		#If single path is specified by user then paths for all dbfiles are assumed same
		if(scalar(@arr1_db_file_paths) == 1)
		{
			$arr_db_file_paths[0] =	$arr_db_file_paths[1] = $arr_db_file_paths[2] = $arr_db_file_paths[3] = $arr_db_file_paths[4] = $arr_db_file_paths[5]  = @arr1_db_file_paths[0];	
		}
		else #If 6 paths spacified then 
		{
			$arr_db_file_paths[0] = @arr1_db_file_paths[0];
			$arr_db_file_paths[1] = @arr1_db_file_paths[1];
			$arr_db_file_paths[2] = @arr1_db_file_paths[2];
			$arr_db_file_paths[3] = @arr1_db_file_paths[3];
			$arr_db_file_paths[4] = @arr1_db_file_paths[4];
                        $arr_db_file_paths[5] = @arr1_db_file_paths[5];
		}
		
	}
	elsif($bln_is_DB_MSSQL == 1)
	{
		#Nine paths for storing following files are needed: ds.mdf, ds_misc.ndf, cust1.ndf, cust2.ndf, orders1.ndf, orders2.ndf, ind1.ndf, ind2.ndf, member1.ndf, member2.ndf, review1.ndf, review2.ndfds_log.ldf, FULLTEXTCAT_DSPROD(catalog file for full text search)  respectively
		#Two dbfiles per table are assumed and path for second dbfile of same table is assumed same as that of first dbfile 
		#Just paths where datafiles will be stored are needed
		#Paths for SQL Server on windows should be like this : c:\sqldbfiles\
 
		
		print "\nFor SQL Server database scripts, total 9 paths needed to specify where ds.mdf,ds_misc,cust,order,index,member,review,log and full text catalog dbfiles are stored. \n";
		print "\nIf only one path is specified, it will be assumed same for all dbfiles. \n";
		print "\nFor specifying multiple paths use ; character as seperator to specify multiple paths \n";
				
		print "\nPlease enter path(s) (; seperated if more than one path) where Database Files will be stored. For example c:\\sql\\dbfiles\ (ensure that path exists) : ";
		chomp($db_file_path = <STDIN>); 
		@arr1_db_file_paths = split(";",$db_file_path);   #Split tokenized string and store paths in array
		
		#If number of paths specified are between 2 and 9 or greater than 9 , its an error
		if((scalar(@arr1_db_file_paths) != 1 && scalar(@arr1_db_file_paths) < 9) || (scalar(@arr1_db_file_paths) > 9))
		{
			print "\nWrong number of paths entered!!!! \n";
			print "\nNine paths for storing following files are needed: ds.mdf, ds_misc.ndf, cust1.ndf, orders1.ndf, ind1.ndf,member1.ndf, review1.ndf ds_log.ldf, FULLTEXTCAT_DSPROD(catalog file for full text search)  respectively \n";
			exit(0);
		}
		#If single path is specified by user then paths for all dbfiles are assumed same
		if(scalar(@arr1_db_file_paths) == 1)
		{
			$arr_db_file_paths[0] =	$arr_db_file_paths[1] = $arr_db_file_paths[2] = $arr_db_file_paths[3] = @arr1_db_file_paths[0];
			$arr_db_file_paths[4] =	$arr_db_file_paths[5] = $arr_db_file_paths[6] = $arr_db_file_paths[7] = @arr1_db_file_paths[0];		
			$arr_db_file_paths[8] =	$arr_db_file_paths[9] = $arr_db_file_paths[10] = $arr_db_file_paths[11] = @arr1_db_file_paths[0];
			$arr_db_file_paths[12] =	$arr_db_file_paths[13] = @arr1_db_file_paths[0];
		}
		else #If 9 paths spacified then 
		{
			$arr_db_file_paths[0] = @arr1_db_file_paths[0];   							#path for ds.mdf
			$arr_db_file_paths[1] = @arr1_db_file_paths[1];   							#path for ds_misc.ndf
			$arr_db_file_paths[2] = $arr_db_file_paths[3] = @arr1_db_file_paths[2];		#path for cust1.ndf and cust2.ndf
			$arr_db_file_paths[4] = $arr_db_file_paths[5] = @arr1_db_file_paths[3];		#path for orders1.ndf and orders2.ndf
			$arr_db_file_paths[6] = $arr_db_file_paths[7] = @arr1_db_file_paths[4];		#path for ind1.ndf and ind2.ndf
                $arr_db_file_paths[8] = $arr_db_file_paths[9] = @arr1_db_file_paths[5];		#path for member1.ndf and member2.ndf
                $arr_db_file_paths[10] = $arr_db_file_paths[11] = @arr1_db_file_paths[6];		#path for review1.ndf and review2.ndf
			$arr_db_file_paths[12] = @arr1_db_file_paths[7];								#path for ds_log.ldf
			$arr_db_file_paths[13] = @arr1_db_file_paths[8];								#path for full text catalog file
		}				
	}	
}

print "***********************************\n";
#***************************************************************************************

print "Initializing parameters...\n";
print "***********************************\n";
print "Database Size: $database_size \n";
print "Database size is in $database_size_str \n";
print "Database Type is $database_type \n";
print "System Type for DB Server is $db_sys_type \n";
if($bln_is_DB_ORACLE == 1 || $bln_is_DB_MSSQL == 1)
{
	print "File Paths : $db_file_path \n";		
}
print "***********************************\n";


#***************************************************************************************

#Code to extract driveletter on which perl script is executing
#This driveletter will be used in SQL Server 2008 template scripts to replace driveletter in CSV datafile path
use Cwd;
my $execute_path = "";
my $str_driveletter = "";
my $bln_IsCygwin = 0;

if(lc($^O) ne lc("linux"))  #If System on which perl script executes is windows
{
	$execute_path = getcwd;
	#If perl script is being run through Cygwin, paths start from /cygdrive/<driveletter>/
	#Else if perl script is run through DOS, paths start from <Driveletter>:\
	if(substr($execute_path, 0, 1) eq "/")	#Script run on Cygwin
	{		
		$bln_IsCygwin = 1;
		#Execute cygpath utility in cygwin to do conversion from cygwin style path to DOS style
		$execute_path = `cygpath -w $execute_path`;
		#Extract Driveletter from execute path (Driveletter now will be first letter)
		$str_driveletter = substr($execute_path, 0, 1);
	}
	else	#Script Run on DOS
	{
		$bln_IsCygwin = 0;
		#Extract first letter which is driveletter from executepath	
		$str_driveletter = substr($execute_path, 0, 1);
	}
}


#This script assumes directory/folder structure as follows
# On linux ds3 folder will be at root
# /
# /ds3/
# /ds3/data_files/
# /ds3/data_files/cust/
# /ds3/data_files/orders/
# /ds3/data_files/prod/
# /ds3/drivers/
# /ds3/mysqlds3/
# /ds3/mysqlds3/build/
# /ds3/mysqlds3/load/
# /ds3/mysqlds3/web/
# /ds3/oracleds3/
# /ds3/oracleds3/build/
# /ds3/oracleds3/load/
# /ds3/oracleds3/web/
# /ds3/sqlserverds3/
# /ds3/sqlserverds3/build/
# /ds3/sqlserverds3/load/
# /ds3/sqlserverds3/web/

#On Windows ds3 folder will be at <Driveletter>: and rest of folder structure will be same


#***************************************************************************************

print "\nCalculating Rows in tables!! \n";

#First we need to calculate ratio which will determine number of rows in Major tables
# Customer, Orders and Products

my $i_Cust_Rows = 0;
my $i_Ord_Rows = 0;
my $i_Prod_Rows = 0;
my $mult_Cust_Rows = 0;
my $mult_Ord_Rows = 0;
my $mult_Prod_Rows = 0;
my $ratio_Mult = 0;
my $ratio_Cust = 0;
my $ratio_Ord = 0;
my $ratio_Prod = 0;


#For small database (Database size greater than 10MB till 1GB/ 1000 MB)
if($bln_is_Small_DB == 1)
{
	#Now base DB will be 10MB database and ratio calculated wrt to that
	print "Small size database (less than 1 GB) \n";
	$mult_Cust_Rows = 20000;				# 2 x 10^4
	$mult_Ord_Rows = 1000;					# 1 x 10^3
	$mult_Prod_Rows = 10000;				# 1 x 10^4
	$ratio_Mult = ($database_size / 10);	# 10MB database is base
}

#For medium database with size exactly 1GB
if($bln_is_Medium_DB == 1)
{
	print "Medium size database ( equal to 1 GB) \n";
	$mult_Cust_Rows = 2000000;				# 2 x 10^6
	$mult_Ord_Rows = 100000;				# 1 x 10^5
	$mult_Prod_Rows = 100000;				# 1 x 10^5
	$ratio_Mult = ($database_size / 1);	 	# 1GB database is base
}

#For large database with size > 1GB
if($bln_is_Large_DB == 1)
{
	print "Large size database ( greater than 1 GB) \n";
	$mult_Cust_Rows = 200000000;			# 2 x 10^8
	$mult_Ord_Rows = 10000000;				# 1 x 10^7
	$mult_Prod_Rows = 1000000;				# 1 x 10^6
	$ratio_Mult = ($database_size / 100);	# 100GB database is base
}

print "Ratio calculated : $ratio_Mult \n";

#Calculate number of rows in table according to ratio
$i_Cust_Rows = ($mult_Cust_Rows * $ratio_Mult);
$i_Ord_Rows = ($mult_Ord_Rows * $ratio_Mult);
$i_Prod_Rows = ($mult_Prod_Rows * $ratio_Mult);

#Print number of rows for a check
print "Customer Rows: $i_Cust_Rows \n";
print "Order Rows / month: $i_Ord_Rows \n";
print "Product Rows: $i_Prod_Rows \n";

#***************************************************************************************

#Start data generation and dump data into CSV files
# CSV files will be converted into their windows or linux equivalent according to System type

print "\nCreating CSV files....\n";

#These are parameters to C Program
# Declaration and Initialization is done here for $par4 and $par_Sys_Type 

my $par1_Start = 0;        #start
my $par2_End = 0;          #end
my $par3_Fname = "";       #Name 
#Month Name used for Order table data generation
my @par3_ArrMonth = ("jan","feb","mar","apr","may","jun","jul","aug","sep","oct","nov","dec");  
my $par4_DB_Size = "";             #S|M|L 
my $par5_Month_Number = 0;			   #parameter used for determining month for Order table data generation(1 to 12)
my $par_Sys_Type = 0;      #0 (Linux) / 1 (Windows)
my $par_n_Prod = 0;		   # Number of Product table rows parameter for Product and Inv data generation
my $par_Max_Prod_Id = 0;	# Max Product Id = number of Product table rows  used in Order table data generation
my $par_Max_Cust_Id = 0; 	# Max Customer Id = number of Customer table rows used in Order table data generation

my $par_Pct_Member = 10;        # Percentage of users that are in the membership program

my $par_Avg_Reviews = 20;	# Average number of reviews per product

if($bln_is_Small_DB == 1)
{
	 $par4_DB_Size = $str_is_Small_DB;
}
elsif($bln_is_Medium_DB == 1) 
{
	$par4_DB_Size = $str_is_Medium_DB;	
}
elsif($bln_is_Large_DB == 1)
{
	 $par4_DB_Size = $str_is_Large_DB;
}

if($bln_is_Sys_Win == 1)
{
	$par_Sys_Type = 1;
}
elsif($bln_is_Sys_Linux == 1)
{
	$par_Sys_Type = 0;	
}


#***************************************************************************************

print "Starting to create CSV data files.... \n";
print "For larger database sizes, it will take time.\n";
print "Do not kill the script till execution is complete. \n";

#Create CSV files for Customer table
#Call already compiled C program in respective folders to generate CSV Files

#Move to cust folder
chdir "./data_files/cust/";

#Initialize parameters for Customer C Program
$par1_Start = 1;
$par2_End = ($i_Cust_Rows / 2);
$par3_Fname = "US"; 

print "\nCreating Customer CSV files!!! \n";

print "$par1_Start $par2_End $par3_Fname $par4_DB_Size $par_Sys_Type \n"; 

#Need to Call exe when System on which perl script is run is Windows
#We need to check which OS this perl script is being run
if(lc($^O) eq lc("linux"))   #If system on which perl script is executing is Linux
{
	system("./ds3_create_cust $par1_Start $par2_End $par3_Fname $par4_DB_Size $par_Sys_Type");		
}
else   #Windows
{
	if($bln_IsCygwin == 1)  #Run through Cygwin Bash Shell
	{
		system("./ds3_create_cust.exe $par1_Start $par2_End $par3_Fname $par4_DB_Size $par_Sys_Type");	
	}
	else	#Run through DOS shell
	{
		system("ds3_create_cust.exe $par1_Start $par2_End $par3_Fname $par4_DB_Size $par_Sys_Type");			
	}
}


$par1_Start = (($i_Cust_Rows / 2) + 1);
$par2_End = $i_Cust_Rows;
$par3_Fname = "ROW";

print "$par1_Start $par2_End $par3_Fname $par4_DB_Size $par_Sys_Type \n";

if(lc($^O) eq lc("linux"))   #If system on which perl script is executing is Linux
{
	system("./ds3_create_cust $par1_Start $par2_End $par3_Fname $par4_DB_Size $par_Sys_Type");		
}
else   #Windows
{
	if($bln_IsCygwin == 1)  #Run through Cygwin Bash Shell
	{
		system("./ds3_create_cust.exe $par1_Start $par2_End $par3_Fname $par4_DB_Size $par_Sys_Type");	
	}
	else	#Run through DOS shell
	{
		system("ds3_create_cust.exe $par1_Start $par2_End $par3_Fname $par4_DB_Size $par_Sys_Type");	
	}
	
}

print "\nCustomer CSV Files created!! \n";

#***************************************************************************************

#Create CSV files for Orders, Orderlines and Cust_Hist table
#Call already compiled C program in respective folders to generate CSV Files

#Move to Orders folder
chdir "../";
chdir "./orders/";

#use Cwd;
#my $dir = getcwd();
#print "Current Directory : $dir \n";

print "\nCreating Orders, Orderlines and Cust_Hist csv files!!! \n";

$par1_Start = 0; 
$par2_End = 0;   
$par3_Fname = "";
$par5_Month_Number = 0;
$par_Max_Prod_Id = $i_Prod_Rows;
$par_Max_Cust_Id = $i_Cust_Rows;

my $i_LoopCount = 0;

for($i_LoopCount = 0; $i_LoopCount < 12; $i_LoopCount++)
{	
	$par5_Month_Number = ($i_LoopCount + 1);
	$par1_Start = ($par2_End + 1);
	$par2_End = ($i_Ord_Rows * $par5_Month_Number);

	print "\nCreating Order CSV file for Month $par3_ArrMonth[$i_LoopCount] !!! \n";	
	
	print "$par1_Start $par2_End $par3_ArrMonth[$i_LoopCount] $par4_DB_Size $par5_Month_Number $par_Sys_Type $par_Max_Prod_Id $par_Max_Cust_Id \n";
	
	if(lc($^O) eq lc("linux"))   #If system on which perl script is executing is Linux
	{
		system("./ds3_create_orders $par1_Start $par2_End $par3_ArrMonth[$i_LoopCount] $par4_DB_Size $par5_Month_Number $par_Sys_Type $par_Max_Prod_Id $par_Max_Cust_Id");		
	}
	else   #Windows
	{
		if($bln_IsCygwin == 1)  #Run through Cygwin Bash Shell
		{
			system("./ds3_create_orders.exe $par1_Start $par2_End $par3_ArrMonth[$i_LoopCount] $par4_DB_Size $par5_Month_Number $par_Sys_Type $par_Max_Prod_Id $par_Max_Cust_Id");	
		}
		else  #Run through DOS Shell
		{
			system("ds3_create_orders.exe $par1_Start $par2_End $par3_ArrMonth[$i_LoopCount] $par4_DB_Size $par5_Month_Number $par_Sys_Type $par_Max_Prod_Id $par_Max_Cust_Id");				
		}
	}		
}

print "\nAll Order, Orderlines, Cust_Hist CSV files created !!! \n";

#***************************************************************************************

#Create CSV files for Products,Inv table
#Call already compiled C program in respective folders to generate CSV Files

$par_n_Prod = $i_Prod_Rows;   #Initialize to number of rows for Product table


#Create Inventory CSV file first since we are in orders folder now
print "\nCreating Inventory CSV file!!!! \n";

if(lc($^O) eq lc("linux"))   #If system on which perl script is executing is Linux
{
	system("./ds3_create_inv $par_n_Prod $par_Sys_Type");
     system("cp inv.csv ../prod/");		
}
else   #Windows
{
	if($bln_IsCygwin == 1)  #Run through Cygwin Bash Shell
	{
		system("./ds3_create_inv.exe $par_n_Prod $par_Sys_Type");
           system("cp inv.csv ../prod/");	
	}
	else  #Run through DOS Shell
	{
		system("ds3_create_inv.exe $par_n_Prod $par_Sys_Type");
     		system("copy /Y inv.csv ..\\prod\\");	
	}
}


print "\nInventory CSV file created!!!! \n";

#Move to prod folder for Product CSV file generation
chdir "../";
chdir "./prod/";

#use Cwd;
#my $dir = getcwd();
#print "Current Directory : $dir \n";

#Create Product CSV file first since we are in prod folder now
print "\nCreating product CSV file!!!! \n";

if(lc($^O) eq lc("linux"))   #If system on which perl script is executing is Linux
{
	system("./ds3_create_prod $par_n_Prod $par_Sys_Type");		
}
else   #Windows
{
	if($bln_IsCygwin == 1)  #Run through Cygwin Bash Shell
	{
		system("./ds3_create_prod.exe $par_n_Prod $par_Sys_Type");	
	}
	else   #Run through DOS Shell
	{
		system("ds3_create_prod.exe $par_n_Prod $par_Sys_Type");	
	}
}

print "\nProduct CSV file created!!!! \n";

#***************************************************************************************

# Create Membership data file

chdir "../";
chdir "./membership/";

print "\nCreating membership CSV file!!!! \n";

if(lc($^O) eq lc("linux"))   #If system on which perl script is executing is Linux
{
        system("./ds3_create_membership $i_Cust_Rows $par_Pct_Member $par_Sys_Type");
}
else   #Windows
{
        if($bln_IsCygwin == 1)  #Run through Cygwin Bash Shell
        {
                system("./ds3_create_membership.exe $i_Cust_Rows $par_Pct_Member $par_Sys_Type");
        }
        else   #Run through DOS Shell
        {
                system("ds3_create_membership.exe $i_Cust_Rows $par_Pct_Member $par_Sys_Type");
        }
}

print "\nMembership CSV file created!!!! \n";


#*****************************************************************************************
#Create Reviews and Reviews Helpfulness data file

chdir "../";
chdir "./reviews/";

print "\nCreating reviews and reviews helpfulness CSV files!!!! \n";

my $par_review_rows = $par_n_Prod * $par_Avg_Reviews; 

if(lc($^O) eq lc("linux"))   #If system on which perl script is executing is Linux
{
        system("./ds3_create_reviews $par_n_Prod $par_Avg_Reviews $i_Cust_Rows $par_Sys_Type");
}
else   #Windows
{
        if($bln_IsCygwin == 1)  #Run through Cygwin Bash Shell
        {
                system("./ds3_create_reviews.exe $par_n_Prod $par_Avg_Reviews $i_Cust_Rows $par_Sys_Type");
        }
        else   #Run through DOS Shell
        {
                system("ds3_create_reviews.exe $par_n_Prod $par_Avg_Reviews $i_Cust_Rows $par_Sys_Type");
        }
}

print "\nReviews and heviews helpfulness CSV files created!!!! \n";



#Now move to required folders according to Database Type
my @lines;
my $line;
my $ord_row = ($i_Ord_Rows * 12);
my $cust_row_plus_one;
$cust_row_plus_one = ($i_Cust_Rows + 1);

if($bln_is_DB_MYSQL == 1)			#For MySQL
{
	chdir "../../mysqlds3/";			#Move to mysql directory
	chdir "./build/";					#Move to build directory inside mysql directory
	
	#Open a template file and replace placeholders in it and write new file

	print "\nCreating build script for MySQL from templates... \n";
	print "\nTemplate files are stored in respective build folders and the output files are also stored in same folder \n";
	print "\nTemplate files are named with generic_template at end of their filename and the output files without _template at end \n";
	
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "mysqlds3_cleanup_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	foreach $line (@lines)
	{
		$line =~ s/{CUST_ROW}/$i_Cust_Rows/g;
		$line =~ s/{ORD_ROW}/$ord_row/g;
	}	
	$str_file_name = "mysqlds3_cleanup_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">" , $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	print "\nCompleted creating and writing build scripts for MySQL database... \n";
	
}
elsif($bln_is_DB_PGSQL == 1)			#For PGSQL
{
	chdir "../../pgsqlds2/";			#Move to postgres directory
	chdir "./build/";					#Move to build directory inside postgres directory
	
	#Open a template file and replace placeholders in it and write new file

	print "\nCreating build script for PostgreSQL from templates... \n";
	print "\nTemplate files are stored in respective build folders and the output files are also stored in same folder \n";
	print "\nTemplate files are named with generic_template at end of their filename and the output files without _template at end \n";
	
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "pgsqlds2_cleanup_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	foreach $line (@lines)
	{
		$line =~ s/{CUST_ROW}/$i_Cust_Rows/g;
		$line =~ s/{ORD_ROW}/$ord_row/g;
	}	
	$str_file_name = "pgsqlds2_cleanup_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">" , $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	print "\nCompleted creating and writing build scripts for PostgreSQL database... \n";
	
}
elsif($bln_is_DB_ORACLE == 1) 		#For Oracle
{
	
	chdir "../../oracleds3/";		#Move to oracle directory
	chdir "./build/";				#Move to build directory

	print "\nStarted creating and writing build scripts for Oracle database... \n";
	
	#Create cleanup sql script (with foreign key disabled) from template
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "oracleds3_cleanup_generic_fk_disabled_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	foreach $line (@lines)
	{
		$line =~ s/{CUST_ROW}/$i_Cust_Rows/g;         
		$line =~ s/{CUST_ROW_PLUS_ONE}/$cust_row_plus_one/g;	
	}	
	$str_file_name = "oracleds3_cleanup_".$database_size.$database_size_str."_fk_disabled.sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	#Create cleanup sql script (without foreign key disabled) from template
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "oracleds3_cleanup_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	foreach $line (@lines)
	{
		$line =~ s/{CUST_ROW}/$i_Cust_Rows/g;         
		$line =~ s/{CUST_ROW_PLUS_ONE}/$cust_row_plus_one/g;	
	}	
	$str_file_name = "oracleds3_cleanup_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	#Create cleanup shell script (without foreign key disabled) from template
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "oracleds3_cleanup_generic_template.sh") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	$str_file_name = "oracleds3_cleanup_".$database_size.$database_size_str.".sql";
	foreach $line (@lines)
	{
		$line =~ s/{SQL_FNAME}/$str_file_name/g;	
	}		
	$str_file_name = "";
	$str_file_name = "oracleds3_cleanup_".$database_size.$database_size_str.".sh";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	#Create cleanup sql script (with foreign key disabled) from template
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "oracleds3_cleanup_generic_fk_disabled_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	foreach $line (@lines)
	{
		$line =~ s/{CUST_ROW}/$i_Cust_Rows/g;         
		$line =~ s/{CUST_ROW_PLUS_ONE}/$cust_row_plus_one/g;	
	}	
	$str_file_name = "oracleds3_cleanup_".$database_size.$database_size_str."_fk_disabled.sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	#Create cleanup shell script (with foreign key disabled) from template
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "oracleds3_cleanup_generic_fk_disabled_template.sh") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	$str_file_name = "oracleds3_cleanup_".$database_size.$database_size_str."_fk_disabled.sql";
	foreach $line (@lines)
	{
		$line =~ s/{SQL_FNAME}/$str_file_name/g;         			
	}	
	$str_file_name = "";
	$str_file_name = "oracleds3_cleanup_".$database_size.$database_size_str."_fk_disabled.sh";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	
	#Create create db sql script from template
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "oracleds3_create_db_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	foreach $line (@lines)
	{
		$line =~ s/{CUST_ROW_PLUS_ONE}/$cust_row_plus_one/g;         			
	}	
	$str_file_name = "oracleds3_create_db_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	#TODO : Need to create table space script which will depend on database size since number of database files will vary as DBsize varies
	#Create create tablespace sql script from template
	@lines = ();
	$line = "";
	$str_file_name = "";
	open (FILE, "oracleds3_create_tablespaces_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);	
	my $i_Cnt = 0;
	foreach $line (@lines)
	{
		if($line =~ m/{CUST1DATAFILE_PATH}/)
		{
			$line =~ s/{CUST1DATAFILE_PATH}/$arr_db_file_paths[0]/g;						
		}			         			
		if($line =~ m/{CUST2DATAFILE_PATH}/)
		{
			$line =~ s/{CUST2DATAFILE_PATH}/$arr_db_file_paths[0]/g;						
		}	
		if($line =~ m/{IND1DATAFILE_PATH}/)
		{
			$line =~ s/{IND1DATAFILE_PATH}/$arr_db_file_paths[1]/g;						
		}	
		if($line =~ m/{IND2DATAFILE_PATH}/)
		{
			$line =~ s/{IND2DATAFILE_PATH}/$arr_db_file_paths[1]/g;						
		}	
		 if($line =~ m/{MEMBERDATAFILE_PATH}/)
                {
                        $line =~ s/{MEMBERDATAFILE_PATH}/$arr_db_file_paths[2]/g;
                }
                if($line =~ m/{REVIEW1DATAFILE_PATH}/)
                {
                        $line =~ s/{REVIEW1DATAFILE_PATH}/$arr_db_file_paths[3]/g;
                }
                if($line =~ m/{REVIEW2DATAFILE_PATH}/)
                {
                        $line =~ s/{REVIEW2DATAFILE_PATH}/$arr_db_file_paths[3]/g;
                }
		if($line =~ m/{DS_MISCDATAFILE_PATH}/)
		{
			$line =~ s/{DS_MISCDATAFILE_PATH}/$arr_db_file_paths[4]/g;						
		}	
		if($line =~ m/{ORDER1DATAFILE_PATH}/)
		{
			$line =~ s/{ORDER1DATAFILE_PATH}/$arr_db_file_paths[5]/g;						
		}	
		if($line =~ m/{ORDER2DATAFILE_PATH}/)
		{
			$line =~ s/{ORDER2DATAFILE_PATH}/$arr_db_file_paths[5]/g;						
		}	
	}	
	$str_file_name = "oracleds3_create_tablespaces_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
		
	
	chdir "../";		#Move to oracle directory to finally edit master shell script
	
	#Create new create_all shell script file from template
	@lines = ();
	$line = "";	
	$str_file_name = "";
	open (FILE, "oracleds3_create_all_generic_template.sh") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	foreach $line (@lines)
	{
		$str_file_name = "oracleds3_create_tablespaces_".$database_size.$database_size_str.".sql";
		$line =~ s/{TBLSPACE_SQLFNAME}/$str_file_name/g;  
		$str_file_name = "oracleds3_create_db_".$database_size.$database_size_str.".sql";     
		$line =~ s/{CREATEDB_SQLFNAME}/$str_file_name/g;		   		   	
	}	
	$str_file_name = "";
	$str_file_name = "oracleds3_create_all_".$database_size.$database_size_str.".sh";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	print "\nCompleted creating and writing build scripts for Oracle database!!\n";
}
elsif($bln_is_DB_MSSQL == 1) 		#For SQL Server
{
	print "\nStarted creating and writing build scripts for SQL Server database...\n";
	
	chdir "../../sqlserverds3/";		#Move to mssql directory
	
	#Create new create_all sql script file from template
	@lines = ();
	$line = "";	
	$str_file_name = "";
	open (FILE, "sqlserverds3_create_all_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	my $i_Cnt = 0;
	my $i_arrCnt = scalar(@arr_db_file_paths);
	my $i_LastIndex = ($i_arrCnt - 1);
	foreach $line (@lines)
	{
		if($line =~ m/{DATAFILE_PATH}/)
		{
			#$arr_db_file_paths[$i_Cnt] =~ s/\\//g;    #Replace all backslashes if exists
			$line =~ s/{DATAFILE_PATH}/$arr_db_file_paths[$i_Cnt]/g;			
			$i_Cnt = ($i_Cnt + 1);				
		}			   
		if($line =~ m/{FULLTEXTCAT_PATH}/)
		{
			$line =~ s/{FULLTEXTCAT_PATH}/$arr_db_file_paths[$i_LastIndex]/g;
		}
		if($line =~ m/{DRIVELETTER}/)
		{
			$line =~ s/{DRIVELETTER}/$str_driveletter/g;
		}            			
	}	
	$str_file_name = "sqlserverds3_create_all_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	chdir "./build/";				#Move to build directory
	
	#Create new cleanup sql script file from template
	@lines = ();
	$line = "";	
	$str_file_name = "";
	open (FILE, "sqlserverds3_cleanup_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	foreach $line (@lines)
	{
		$line =~ s/{CUST_ROW}/$i_Cust_Rows/g;
		$line =~ s/{ORD_ROW}/$ord_row/g;
		$line =~ s/{DRIVELETTER}/$str_driveletter/g;
		$line =~ s/{REVIEW_ROW}/$par_review_rows/g;		   		   	
	}	
	$str_file_name = "sqlserverds3_cleanup_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	#Create new create_ind sql script from template
	@lines = ();
	$line = "";	
	$str_file_name = "";
	open (FILE, "sqlserverds3_create_ind_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	my $i_Cnt = 0;
	my $i_arrCnt = scalar(@arr_db_file_paths);
	my $i_LastIndex = ($i_arrCnt - 1);
	foreach $line (@lines)
	{				   
		if($line =~ m/{FULLTEXTCAT_PATH}/)
		{
			#$arr_db_file_paths[$i_Cnt] =~ s/\\//g;    #Replace all backslashes if exists
			$line =~ s/{FULLTEXTCAT_PATH}/$arr_db_file_paths[$i_LastIndex]/g;
		}      			
	}	
	$str_file_name = "sqlserverds3_create_ind_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	#Create new create_db sql script file from template
	@lines = ();
	$line = "";	
	$str_file_name = "";
	open (FILE, "sqlserverds3_create_db_generic_template.sql") || die "Can not Open file : $!";	
	@lines =  <FILE>;
	close (FILE);
	my $i_Cnt = 0;
	my $i_arrCnt = scalar(@arr_db_file_paths);
	my $i_LastIndex = ($i_arrCnt - 1);
	foreach $line (@lines)
	{
		if($line =~ m/{DATAFILE_PATH}/)
		{
			#$arr_db_file_paths[$i_Cnt] =~ s/\\//g;    #Replace all backslashes if exists
			$line =~ s/{DATAFILE_PATH}/$arr_db_file_paths[$i_Cnt]/g;			
			$i_Cnt = ($i_Cnt + 1);				
		}			   		    			
	}	
	$str_file_name = "sqlserverds3_create_db_".$database_size.$database_size_str.".sql";
	open (NEWFILE, ">", $str_file_name) || die "Creating new file to write failed : $!";
	print NEWFILE @lines;
	close (NEWFILE);
	
	print "\nCompleted creating and writing build scripts for SQL Server database!!\n";
}

print "\nAll database build scripts(shell and sql) are dumped into their respective folders. \n";
print "\nThese scripts are created from template files in same folders with '_generic_template' in their name. \n";
print "\nScripts that are created from template files have '_' $database_size $database_size_str in their name. \n";
print "\nUser can edit the sql script generated for customizing sql script for more DBFiles per table and change the paths of DBFiles.\n";
print "\nNow Run CreateConfigFile.pl perl script in ds3 folder which will generate configuration file used as input to the driver program.\n"
