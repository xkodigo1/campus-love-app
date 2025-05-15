-- 1. Countries
INSERT INTO Countries (CountryName) VALUES
('United States'),
('Colombia'),
('Mexico');

-- 2. Regions
INSERT INTO Regions (RegionName, CountryID) VALUES
('Antioquia', 2),
('Cundinamarca', 2),
('California', 1),
('Jalisco', 3);

-- 3. Cities
INSERT INTO Cities (CityName, RegionID) VALUES
('Medellín', 1),
('Bogotá', 2),
('Los Angeles', 3),
('Guadalajara', 4);

-- 4. Genders
INSERT INTO Genders (GenderName) VALUES
('Male'),
('Female'),
('Non-binary'),
('Other');

-- 5. Careers
INSERT INTO Careers (CareerName) VALUES
('Computer Science'),
('Psychology'),
('Engineering'),
('Business Administration');

-- 6. Interests
INSERT INTO Interests (InterestName) VALUES
('Music'),
('Technology'),
('Sports'),
('Reading'),
('Travel'),
('Gaming');

-- 7. Sexual Orientations
INSERT INTO SexualOrientations (OrientationName) VALUES
('Heterosexual'),
('Homosexual'),
('Bisexual'),
('Asexual');

-- 8. Users
INSERT INTO Users (
    FullName, Age, GenderID, CareerID, OrientationID, ProfilePhrase,
    MinPreferredAge, MaxPreferredAge, IsVerified, CityID
) VALUES
('Alice Johnson', 22, 2, 1, 1, 'Lover of code and coffee.', 22, 30, TRUE, 1),
('Bob Smith', 25, 1, 3, 3, 'Let’s build something cool!', 20, 28, TRUE, 2),
('Charlie Green', 24, 3, 2, 2, 'Books and calm evenings.', 22, 26, FALSE, 3),
('Diana López', 21, 2, 4, 1, 'Adventures await!', 20, 27, TRUE, 4);

-- 9. UserInterests
INSERT INTO UserInterests (UserID, InterestID) VALUES
(1, 2), -- Alice: Technology
(1, 6), -- Alice: Gaming
(2, 1), -- Bob: Music
(2, 2), -- Bob: Technology
(3, 4), -- Charlie: Reading
(3, 5), -- Charlie: Travel
(4, 1), -- Diana: Music
(4, 5); -- Diana: Travel

-- 10. Interactions (LIKE / DISLIKE)
INSERT INTO Interactions (FromUserID, ToUserID, InteractionType) VALUES
(1, 2, 'LIKE'),   -- Alice likes Bob
(2, 1, 'LIKE'),   -- Bob likes Alice -> match
(3, 4, 'LIKE'),   -- Charlie likes Diana
(4, 3, 'DISLIKE');-- Diana dislikes Charlie

-- 11. Matches (Only for mutual likes)
INSERT INTO Matches (User1ID, User2ID) VALUES
(1, 2); -- Alice & Bob

-- 12. DailyCredits (simulating like limits)
INSERT INTO DailyCredits (UserID, CreditDate, LikesUsed) VALUES
(1, CURRENT_DATE, 2),
(2, CURRENT_DATE, 1),
(3, CURRENT_DATE, 1),
(4, CURRENT_DATE, 1);
