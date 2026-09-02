Create Database RaceDayDB;
-- 1. CREATE TABLES 


-- One organiser can host many events
CREATE TABLE Organiser (
    organiserID INT PRIMARY KEY,
    name        VARCHAR(100),
    email       VARCHAR(100),
    phone       VARCHAR(20),
    address     VARCHAR(255)
);

--One participant can have many entries
CREATE TABLE Participant (
    participantID     INT PRIMARY KEY,
    firstName         VARCHAR(50),
    lastName          VARCHAR(50),
    email             VARCHAR(100),
    dateOfBirth       DATE,
    gender            VARCHAR(10),
    phone             VARCHAR(20),
    emergencyContact  VARCHAR(100)
);

-- Each event belongs to exactly one organiser
CREATE TABLE Event (
    eventID      INT PRIMARY KEY,
    organiserID  INT FOREIGN KEY REFERENCES Organiser(organiserID),
    name         VARCHAR(100),
    description  VARCHAR(255),
    eventDate    DATE,
    startTime    VARCHAR(10),
    location     VARCHAR(255),
    status       VARCHAR(20)
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

-- A. Organisers
INSERT INTO Organiser (organiserID, name, email, phone, address) VALUES
(1, 'Comrades Marathon Association', 'info@comrades.com', '+27 33 345 6789', '18 Connaught Road, Scottsville, Pietermaritzburg, 3201'),
(2, 'Soweto Marathon Trust', 'info@sowetomarathon.com', '+27 11 987 6543', 'Nasrec Expo Centre, Corner Nasrec Road & Rand Show Rd, Johannesburg, 2091');

-- B. Participants
INSERT INTO Participant (participantID, firstName, lastName, email, dateOfBirth, gender, phone, emergencyContact) VALUES
(1, 'Thabo', 'Mokoena', 'thabo.mokoena@email.com', '1994-05-12', 'M', '082 123 4567', '081 987 6543'),
(2, 'Sarah', 'van der Merwe', 'sarah.vdm@email.com', '1998-08-23', 'F', '083 234 5678', '082 876 5432'),
(3, 'John', 'Smith', 'john.smith@email.com', '1981-11-02', 'M', '084 345 6789', '083 765 4321'),
(4, 'Zanele', 'Nkosi', 'zanele.nkosi@email.com', '1974-03-17', 'F', '085 456 7890', '084 654 3210'),
(5, 'Pieter', 'Botha', 'pieter.botha@email.com', '1991-07-30', 'M', '086 567 8901', '085 543 2109');

-- C. Events
INSERT INTO Event (eventID, organiserID, name, description, eventDate, startTime, location, status) VALUES
(1, 1, 'Comrades Marathon 2026', 'The iconic "Up Run" from Durban to Pietermaritzburg (87km).', '2026-06-14', '05:30', 'Durban City Hall to Pietermaritzburg Oval', 'Open'),
(2, 2, 'Soweto Marathon 2026', 'The "People''s Race" featuring Full, Half, and 10km.', '2026-11-29', '06:00', 'Nasrec Expo Centre, Johannesburg', 'Open');

-- D. Categories
INSERT INTO Category (categoryID, eventID, name, description, minAge, maxAge, gender, distance) VALUES
(1, 1, 'Men Open', 'Competitive men aged 20-39', 20, 39, 'M', 87.0),
(2, 1, 'Women Open', 'Competitive women aged 20-39', 20, 39, 'F', 87.0),
(3, 1, 'Men Veteran', 'Competitive men aged 40-49', 40, 49, 'M', 87.0),
(4, 1, 'Women Master', 'Competitive women aged 50-59', 50, 59, 'F', 87.0),
(5, 2, 'Full Marathon', 'Competitive Full Marathon (42.2km)', 20, 99, 'None', 42.2);

-- E. Entries (Critical: These provide the entryID values 1-5 for Results)
INSERT INTO Entry (entryID, participantID, eventID, categoryID, registrationDate, status, startNumber, paid) VALUES
(1, 1, 1, 1, '2026-01-15', 'Confirmed', 101, 1),
(2, 2, 1, 2, '2026-01-16', 'Confirmed', 202, 1),
(3, 3, 1, 3, '2026-01-20', 'Confirmed', 305, 1),
(4, 4, 1, 4, '2026-01-22', 'Confirmed', 408, 1),
(5, 5, 2, 5, '2026-02-01', 'Confirmed', 512, 1);

-- F. Results (5 rows, all fully populated, no NULLs)
INSERT INTO Result (resultID, entryId, finishTime, position, status, chipTime, gunTime) VALUES
(1, 1, '08:45:30', 523, 'Finished', '08:42:15', '08:45:30'),
(2, 2, '09:10:45', 648, 'Finished', '09:07:20', '09:10:45'),
(3, 3, '10:02:10', 912, 'Finished', '09:58:55', '10:02:10'),
(4, 4, '11:15:00', 1204, 'Finished', '11:10:40', '11:15:00'),
(5, 5, '03:45:20', 118, 'Finished', '03:43:50', '03:45:20');
GO

