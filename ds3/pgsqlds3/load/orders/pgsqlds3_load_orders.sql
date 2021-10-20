ALTER TABLE ORDERS DISABLE TRIGGER ALL;

\COPY ORDERS FROM '../../../data_files/orders/jan_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/feb_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/mar_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/apr_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/may_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/jun_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/jul_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/aug_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/sep_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/oct_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/nov_orders.csv' WITH DELIMITER ',' 
\COPY ORDERS FROM '../../../data_files/orders/dec_orders.csv' WITH DELIMITER ',' 

ALTER TABLE ORDERS ENABLE TRIGGER ALL;
