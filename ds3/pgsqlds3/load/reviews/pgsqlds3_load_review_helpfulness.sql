ALTER TABLE REVIEWS_HELPFULNESS DISABLE TRIGGER ALL;

\COPY REVIEWS_HELPFULNESS FROM '../../../data_files/reviews/review_helpfulness.csv' WITH DELIMITER ','

ALTER TABLE REVIEWS_HELPFULNESS ENABLE TRIGGER ALL;