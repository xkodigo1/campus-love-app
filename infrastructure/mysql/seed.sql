-- Campus Love App - Seed Data
-- This script inserts sample data into the database without truncating existing data

USE campuslove_db;

-- Insert data into reference tables using INSERT IGNORE to avoid duplicates
-- 1. Genders
INSERT IGNORE INTO Genders (GenderName) VALUES 
('Male'),
('Female'),
('Non-binary'),
('Other');

-- Get the GenderIDs for reference
SET @male_id = (SELECT GenderID FROM Genders WHERE GenderName = 'Male');
SET @female_id = (SELECT GenderID FROM Genders WHERE GenderName = 'Female');
SET @nonbinary_id = (SELECT GenderID FROM Genders WHERE GenderName = 'Non-binary');

-- 2. SexualOrientations
INSERT IGNORE INTO SexualOrientations (OrientationName) VALUES 
('Straight'),
('Gay'),
('Bisexual'),
('Pansexual'),
('Asexual');

-- Get the OrientationIDs for reference
SET @straight_id = (SELECT OrientationID FROM SexualOrientations WHERE OrientationName = 'Straight');
SET @gay_id = (SELECT OrientationID FROM SexualOrientations WHERE OrientationName = 'Gay');
SET @bi_id = (SELECT OrientationID FROM SexualOrientations WHERE OrientationName = 'Bisexual');

-- 3. Careers
INSERT IGNORE INTO Careers (CareerName) VALUES 
('Computer Science'),
('Business Administration'),
('Medicine'),
('Law'),
('Psychology'),
('Architecture'),
('Graphic Design'),
('Marketing'),
('Biology'),
('Mathematics');

-- Get the CareerIDs for reference
SET @cs_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Computer Science');
SET @business_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Business Administration');
SET @medicine_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Medicine');
SET @law_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Law');
SET @psych_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Psychology');
SET @arch_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Architecture');
SET @design_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Graphic Design');
SET @marketing_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Marketing');
SET @biology_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Biology');
SET @math_id = (SELECT CareerID FROM Careers WHERE CareerName = 'Mathematics');

-- 4. Interests
INSERT IGNORE INTO Interests (InterestName) VALUES 
('Sports'),
('Music'),
('Movies'),
('Literature'),
('Travel'),
('Photography'),
('Cooking'),
('Technology'),
('Art'),
('Video Games'),
('Dance'),
('Nature'),
('Politics'),
('Science'),
('Animals');

-- 5. Locations
-- Countries
INSERT IGNORE INTO Countries (CountryName) VALUES 
('United States'),
('United Kingdom'),
('Canada'),
('Australia'),
('Germany');

-- Get the CountryIDs for reference
SET @us_id = (SELECT CountryID FROM Countries WHERE CountryName = 'United States');
SET @uk_id = (SELECT CountryID FROM Countries WHERE CountryName = 'United Kingdom');
SET @canada_id = (SELECT CountryID FROM Countries WHERE CountryName = 'Canada');
SET @australia_id = (SELECT CountryID FROM Countries WHERE CountryName = 'Australia');
SET @germany_id = (SELECT CountryID FROM Countries WHERE CountryName = 'Germany');

-- Regions - using the correct CountryIDs
INSERT IGNORE INTO Regions (RegionName, CountryID) VALUES 
('California', @us_id),
('New York', @us_id),
('Texas', @us_id),
('London', @uk_id),
('Manchester', @uk_id),
('Ontario', @canada_id),
('British Columbia', @canada_id),
('New South Wales', @australia_id),
('Victoria', @australia_id),
('Bavaria', @germany_id);

-- Get the RegionIDs for reference
SET @california_id = (SELECT RegionID FROM Regions WHERE RegionName = 'California' AND CountryID = @us_id);
SET @newyork_id = (SELECT RegionID FROM Regions WHERE RegionName = 'New York' AND CountryID = @us_id);
SET @texas_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Texas' AND CountryID = @us_id);
SET @london_id = (SELECT RegionID FROM Regions WHERE RegionName = 'London' AND CountryID = @uk_id);
SET @manchester_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Manchester' AND CountryID = @uk_id);
SET @ontario_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Ontario' AND CountryID = @canada_id);
SET @bc_id = (SELECT RegionID FROM Regions WHERE RegionName = 'British Columbia' AND CountryID = @canada_id);
SET @nsw_id = (SELECT RegionID FROM Regions WHERE RegionName = 'New South Wales' AND CountryID = @australia_id);
SET @victoria_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Victoria' AND CountryID = @australia_id);
SET @bavaria_id = (SELECT RegionID FROM Regions WHERE RegionName = 'Bavaria' AND CountryID = @germany_id);

-- Cities - using the correct RegionIDs
INSERT IGNORE INTO Cities (CityName, RegionID) VALUES 
('Los Angeles', @california_id),
('San Francisco', @california_id),
('New York City', @newyork_id),
('London Central', @london_id),
('Manchester City', @manchester_id),
('Toronto', @ontario_id),
('Vancouver', @bc_id),
('Sydney', @nsw_id),
('Melbourne', @victoria_id),
('Munich', @bavaria_id);

-- Get the CityIDs for reference
SET @la_id = (SELECT CityID FROM Cities WHERE CityName = 'Los Angeles' AND RegionID = @california_id);
SET @sf_id = (SELECT CityID FROM Cities WHERE CityName = 'San Francisco' AND RegionID = @california_id);
SET @nyc_id = (SELECT CityID FROM Cities WHERE CityName = 'New York City' AND RegionID = @newyork_id);
SET @london_central_id = (SELECT CityID FROM Cities WHERE CityName = 'London Central' AND RegionID = @london_id);
SET @manchester_city_id = (SELECT CityID FROM Cities WHERE CityName = 'Manchester City' AND RegionID = @manchester_id);
SET @toronto_id = (SELECT CityID FROM Cities WHERE CityName = 'Toronto' AND RegionID = @ontario_id);
SET @vancouver_id = (SELECT CityID FROM Cities WHERE CityName = 'Vancouver' AND RegionID = @bc_id);
SET @sydney_id = (SELECT CityID FROM Cities WHERE CityName = 'Sydney' AND RegionID = @nsw_id);
SET @melbourne_id = (SELECT CityID FROM Cities WHERE CityName = 'Melbourne' AND RegionID = @victoria_id);
SET @munich_id = (SELECT CityID FROM Cities WHERE CityName = 'Munich' AND RegionID = @bavaria_id);

-- 6. Users - using the correct GenderIDs, CareerIDs, OrientationIDs, and CityIDs
INSERT IGNORE INTO Users (UserID, FullName, Age, GenderID, CareerID, OrientationID, ProfilePhrase, MinPreferredAge, MaxPreferredAge, IsVerified, CityID) VALUES 
(1, 'Charles Smith', 22, @male_id, @cs_id, @straight_id, 'I love programming and video games', 18, 28, TRUE, @la_id),
(2, 'Anna Johnson', 21, @female_id, @psych_id, @straight_id, 'Book lover and coffee enthusiast', 20, 25, TRUE, @la_id),
(3, 'Peter Williams', 25, @male_id, @law_id, @gay_id, 'Music, movies and law', 23, 30, FALSE, @sf_id),
(4, 'Mary Brown', 23, @female_id, @medicine_id, @straight_id, 'Med student who loves to travel', 22, 29, TRUE, @nyc_id),
(5, 'Laura Davis', 24, @female_id, @design_id, @bi_id, 'Creative designer and writer', 20, 30, TRUE, @london_central_id),
(6, 'Jack Wilson', 26, @male_id, @marketing_id, @straight_id, 'Digital marketing and photography', 24, 32, TRUE, @manchester_city_id),
(7, 'Victoria Taylor', 22, @female_id, @biology_id, @straight_id, 'Marine biology and conservation', 21, 28, TRUE, @toronto_id),
(8, 'Michael Anderson', 28, @male_id, @arch_id, @straight_id, 'Architect, I love modern art', 25, 35, TRUE, @vancouver_id),
(9, 'Sophia Martinez', 21, @female_id, @business_id, @bi_id, 'Finance and economics, also music', 20, 28, FALSE, @sydney_id),
(10, 'Daniel Thompson', 27, @male_id, @math_id, @gay_id, 'Applied mathematics and AI', 24, 32, TRUE, @melbourne_id);

-- 7. User accounts (password is "password123" for all)
INSERT IGNORE INTO UserAccounts (UserID, Email, Username, PasswordHash, IsActive) VALUES 
(1, 'charles@example.com', 'charles_s', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(2, 'anna@example.com', 'anna_j', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(3, 'peter@example.com', 'peter_w', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(4, 'mary@example.com', 'mary_b', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(5, 'laura@example.com', 'laura_d', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(6, 'jack@example.com', 'jack_w', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(7, 'victoria@example.com', 'victoria_t', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(8, 'michael@example.com', 'michael_a', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(9, 'sophia@example.com', 'sophia_m', '9c42a1346e333a770904b2a2b37fa7d3', TRUE),
(10, 'daniel@example.com', 'daniel_t', '9c42a1346e333a770904b2a2b37fa7d3', TRUE);

-- 8. User interests
INSERT IGNORE INTO UserInterests (UserID, InterestID) VALUES 
(1, 8), (1, 10), (1, 2), -- Charles: Technology, Video Games, Music
(2, 4), (2, 2), (2, 5),  -- Anna: Literature, Music, Travel
(3, 2), (3, 3), (3, 13), -- Peter: Music, Movies, Politics
(4, 5), (4, 14), (4, 3), -- Mary: Travel, Science, Movies
(5, 9), (5, 4), (5, 6),  -- Laura: Art, Literature, Photography
(6, 6), (6, 8), (6, 5),  -- Jack: Photography, Technology, Travel
(7, 12), (7, 14), (7, 15), -- Victoria: Nature, Science, Animals
(8, 9), (8, 1), (8, 7),  -- Michael: Art, Sports, Cooking
(9, 2), (9, 11), (9, 5), -- Sophia: Music, Dance, Travel
(10, 8), (10, 14), (10, 10); -- Daniel: Technology, Science, Video Games

-- 9. Interactions (Likes/Dislikes)
INSERT IGNORE INTO Interactions (FromUserID, ToUserID, InteractionType) VALUES 
-- Charles and Anna like each other
(1, 2, 'LIKE'),
(2, 1, 'LIKE'),
-- Laura and Jack like each other
(5, 6, 'LIKE'),
(6, 5, 'LIKE'),
-- Victoria and Michael like each other
(7, 8, 'LIKE'),
(8, 7, 'LIKE'),
-- Other interactions without matches
(1, 4, 'LIKE'),
(3, 2, 'LIKE'),
(4, 6, 'LIKE'),
(9, 10, 'LIKE'),
(10, 5, 'LIKE'),
(3, 9, 'DISLIKE'),
(5, 9, 'DISLIKE'),
(7, 3, 'DISLIKE'),
(8, 10, 'DISLIKE'),
(10, 1, 'DISLIKE');

-- 10. Matches (based on mutual likes)
INSERT IGNORE INTO Matches (User1ID, User2ID) VALUES 
(1, 2),   -- Charles and Anna
(5, 6),   -- Laura and Jack
(7, 8);   -- Victoria and Michael

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
