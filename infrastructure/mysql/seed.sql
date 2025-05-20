-- Campus Love App - Seed Data
-- This script inserts sample data into the database without truncating existing data

USE campuslove_db;

-- Insert data into reference tables using INSERT IGNORE to avoid duplicates
-- 1. Genders
INSERT IGNORE INTO Genders (GenderName) VALUES 
('Male'),
('Female'),
('Non-binary'),
('Gender fluid'),
('Agender');

-- Get the GenderIDs for reference
SET @male_id = (SELECT GenderID FROM Genders WHERE GenderName = 'Male');
SET @female_id = (SELECT GenderID FROM Genders WHERE GenderName = 'Female');
SET @nonbinary_id = (SELECT GenderID FROM Genders WHERE GenderName = 'Non-binary');
SET @genderfluid_id = (SELECT GenderID FROM Genders WHERE GenderName = 'Gender fluid');
SET @agender_id = (SELECT GenderID FROM Genders WHERE GenderName = 'Agender');

-- 2. SexualOrientations
INSERT IGNORE INTO SexualOrientations (OrientationName) VALUES 
('Heterosexual'),
('Homosexual'),
('Bisexual'),
('Pansexual'),
('Asexual');

-- Get the OrientationIDs for reference
SET @heterosexual_id = (SELECT OrientationID FROM SexualOrientations WHERE OrientationName = 'Heterosexual');
SET @homosexual_id = (SELECT OrientationID FROM SexualOrientations WHERE OrientationName = 'Homosexual');
SET @bisexual_id = (SELECT OrientationID FROM SexualOrientations WHERE OrientationName = 'Bisexual');
SET @pansexual_id = (SELECT OrientationID FROM SexualOrientations WHERE OrientationName = 'Pansexual');
SET @asexual_id = (SELECT OrientationID FROM SexualOrientations WHERE OrientationName = 'Asexual');

-- 3. Careers
INSERT IGNORE INTO Careers (CareerName) VALUES 
('Computer Science'),
('Engineering'),
('Medicine'),
('Law'),
('Business Administration'),
('Psychology'),
('Communications'),
('Architecture'),
('Fine Arts'),
('Mathematics');

-- Get the CareerIDs for reference
SET @cs_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Computer Science');
SET @engineering_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Engineering');
SET @medicine_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Medicine');
SET @law_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Law');
SET @business_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Business Administration');
SET @psychology_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Psychology');
SET @communications_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Communications');
SET @architecture_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Architecture');
SET @finearts_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Fine Arts');
SET @mathematics_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Mathematics');

-- 4. Interests
INSERT IGNORE INTO Interests (InterestName) VALUES 
('Reading'),
('Movies'),
('Sports'),
('Music'),
('Gaming'),
('Cooking'),
('Travel'),
('Photography'),
('Dancing'),
('Art'),
('Technology'),
('Fitness'),
('Politics'),
('Science'),
('Nature');

-- 5. Locations
-- Countries
INSERT IGNORE INTO Countries (CountryName) VALUES 
('Mexico'),
('United States'),
('Canada'),
('Spain'),
('Argentina');

-- Get the CountryIDs for reference
SET @mexico_id = (SELECT CountryID FROM Countries WHERE CountryName = 'Mexico');
SET @us_id = (SELECT CountryID FROM Countries WHERE CountryName = 'United States');
SET @canada_id = (SELECT CountryID FROM Countries WHERE CountryName = 'Canada');
SET @spain_id = (SELECT CountryID FROM Countries WHERE CountryName = 'Spain');
SET @argentina_id = (SELECT CountryID FROM Countries WHERE CountryName = 'Argentina');

-- Regions - using the correct CountryIDs
INSERT IGNORE INTO Regions (RegionName, CountryID) VALUES 
('Ciudad de México', @mexico_id),
('Jalisco', @mexico_id),
('Nuevo León', @mexico_id),
('California', @us_id),
('Texas', @us_id),
('New York', @us_id),
('Ontario', @canada_id),
('Quebec', @canada_id),
('Madrid', @spain_id),
('Barcelona', @spain_id),
('Buenos Aires', @argentina_id);

-- Get the RegionIDs for reference
SET @mexico_city_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Ciudad de México' AND CountryID = @mexico_id);
SET @guadalajara_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Jalisco' AND CountryID = @mexico_id);
SET @monterrey_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Nuevo León' AND CountryID = @mexico_id);
SET @california_id = (SELECT RegionID FROM Regions WHERE RegionName = 'California' AND CountryID = @us_id);
SET @texas_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Texas' AND CountryID = @us_id);
SET @newyork_id = (SELECT RegionID FROM Regions WHERE RegionName = 'New York' AND CountryID = @us_id);
SET @ontario_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Ontario' AND CountryID = @canada_id);
SET @quebec_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Quebec' AND CountryID = @canada_id);
SET @madrid_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Madrid' AND CountryID = @spain_id);
SET @barcelona_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Barcelona' AND CountryID = @spain_id);
SET @buenosaires_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Buenos Aires' AND CountryID = @argentina_id);

-- Cities - using the correct RegionIDs
INSERT IGNORE INTO Cities (CityName, RegionID) VALUES 
('Mexico City', @mexico_city_id),
('Guadalajara', @guadalajara_id),
('Monterrey', @monterrey_id),
('Los Angeles', @california_id),
('San Francisco', @california_id),
('Dallas', @texas_id),
('Houston', @texas_id),
('New York City', @newyork_id),
('Toronto', @ontario_id),
('Montreal', @quebec_id),
('Madrid', @madrid_id),
('Barcelona', @barcelona_id),
('Buenos Aires', @buenosaires_id);

-- Get the CityIDs for reference
SET @mexico_city_city_id = (SELECT CityID FROM Cities WHERE CityName = 'Mexico City' AND RegionID = @mexico_city_id);
SET @guadalajara_city_id = (SELECT CityID FROM Cities WHERE CityName = 'Guadalajara' AND RegionID = @guadalajara_id);
SET @monterrey_city_id = (SELECT CityID FROM Cities WHERE CityName = 'Monterrey' AND RegionID = @monterrey_id);
SET @la_id = (SELECT CityID FROM Cities WHERE CityName = 'Los Angeles' AND RegionID = @california_id);
SET @sf_id = (SELECT CityID FROM Cities WHERE CityName = 'San Francisco' AND RegionID = @california_id);
SET @dallas_id = (SELECT CityID FROM Cities WHERE CityName = 'Dallas' AND RegionID = @texas_id);
SET @houston_id = (SELECT CityID FROM Cities WHERE CityName = 'Houston' AND RegionID = @texas_id);
SET @nyc_id = (SELECT CityID FROM Cities WHERE CityName = 'New York City' AND RegionID = @newyork_id);
SET @toronto_id = (SELECT CityID FROM Cities WHERE CityName = 'Toronto' AND RegionID = @ontario_id);
SET @montreal_id = (SELECT CityID FROM Cities WHERE CityName = 'Montreal' AND RegionID = @quebec_id);
SET @madrid_city_id = (SELECT CityID FROM Cities WHERE CityName = 'Madrid' AND RegionID = @madrid_id);
SET @barcelona_city_id = (SELECT CityID FROM Cities WHERE CityName = 'Barcelona' AND RegionID = @barcelona_id);
SET @buenosaires_city_id = (SELECT CityID FROM Cities WHERE CityName = 'Buenos Aires' AND RegionID = @buenosaires_id);

-- 6. Users - using the correct GenderIDs, CareerIDs, OrientationIDs, and CityIDs
INSERT IGNORE INTO Users (UserID, FullName, Age, GenderID, CareerID, OrientationID, ProfilePhrase, MinPreferredAge, MaxPreferredAge, IsVerified, CityID) VALUES 
(1, 'Laura Martinez', 23, @female_id, @medicine_id, @heterosexual_id, 'Future doctor looking for someone to share coffee breaks with', 22, 30, TRUE, @la_id),
(2, 'Carlos Rodriguez', 25, @male_id, @engineering_id, @heterosexual_id, 'Tech enthusiast with a passion for innovation', 23, 28, TRUE, @sf_id),
(3, 'Ana Garcia', 22, @female_id, @psychology_id, @heterosexual_id, 'Words are my playground', 21, 27, TRUE, @guadalajara_city_id),
(4, 'Juan Perez', 28, @male_id, @law_id, @heterosexual_id, 'Creating business opportunities wherever I go', 24, 30, FALSE, @dallas_id),
(5, 'Sofia Gutierrez', 24, @female_id, @psychology_id, @heterosexual_id, 'Understanding the mind, one session at a time', 22, 32, TRUE, @toronto_id),
(6, 'Miguel Torres', 26, @male_id, @architecture_id, @heterosexual_id, 'Building the future, one structure at a time', 22, 30, FALSE, @barcelona_city_id),
(7, 'Isabella Morales', 21, @female_id, @finearts_id, @heterosexual_id, 'Art is how I express what words cannot say', 20, 28, TRUE, @madrid_city_id),
(8, 'Alejandro Diaz', 29, @male_id, @law_id, @heterosexual_id, 'Justice for all is my motto', 25, 35, TRUE, @buenosaires_city_id),
(9, 'Valentina Lopez', 23, @female_id, @architecture_id, @heterosexual_id, 'Designing spaces that inspire', 21, 29, FALSE, @montreal_id),
(10, 'Daniel Silva', 27, @male_id, @mathematics_id, @heterosexual_id, 'Numbers tell the most fascinating stories', 24, 32, TRUE, @nyc_id);

-- 7. User accounts (password is "password123" for all)
INSERT IGNORE INTO UserAccounts (UserID, Email, Username, PasswordHash, IsActive) VALUES 
(1, 'laura.martinez@email.com', 'lauramtz', '$2a$11$k7R32JaFADxMA2Xt9uOUwuZfeTXSEKZrQVWAgj/zlHyZO0pC36zfC', TRUE),
(2, 'carlos.rodriguez@email.com', 'carlosr', '$2a$11$zFa5cT/3SJ1qveQVM9RRke1.nsKjUYVeGNQ0GGFZoXwTiw9WcL0BC', TRUE),
(3, 'ana.garcia@email.com', 'anagarcia', '$2a$11$eqYDFBA5ILQmrO/zTrxHwOFytKTw8vs0RaHDI3NUYvnRwFcwMjCoa', TRUE),
(4, 'juan.perez@email.com', 'juanp', '$2a$11$YwZS2ADhh2KX4CgCY/Ao4.2uhJ7eQJATq1PL2R.S1QcYkCW1wJqLy', TRUE),
(5, 'sofia.gutierrez@email.com', 'sofiag', '$2a$11$w2.KWw6dHWT36H.MUvBig.GKZH8nwz95/ECjFGNJZ2zHEwZnuPUVK', TRUE),
(6, 'miguel.torres@email.com', 'miguelt', '$2a$11$QCLuJKF3QREcOI8wrYLB1.FRpm.E6o/D6SDGkuRdNFvgYP5KXj0Q6', TRUE),
(7, 'isabella.morales@email.com', 'isabellam', '$2a$11$YY6MOsRukt2WUaz5YY.TH.FS90EupR5A5UZxW5D0g0FtRXL0056Uu', TRUE),
(8, 'alejandro.diaz@email.com', 'alejandrod', '$2a$11$kmJMCtxU5srOvAw0G0UGqeN412OFKBPzsT91hZRFMoeBJYC0Z9016', TRUE),
(9, 'valentina.lopez@email.com', 'valentinal', '$2a$11$kN9s/7.7sISv/C4xYf5LG.XeGIEFNd1jZcO.bCGsY4ZF55V5y8ZRu', TRUE),
(10, 'daniel.silva@email.com', 'daniels', '$2a$11$30qf8Yt93KHIDODKvUP2EuxC9yK8VsBqT9NAL6J0wgzjLvLjbT5OG', TRUE);

-- 8. User interests
INSERT IGNORE INTO UserInterests (UserID, InterestID) VALUES 
(1, 1), (1, 4), (1, 12),  -- Laura: Reading, Movies, Fitness
(2, 1), (2, 5), (2, 11),  -- Carlos: Reading, Gaming, Technology
(3, 1), (3, 8), (3, 10),  -- Ana: Reading, Photography, Art
(4, 4), (4, 6), (4, 13),  -- Juan: Movies, Cooking, Politics
(5, 1), (5, 2), (5, 14),  -- Sofia: Reading, Movies, Science
(6, 3), (6, 5), (6, 11),  -- Miguel: Sports, Gaming, Technology
(7, 2), (7, 9), (7, 10),  -- Isabella: Movies, Dancing, Art
(8, 1), (8, 13), (8, 15),  -- Alejandro: Reading, Politics, Nature
(9, 7), (9, 8), (9, 10),  -- Valentina: Travel, Photography, Art
(10, 5), (10, 11), (10, 14);  -- Daniel: Gaming, Technology, Science

-- 9. Interactions (Likes/Dislikes)
INSERT IGNORE INTO Interactions (FromUserID, ToUserID, InteractionType, InteractionDate) VALUES 
-- Laura and Carlos like each other
(1, 2, 'LIKE', DATE_SUB(NOW(), INTERVAL 10 DAY)),
(2, 1, 'LIKE', DATE_SUB(NOW(), INTERVAL 9 DAY)),
-- Juan and Ana like each other
(4, 3, 'LIKE', DATE_SUB(NOW(), INTERVAL 7 DAY)),
(3, 4, 'LIKE', DATE_SUB(NOW(), INTERVAL 6 DAY)),
-- Sofia and Miguel like each other
(5, 6, 'LIKE', DATE_SUB(NOW(), INTERVAL 5 DAY)),
(6, 5, 'LIKE', DATE_SUB(NOW(), INTERVAL 4 DAY)),
-- Other interactions without matches
(1, 4, 'DISLIKE', DATE_SUB(NOW(), INTERVAL 8 DAY)),
(3, 2, 'DISLIKE', DATE_SUB(NOW(), INTERVAL 3 DAY)),
(5, 9, 'DISLIKE', DATE_SUB(NOW(), INTERVAL 2 DAY)),
(9, 10, 'DISLIKE', DATE_SUB(NOW(), INTERVAL 1 DAY)),
(10, 5, 'DISLIKE', NOW()),
(7, 8, 'DISLIKE', DATE_SUB(NOW(), INTERVAL 2 DAY)),
(8, 7, 'DISLIKE', DATE_SUB(NOW(), INTERVAL 1 DAY));

-- 10. Matches (based on mutual likes)
INSERT IGNORE INTO Matches (User1ID, User2ID, MatchDate) VALUES 
(1, 2, DATE_SUB(NOW(), INTERVAL 9 DAY)),
(3, 4, DATE_SUB(NOW(), INTERVAL 6 DAY)),
(5, 6, DATE_SUB(NOW(), INTERVAL 4 DAY)),
(9, 10, NOW());

-- 11. DailyCredits
INSERT IGNORE INTO DailyCredits (UserID, CreditDate, LikesUsed) VALUES 
(1, CURRENT_DATE, 3),
(2, CURRENT_DATE, 1),
(3, CURRENT_DATE, 2),
(4, CURRENT_DATE, 1),
(5, CURRENT_DATE, 2),
(6, CURRENT_DATE, 1),
(7, CURRENT_DATE, 1),
(8, CURRENT_DATE, 1),
(9, CURRENT_DATE, 1),
(10, CURRENT_DATE, 2);

-- Confirmation
SELECT 'Sample data successfully inserted' AS Message;
