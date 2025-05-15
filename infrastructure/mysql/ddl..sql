CREATE DATABASE IF NOT EXISTS campus-love-db;

USE campus-love-db;

-- 1. Lookup Tables First (no dependencies)
CREATE TABLE Genders (
    GenderID INT PRIMARY KEY AUTO_INCREMENT,
    GenderName VARCHAR(20) NOT NULL UNIQUE
);

CREATE TABLE Careers (
    CareerID INT PRIMARY KEY AUTO_INCREMENT,
    CareerName VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Interests (
    InterestID INT PRIMARY KEY AUTO_INCREMENT,
    InterestName VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE SexualOrientations (
    OrientationID INT PRIMARY KEY AUTO_INCREMENT,
    OrientationName VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Countries (
    CountryID INT PRIMARY KEY AUTO_INCREMENT,
    CountryName VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Regions (
    RegionID INT PRIMARY KEY AUTO_INCREMENT,
    RegionName VARCHAR(100) NOT NULL,
    CountryID INT NOT NULL,
    FOREIGN KEY (CountryID) REFERENCES Countries(CountryID),
    UNIQUE (RegionName, CountryID)
);

CREATE TABLE Cities (
    CityID INT PRIMARY KEY AUTO_INCREMENT,
    CityName VARCHAR(100) NOT NULL,
    RegionID INT NOT NULL,
    FOREIGN KEY (RegionID) REFERENCES Regions(RegionID),
    UNIQUE (CityName, RegionID)
);

-- 2. Main User Table
CREATE TABLE Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    FullName VARCHAR(100) NOT NULL,
    Age INT NOT NULL CHECK (Age >= 18),
    GenderID INT NOT NULL,
    CareerID INT NOT NULL,
    OrientationID INT,
    ProfilePhrase VARCHAR(255),
    MinPreferredAge INT NOT NULL DEFAULT 18,
    MaxPreferredAge INT NOT NULL DEFAULT 100,
    IsVerified BOOLEAN NOT NULL DEFAULT FALSE,
    CityID INT,
    RegistrationDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ProfilePictureUrl VARCHAR(255),
    FOREIGN KEY (GenderID) REFERENCES Genders(GenderID),
    FOREIGN KEY (CareerID) REFERENCES Careers(CareerID),
    FOREIGN KEY (OrientationID) REFERENCES SexualOrientations(OrientationID),
    FOREIGN KEY (CityID) REFERENCES Cities(CityID),
    CONSTRAINT chk_preferred_age CHECK (MinPreferredAge >= 18 AND MaxPreferredAge >= MinPreferredAge)
);

-- 3. Many-to-Many Table for Interests
CREATE TABLE UserInterests (
    UserID INT,
    InterestID INT,
    PRIMARY KEY (UserID, InterestID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (InterestID) REFERENCES Interests(InterestID)
);

-- 4. Interactions Table (LIKE / DISLIKE)
CREATE TABLE Interactions (
    InteractionID INT PRIMARY KEY AUTO_INCREMENT,
    FromUserID INT NOT NULL,
    ToUserID INT NOT NULL,
    InteractionType ENUM('LIKE', 'DISLIKE') NOT NULL,
    InteractionDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (FromUserID) REFERENCES Users(UserID),
    FOREIGN KEY (ToUserID) REFERENCES Users(UserID),
    CONSTRAINT chk_not_self_interaction CHECK (FromUserID <> ToUserID)
);

-- 5. Matches Table
CREATE TABLE Matches (
    MatchID INT PRIMARY KEY AUTO_INCREMENT,
    User1ID INT NOT NULL,
    User2ID INT NOT NULL,
    MatchDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (User1ID) REFERENCES Users(UserID),
    FOREIGN KEY (User2ID) REFERENCES Users(UserID),
    CONSTRAINT chk_unique_match_pair UNIQUE (LEAST(User1ID, User2ID), GREATEST(User1ID, User2ID))
);

-- 6. Daily Credits Table
CREATE TABLE DailyCredits (
    CreditID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT NOT NULL,
    CreditDate DATE NOT NULL,
    LikesUsed INT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    CONSTRAINT uc_user_date UNIQUE (UserID, CreditDate)
);

