-- Script إنشاء قاعدة البيانات والجداول مع بعض البيانات التجريبية
-- مناسب للتشغيل على SQL Server

/* 1) إنشاء قاعدة البيانات */
IF DB_ID(N'CarShopManagementSystem') IS NULL
BEGIN
    CREATE DATABASE CarShopManagementSystem;
END;
GO
USE CarShopManagementSystem;
GO

/* 2) جداول المستخدمين */
IF OBJECT_ID(N'Users', N'U') IS NOT NULL DROP TABLE Users;
GO
CREATE TABLE Users (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    FirstName    NVARCHAR(50)  NOT NULL,
    LastName     NVARCHAR(50)  NOT NULL,
    Email        NVARCHAR(100) NOT NULL UNIQUE,
    Password     NVARCHAR(100) NOT NULL,
    PhoneNumber  NVARCHAR(20),
    Address      NVARCHAR(200),
    Role         NVARCHAR(20)  NOT NULL DEFAULT N'Customer',
    IsAdmin      BIT           NOT NULL DEFAULT 0,
    RegisterDate DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

/* 3) جداول السيارات */
IF OBJECT_ID(N'Cars', N'U') IS NOT NULL DROP TABLE Cars;
GO
CREATE TABLE Cars (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Make        NVARCHAR(100) NOT NULL,
    Model       NVARCHAR(100) NOT NULL,
    Year        INT           NOT NULL,
    Color       NVARCHAR(50)  NOT NULL,
    Price       DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    IsUsed      BIT           NOT NULL DEFAULT 0,
    Mileage     INT,
    ImageUrl    NVARCHAR(200) NOT NULL,
    IsAvailable BIT           NOT NULL DEFAULT 1,
    SoldDate    DATETIME,
    DateAdded   DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

/* 4) جداول البطاقات الائتمانية */
IF OBJECT_ID(N'CreditCards', N'U') IS NOT NULL DROP TABLE CreditCards;
GO
CREATE TABLE CreditCards (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    CardNumber  NVARCHAR(16)  NOT NULL,
    HolderName  NVARCHAR(100) NOT NULL,
    ExpiryMonth INT           NOT NULL CHECK (ExpiryMonth BETWEEN 1 AND 12),
    ExpiryYear  INT           NOT NULL,
    CVV         NVARCHAR(4)   NOT NULL,
    Balance     DECIMAL(18,2) NOT NULL,
    IsValid     BIT           NOT NULL DEFAULT 1
);
GO

/* 5) جداول الحجوزات */
IF OBJECT_ID(N'Bookings', N'U') IS NOT NULL DROP TABLE Bookings;
GO
CREATE TABLE Bookings (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NOT NULL,
    CarId       INT NOT NULL,
    BookingDate DATETIME NOT NULL DEFAULT GETDATE(),
    ExpiryDate  DATETIME NOT NULL,
    IsActive    BIT      NOT NULL DEFAULT 1,
    CONSTRAINT FK_Bookings_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Bookings_Cars  FOREIGN KEY (CarId)  REFERENCES Cars(Id)
);
GO

/* 6) جداول المعاملات */
IF OBJECT_ID(N'Transactions', N'U') IS NOT NULL DROP TABLE Transactions;
GO
CREATE TABLE Transactions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    CarId           INT NOT NULL,
    SalePrice       DECIMAL(18,2) NOT NULL,
    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
    Notes           NVARCHAR(500),
    CONSTRAINT FK_Transactions_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Transactions_Cars  FOREIGN KEY (CarId)  REFERENCES Cars(Id)
);
GO

/*************************************************************************/
/*                        بيانات تجريبية (اختيارية)                      */
/*************************************************************************/

/* مستخدم مسؤول افتراضي */
INSERT INTO Users (FirstName, LastName, Email, Password, Role, IsAdmin)
VALUES (N'Admin', N'User', N'admin@carshop.com', N'Admin123!', N'Admin', 1);

/* بطاقات ائتمان للاستعمال أثناء الاختبارات */
INSERT INTO CreditCards (CardNumber, HolderName, ExpiryMonth, ExpiryYear, CVV, Balance)
VALUES
(N'4111111111111111', N'Waleed Khaled', 12, 2026, N'123', 10000.00),
(N'5500000000000004', N'Aya Ahmed',    5,  2025, N'456', 5000.00);

/* بعض السيارات المعروضة للبيع */
INSERT INTO Cars (Make, Model, Year, Color, Price, Description, IsUsed, Mileage, ImageUrl)
VALUES
(N'Toyota', N'Corolla', 2020, N'أبيض', 18000.00, N'سيارة موفرة واقتصادية', 0, NULL, N'/images/cars/corolla-2020.jpg'),
(N'Hyundai', N'Elantra', 2021, N'أسود', 19000.00, N'سيارة عائلية مريحة',    0, NULL, N'/images/cars/elantra-2021.jpg');

/* حجز تجريبي */
DECLARE @userId INT = (SELECT TOP 1 Id FROM Users WHERE Role = N'Customer');
DECLARE @carId  INT = (SELECT TOP 1 Id FROM Cars WHERE IsAvailable = 1);
IF @userId IS NOT NULL AND @carId IS NOT NULL
BEGIN
    INSERT INTO Bookings (UserId, CarId, ExpiryDate)
    VALUES (@userId, @carId, DATEADD(HOUR, 24, GETDATE()));
END;
GO

PRINT N'تم إنشاء الجداول وإضافة البيانات التجريبية بنجاح!';