-- 1. Создаём базу
CREATE DATABASE CatalogAppDb;
GO

-- 2. Переключаемся в неё
USE CatalogAppDb;
GO

-- 3. Создаём таблицу Users
CREATE TABLE Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    UserEmail NVARCHAR(200) NULL,
    PasswordHash NVARCHAR(200) NOT NULL
);

-- 4. Создаём таблицу Products
CREATE TABLE Products
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(18,2) NOT NULL,
    ImageData VARBINARY(MAX) NULL,
    ImageContentType NVARCHAR(100) NULL,
    ImageFileName NVARCHAR(255) NULL,
    CategoryId INT NOT NULL
);