USE master
GO

ALTER DATABASE TaskR SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
GO
Drop DATABASE if exists TaskR
GO

CREATE DATABASE TaskR
GO

USE TaskR
GO
	CREATE TABLE AppRole
(
	Id INT PRIMARY KEY IDENTITY,
	RoleName VARCHAR(20) NOT NULL UNIQUE
)
GO
INSERT INTO AppRole VALUES
	('Admin'),
	('FreeUser'),
	('PremiumUser')
GO
CREATE TABLE AppUser
(
	Id INT PRIMARY KEY IDENTITY,
	Username VARCHAR(50) NOT NULL,
	PasswordHash BINARY(32) NOT NULL,
	Salt BINARY(32) NOT NULL,
	RegisteredOn DATETIME NOT NULL,
	Email VARCHAR(100) UNIQUE,
	AppRoleId INT NOT NULL DEFAULT(2) REFERENCES AppRole(Id)
)
GO


CREATE TABLE ToDoList
(
	Id INT PRIMARY KEY IDENTITY,
	Name VARCHAR(30) NOT NULL,
	AppUserId int NOT NULL,
	Constraint Fk_ToDoList_AppUserId
		foreign key (AppUserId) References AppUser(Id)
)

CREATE TABLE TaskItem
(
	Id INT PRIMARY KEY IDENTITY,
	[Description] VARCHAR(100) NOT NULL,
	ToDoListId INT NOT NULL FOREIGN KEY REFERENCES ToDoList ON DELETE CASCADE,
	IsCompleted BIT NOT NULL,
	CreatedOn DATETIME NOT NULL,
	CompletedOn DATETIME,
	Deadline DATETIME, 
	Priority INT 
)
CREATE TABLE Tags
(
	Id INT PRIMARY KEY IDENTITY,
	AppUserId INT,
	Name VARCHAR(10) NOT NULL,
	HexColor CHAR(6) NOT NULL,
	Constraint Fk_Tags_AppUserId
		foreign key (AppUserId) References AppUser(Id)
)

CREATE TABLE TaskTags
(
	TaskId INT NOT NULL FOREIGN KEY REFERENCES Task ON DELETE CASCADE,
	TagsId INT NOT NULL FOREIGN KEY REFERENCES Tags ON DELETE CASCADE,
	PRIMARY KEY (TaskId,TagsId)
)

--go
--Alter Table AppUser Add Email varchar(100)
--go

--ALTER TABLE AppUser ADD CONSTRAINT UQ_Email UNIQUE (Email)
--go


--ALTER TABLE ToDoList
--Add AppUserId int 
--go

--ALTER TABLE ToDoList
--Add Constraint Fk_ToDoList_AppUserId
--foreign key (AppUserId) References AppUser(Id)  
--go


Insert into Tags(Name, HexColor) Values 
	('Chores', 'EEFF88'),
	('Finance', '881122'),
	('Hobbies', '555555'),
	('Learning', '3399FF')

go

