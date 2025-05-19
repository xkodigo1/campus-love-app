CREATE DATABASE IF NOT EXISTS campuslove_db;

USE campuslove_db;

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
    FOREIGN KEY (GenderID) REFERENCES Genders(GenderID),
    FOREIGN KEY (CareerID) REFERENCES Careers(CareerID),
    FOREIGN KEY (OrientationID) REFERENCES SexualOrientations(OrientationID),
    FOREIGN KEY (CityID) REFERENCES Cities(CityID),
    CONSTRAINT chk_preferred_age CHECK (MinPreferredAge >= 18 AND MaxPreferredAge >= MinPreferredAge)
);

-- User Account Table for Login
CREATE TABLE UserAccounts (
    AccountID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    LastLoginDate DATETIME,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
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
    UNIQUE KEY unique_match (User1ID, User2ID)
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

-- 7. Chat System Tables
CREATE TABLE Conversations (
    ConversationID INT PRIMARY KEY AUTO_INCREMENT,
    User1ID INT NOT NULL,
    User2ID INT NOT NULL,
    StartDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastMessageDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (User1ID) REFERENCES Users(UserID),
    FOREIGN KEY (User2ID) REFERENCES Users(UserID),
    CONSTRAINT chk_different_users CHECK (User1ID <> User2ID),
    CONSTRAINT uc_users_conversation UNIQUE (User1ID, User2ID)
);

CREATE TABLE Messages (
    MessageID INT PRIMARY KEY AUTO_INCREMENT,
    ConversationID INT NOT NULL,
    SenderID INT NOT NULL,
    MessageText TEXT NOT NULL,
    SentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (ConversationID) REFERENCES Conversations(ConversationID),
    FOREIGN KEY (SenderID) REFERENCES Users(UserID)
);

-- Trigger to validate that sender is part of the conversation
DELIMITER //
CREATE TRIGGER before_message_insert
BEFORE INSERT ON Messages
FOR EACH ROW
BEGIN
    DECLARE sender_in_conversation INT;
    
    SELECT COUNT(*) INTO sender_in_conversation
    FROM Conversations 
    WHERE ConversationID = NEW.ConversationID 
    AND (User1ID = NEW.SenderID OR User2ID = NEW.SenderID);
    
    IF sender_in_conversation = 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Sender must be part of the conversation';
    END IF;
END //
DELIMITER ;

-- Index for faster message retrieval
CREATE INDEX idx_conversation_messages ON Messages(ConversationID, SentDate);

-- Index for unread message count queries
CREATE INDEX idx_unread_messages ON Messages(ConversationID, SenderID, IsRead);

-- Table for User Credits
CREATE TABLE IF NOT EXISTS user_credits (
    user_id INT PRIMARY KEY,
    credits_remaining INT NOT NULL DEFAULT 10,
    last_reset_date DATE NOT NULL DEFAULT CURRENT_DATE,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- Trigger to reset credits daily (this would typically be handled by a scheduled job)
-- For MySQL, you might use an event scheduler instead in production
DELIMITER //
CREATE TRIGGER IF NOT EXISTS reset_user_credits
BEFORE UPDATE ON user_credits
FOR EACH ROW
BEGIN
    IF DATE(OLD.last_reset_date) < DATE(CURRENT_DATE) THEN
        SET NEW.credits_remaining = 10;
        SET NEW.last_reset_date = CURRENT_DATE;
    END IF;
END//
DELIMITER ; 

