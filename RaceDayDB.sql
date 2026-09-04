Create Database RaceDayDB;
-- 1. CREATE TABLES 

-- User table for authentication
CREATE TABLE [User] (
    userID       INT PRIMARY KEY IDENTITY(1,1),
    email        VARCHAR(100) UNIQUE NOT NULL,
    passwordHash VARCHAR(255) NOT NULL,
    role         VARCHAR(20) NOT NULL CHECK (role IN ('Organiser', 'Participant')),
    fullName     VARCHAR(100) NOT NULL,
    phone        VARCHAR(20),
    createdAt    DATETIME DEFAULT GETDATE()
);

-- One organiser can host many events
CREATE TABLE Organiser (
    organiserID INT PRIMARY KEY,
    userID      INT UNIQUE NOT NULL,
    name        VARCHAR(100),
    address     VARCHAR(255),
    FOREIGN KEY (userID) REFERENCES [User](userID)
);

--One participant can have many entries
CREATE TABLE Participant (
    participantID     INT PRIMARY KEY,
    userID            INT UNIQUE NOT NULL,
    firstName         VARCHAR(50),
    lastName          VARCHAR(50),
    dateOfBirth       DATE,
    gender            VARCHAR(10),
    emergencyContact  VARCHAR(100),
    FOREIGN KEY (userID) REFERENCES [User](userID)
);

-- Each event belongs to exactly one organiser
CREATE TABLE Event (
    eventID         INT PRIMARY KEY,
    organiserID     INT FOREIGN KEY REFERENCES Organiser(organiserID),
    name            VARCHAR(100),
    description     VARCHAR(255),
    eventDate       DATE,
    startTime       VARCHAR(10),
    location        VARCHAR(255),
    status          VARCHAR(20),
    startPoint      VARCHAR(255),
    finishPoint     VARCHAR(255),
    mapUrl          VARCHAR(500),
    elevationUrl    VARCHAR(500),
    weatherForecast VARCHAR(50),
    temperature     DECIMAL(4,1),
    windSpeed       INT,
    precipitation   INT
);

-- Each category belongs to exactly one event
CREATE TABLE Category (
    categoryID  INT PRIMARY KEY,
    eventID     INT FOREIGN KEY REFERENCES Event(eventID),
    name        VARCHAR(50),
    description VARCHAR(100),
    minAge      INT,
    maxAge      INT,
    gender      VARCHAR(10),
    distance    DECIMAL(5,1)
);

-- Entry is a junction table linking Participant, Event, and Category
CREATE TABLE Entry (
    entryID          INT PRIMARY KEY,
    participantID    INT FOREIGN KEY REFERENCES Participant(participantID),
    eventID          INT FOREIGN KEY REFERENCES Event(eventID),
    categoryID       INT FOREIGN KEY REFERENCES Category(categoryID),
    registrationDate DATE,
    status           VARCHAR(20),
    startNumber      INT,
    paid             INT   
);

--Each result belongs to exactly one entry
CREATE TABLE Result (
    resultID   INT PRIMARY KEY,
    entryId    INT FOREIGN KEY REFERENCES Entry(entryID),
    finishTime VARCHAR(10),
    position   INT,
    status     VARCHAR(20),
    chipTime   VARCHAR(10),
    gunTime    VARCHAR(10)
);


-- ======================================================
-- 1. INSERT DATA

-- Users are participants and organisers

INSERT INTO [User] (email, passwordHash, role, fullName, phone) VALUES
('info@comrades.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Organiser', 'Comrades Marathon Association', '+27 33 345 6789'),
('info@sowetomarathon.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Organiser', 'Soweto Marathon Trust', '+27 11 987 6543'),
('info@twooceans.co.za', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Organiser', 'Two Oceans Marathon', '+27 21 555 1212'),
('info@cycletour.co.za', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Organiser', 'Cape Town Cycle Tour', '+27 21 888 9999'),
('thabo.mokoena@gmail.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Participant', 'Thabo Mokoena', '082 123 4567'),
('sarah.vdm@icloud.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Participant', 'Sarah van der Merwe', '083 234 5678'),
('john.smith@gmail.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Participant', 'John Smith', '084 345 6789'),
('zanele.nkosi@icloud.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Participant', 'Zanele Nkosi', '085 456 7890'),
('pieter.botha@gmail.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Participant', 'Pieter Botha', '086 567 8901'),
('lerato.mthembu@icloud.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Participant', 'Lerato Mthembu', '087 678 9012'),
('khanya.zondi@gmail.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Participant', 'Khanya Zondi', '088 765 4321'),
('tumisang.sebe@icloud.com', '$2a$11$K8XG7YzL9QwR5tU2vNxP4uW3mJkL1hG5fD2sA9qW8eR6tY4uI7oP0', 'Participant', 'Tumisang Sebe', '089 876 5432');

-- A. Organisers

INSERT INTO Organiser (organiserID, userID, name, address) VALUES
(1, 1, 'Comrades Marathon Association', '18 Connaught Road, Scottsville, Pietermaritzburg, 3201'),
(2, 2, 'Soweto Marathon Trust', 'Nasrec Expo Centre, Corner Nasrec Road & Rand Show Rd, Johannesburg, 2091'),
(3, 3, 'Two Oceans Marathon', 'PO Box 123, Newlands, Cape Town, 7700'),
(4, 4, 'Cape Town Cycle Tour', 'Cape Town Stadium, Green Point, Cape Town, 8005');

-- B. Participants

INSERT INTO Participant (participantID, userID, firstName, lastName, dateOfBirth, gender, emergencyContact) VALUES
(1, 5, 'Thabo', 'Mokoena', '1994-05-12', 'M', '081 987 6543'),
(2, 6, 'Sarah', 'van der Merwe', '1998-08-23', 'F', '082 876 5432'),
(3, 7, 'John', 'Smith', '1981-11-02', 'M', '083 765 4321'),
(4, 8, 'Zanele', 'Nkosi', '1974-03-17', 'F', '084 654 3210'),
(5, 9, 'Pieter', 'Botha', '1991-07-30', 'M', '085 543 2109'),
(6, 10, 'Lerato', 'Mthembu', '1996-09-10', 'F', '086 432 1098'),
(7, 11, 'Khanya', 'Zondi', '1993-02-14', 'F', '087 321 4567'),
(8, 12, 'Tumisang', 'Sebe', '1992-06-25', 'M', '088 765 4321');

-- C. Events

INSERT INTO Event (eventID, organiserID, name, description, eventDate, startTime, location, status, startPoint, finishPoint, mapUrl, elevationUrl, weatherForecast, temperature, windSpeed, precipitation) VALUES
(1, 1, 'Comrades Marathon 2026', 'The iconic "Up Run" from Durban to Pietermaritzburg (87km).', '2026-06-14', '05:30', 'Durban City Hall to Pietermaritzburg Oval', 'Open', 'Durban City Hall', 'Pietermaritzburg Oval', 'https://maps.example.com/comrades-2026', 'https://elevation.example.com/comrades', 'Sunny with light clouds', 22.5, 10, 0),
(2, 2, 'Soweto Marathon 2026', 'The "People''s Race" featuring Full, Half, and 10km.', '2026-11-29', '06:00', 'Nasrec Expo Centre, Johannesburg', 'Open', 'Nasrec Expo Centre', 'Nasrec Expo Centre', 'https://maps.example.com/soweto-2026', 'https://elevation.example.com/soweto', 'Mild, possible morning drizzle', 17.0, 15, 20),
(3, 3, 'Two Oceans Marathon 2026', 'The scenic 56km ultra marathon around the Cape Peninsula.', '2026-04-18', '06:30', 'Newlands, Cape Town to UCT', 'Open', 'Newlands, Cape Town', 'University of Cape Town', 'https://maps.example.com/twooceans-2026', 'https://elevation.example.com/twooceans', 'Clear skies, light wind', 19.0, 8, 0),
(4, 4, 'Cape Town Cycle Tour 2026', 'The world''s largest timed cycle race around the Cape Peninsula.', '2026-03-08', '07:00', 'Cape Town Stadium, Green Point', 'Open', 'Cape Town Stadium', 'Cape Town Stadium', 'https://maps.example.com/cycletour-2026', 'https://elevation.example.com/cycletour', 'Sunny, light breeze', 23.0, 5, 0);

-- D. Categories

INSERT INTO Category (categoryID, eventID, name, description, minAge, maxAge, gender, distance) VALUES
(1, 1, 'Men Open', 'Competitive men aged 20-39', 20, 39, 'M', 87.0),
(2, 1, 'Women Open', 'Competitive women aged 20-39', 20, 39, 'F', 87.0),
(3, 1, 'Men Veteran', 'Competitive men aged 40-49', 40, 49, 'M', 87.0),
(4, 1, 'Women Master', 'Competitive women aged 50-59', 50, 59, 'F', 87.0),
(5, 2, 'Full Marathon', 'Competitive Full Marathon (42.2km)', 20, 99, 'None', 42.2),
(6, 3, 'Ultra Marathon', '56km Ultra Marathon', 20, 60, 'None', 56.0),
(7, 4, 'Men 18-49', 'Competitive men aged 18-49', 18, 49, 'M', 109.0),
(8, 4, 'Women 18-49', 'Competitive women aged 18-49', 18, 49, 'F', 109.0);

-- E. Entries 

INSERT INTO Entry (entryID, participantID, eventID, categoryID, registrationDate, status, startNumber, paid) VALUES
(1, 1, 1, 1, '2026-01-15', 'Confirmed', 101, 1),
(2, 2, 1, 2, '2026-01-16', 'Confirmed', 202, 1),
(3, 3, 1, 3, '2026-01-20', 'Confirmed', 305, 1),
(4, 4, 1, 4, '2026-01-22', 'Confirmed', 408, 1),
(5, 5, 2, 5, '2026-02-01', 'Confirmed', 512, 1),
(6, 6, 2, 5, '2026-02-05', 'Pending', NULL, 0),
(7, 1, 3, 6, '2026-02-10', 'Confirmed', 603, 1),
(8, 7, 4, 8, '2026-02-12', 'Confirmed', 701, 1),
(9, 8, 4, 7, '2026-02-13', 'Confirmed', 801, 1),
(10, 5, 3, 6, '2026-02-15', 'Pending', NULL, 0);

-- F. Results 

INSERT INTO Result (resultID, entryId, finishTime, position, status, chipTime, gunTime) VALUES
(1, 1, '08:45:30', 523, 'Finished', '08:42:15', '08:45:30'),
(2, 2, '09:10:45', 648, 'Finished', '09:07:20', '09:10:45'),
(3, 3, '10:02:10', 912, 'Finished', '09:58:55', '10:02:10'),
(4, 4, '11:15:00', 1204, 'Finished', '11:10:40', '11:15:00'),
(5, 5, '03:45:20', 118, 'Finished', '03:43:50', '03:45:20');
GO