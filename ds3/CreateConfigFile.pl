#!/usr/bin/perl
use strict;
#Perl script created by GSK 
#Last updated: 6/14/2010

#Purpose of this perl script:
#			This perl script will create a text file DriverConfig.txt.
#			This text file can be used to pass parameters to driver program through text file.
#Prerequisites for Perl script:
#			To run this perl script on windows machines, user needs to install cygwin with perl on windows machine.
#			To run this perl script on linux machines, user just needs to install perl package on linux machine.
#			To understand how to use this perl script and what parameter values should be given to this perl script,
#			please go through section 6 of documentation ds2.1_Documentation.txt in /ds2 folder


#Config File will be created in /ds2 folder
#This config file will be used for executing Driver to drive workload against database server

my $target_host = "localhost";			#Database/web server hostname or IP Address  Default = localhost
my $database_size = "10MB";				#Database Size Default = 10mb  (e.g. 30MB, 80GB)
my $n_threads = 1;						#number of driver threads against one DB Server
my $ramp_rate = 10;						#startup rate (users/sec) default = 10
my $run_time = 0;						#run time (min) - Default = 0 is infinite
my $warmup_time = 1;					#warmup_time (min) default = 1
my $think_time = 0;						#think time (sec) default = 0
my $pct_newcustomers = 20;				#percent of customers that are new customers default = 20
my $n_searches = 3; 					#average number of searches per order default = 3
my $search_batch_size = 5;				#average number of items returned in each search default = 5
my $n_line_items = 5;					#average number of items per order default = 5
my $virt_dir = "ds2";					#virtual directory (for web driver) default = ds2
my $page_type = "php";					#web page type (for web driver) default = php
my $windows_perf_host = "";				#target hostname for Perfmon CPU% display (Windows only)
my $detailed_view = "N";				#Parameter to display detailed view of Runtime Statistics on Each target machine default = N
my $linux_perf_host = "";				#Parameter for linux CPU utilization Required format for value: <username>:<password>:<IP Address>

my $line = "";
my $end_line = "";						#End of line character


print "Please enter following parameters: \n";
print "***********************************\n";
print "Please enter target host(s) (database/web server hostname or IP Address) : "; 
chomp($target_host = <STDIN>);
print "Please enter database size (e.g. Input can be like 30MB, 80GB ,etc) : "; 
chomp($database_size = <STDIN>);
print "Please enter target hostname for perfmon CPU% display (windows only) : "; 
chomp($windows_perf_host = <STDIN>);
print "Please enter <username>:<password>:<IP Address> for linux machines for CPU % display (linux only) : ";
chomp($linux_perf_host = <STDIN>);
print "Please enter if you want detailed view of runtime statistics of each target machine ( Y / N): ";
chomp($detailed_view = <STDIN>);

print "***********************************\n";

if(lc($^O) eq lc("linux")) #If system on which perl script is executing is Linux
{
	$end_line = "\n";
}
else
{
	$end_line = "\r\n";
}

print "Creating config file: DriverConfig.txt to be used for Driver Program input parameters....\n";

#New file will be DriverConfig.txt and will be in /ds2 folder

open (FILE, ">DriverConfig.txt") || die "Creating new Config file to write failed : $!";  #Create new empty file
close (FILE);

open (NEWFILE, ">>DriverConfig.txt") || die "Creating new Config file to write failed : $!";

$line = "target=".$target_host;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "n_threads=".$n_threads;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "ramp_rate=".$ramp_rate;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "run_time=".$run_time;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "db_size=".$database_size;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "warmup_time=".$warmup_time;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "think_time=".$think_time;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "pct_newcustomers=".$pct_newcustomers;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "n_searches=".$n_searches;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "search_batch_size=".$search_batch_size;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "n_line_items=".$n_line_items;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "virt_dir=".$virt_dir;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "page_type=".$page_type;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "windows_perf_host=".$windows_perf_host;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "linux_perf_host=".$linux_perf_host;
print NEWFILE $line;
print NEWFILE $end_line;
$line = "detailed_view=".$detailed_view;
print NEWFILE $line;
print NEWFILE $end_line;

close (NEWFILE);

print "Completed creating config file: DriverConfig.txt to be used for Driver Program input parameters....\n";
print "Configuration file DriverConfig.txt is saved under ds2 folder. \n";
print "Edit DriverConfig.txt for input parameters like n_threads, ramp_rate, run_time, warmup_time, think_time, etc....\n";
print "Then Run the driver program from command prompt as follows: ds2webdriver.exe --config_file=<path of config file> \n";


