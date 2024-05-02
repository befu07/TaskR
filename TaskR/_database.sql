﻿

USE master
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
	AppRoleId INT NOT NULL DEFAULT(2) REFERENCES AppRole(Id)
)
GO


CREATE TABLE ToDoList
(
	Id INT PRIMARY KEY IDENTITY,
	Name VARCHAR(30) NOT NULL
)

CREATE TABLE Task
(
	Id INT PRIMARY KEY IDENTITY,
	[Descripton] VARCHAR(100) NOT NULL,
	ToDoListId INT NOT NULL FOREIGN KEY REFERENCES ToDoList,
	IsCompleted BIT NOT NULL,
	CreatedOn DATETIME NOT NULL,
	CompletedOn DATETIME NOT NULL,
	PricePerPerson DECIMAL(9,2) NULL,
	Deadline DATETIME NOT NULL, 
	Priority INT 
)
CREATE TABLE Tags
(
	Id INT PRIMARY KEY IDENTITY,
	Name VARCHAR(10) NOT NULL,
	HexColor CHAR(6) NOT NULL
)

CREATE TABLE TaskTags
(
	TaskId INT NOT NULL FOREIGN KEY REFERENCES Task,
	TagsId INT NOT NULL FOREIGN KEY REFERENCES Tags,
	  PRIMARY KEY (TaskId,TagsId)
)

go
Alter Table AppUser Add Email varchar(100)
go

ALTER TABLE AppUser ADD CONSTRAINT UQ_Email UNIQUE (Email)
go

ALTER TABLE Task 
drop Column PricePerPerson
go


ALTER TABLE ToDoList
Add AppUserId int 
go

ALTER TABLE ToDoList
Add Constraint Fk_ToDoList_AppUserId
foreign key (AppUserId) References AppUser(Id)  
go

select * from AppRole