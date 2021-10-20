ALTER TABLE CUST_HIST DISABLE TRIGGER ALL;

\COPY CUST_HIST FROM '../../../data_files/orders/jan_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/feb_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/mar_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/apr_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/may_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/jun_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/jul_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/aug_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/sep_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/oct_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/nov_cust_hist.csv' WITH DELIMITER ',' 
\COPY CUST_HIST FROM '../../../data_files/orders/dec_cust_hist.csv' WITH DELIMITER ',' 

ALTER TABLE CUST_HIST ENABLE TRIGGER ALL;
