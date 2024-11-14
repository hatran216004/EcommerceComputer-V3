CREATE DATABASE Store
GO
USE Store
GO

DBCC USEROPTIONS
ALTER DATABASE Store SET READ_COMMITTED_SNAPSHOT ON
GO

CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(10) NOT NULL DEFAULT 'User' CHECK (RoleName IN ('Admin', 'User', 'Employee')),
    Email VARCHAR(320) NOT NULL UNIQUE,
    [Password] VARCHAR(255) NOT NULL,
    PasswordChangedAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
	IsConfirm BIT NOT NULL DEFAULT 0,
	UniqueCode CHAR(64) NOT NULL UNIQUE,
	IsActive BIT NOT NULL DEFAULT 1
)

CREATE TABLE UserDetail (
    UserId INT PRIMARY KEY FOREIGN KEY REFERENCES [User](UserId) ON DELETE CASCADE,
    [Name] NVARCHAR(50) NOT NULL,
	Gender BIT, -- 1: Nam | 0: Nữ
    Phone VARCHAR(11),
    [Address] NVARCHAR(MAX),
    DateOfBirth DATE,
)

CREATE TABLE Brand (
    BrandId INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL UNIQUE
)

CREATE TABLE Category (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL UNIQUE
)

CREATE TABLE Product (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Stock INT NOT NULL CHECK (Stock >= 0),
    Price INT NOT NULL CHECK (Price > 0),
    PromoPrice INT,
    [Description] NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    BrandId INT FOREIGN KEY REFERENCES Brand(BrandId),
    CategoryId INT FOREIGN KEY REFERENCES Category(CategoryId)
)

ALTER TABLE Product ADD CHECK (PromoPrice IS NULL OR (PromoPrice > 0 AND PromoPrice < Price))

CREATE TABLE Gallery (
    GalleryId INT IDENTITY(1,1) PRIMARY KEY,
    Thumbnail NVARCHAR(MAX),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Product(ProductId) ON DELETE CASCADE,
	IsPrimary BIT NOT NULL DEFAULT 0
)


CREATE TABLE Cart (
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](UserId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Product(ProductId) ON DELETE CASCADE, -- Xóa sản phẩm sẽ xóa giỏ hàng liên quan
    Quantity INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY (UserId, ProductId)
)

CREATE TABLE [Order] (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(50) NOT NULL,
	Phone VARCHAR(11) NOT NULL,
    [Address] NVARCHAR(MAX) NOT NULL,
    Note NVARCHAR(MAX),
    [Status] NVARCHAR(50),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UserId INT FOREIGN KEY (UserId) REFERENCES [User](UserId),
	TotalPrice INT NOT NULL DEFAULT 0
)

CREATE TABLE OrderDetail (
    OrderId INT NOT NULL FOREIGN KEY REFERENCES [Order](OrderId) ON DELETE CASCADE,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Product(ProductId),
    Price INT NOT NULL,
    Quantity INT NOT NULL,
	PRIMARY KEY (OrderId, ProductId),
)

CREATE TABLE Payment (
	PaymentId INT PRIMARY KEY IDENTITY(1,1),
	OrderId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES [Order](OrderId) ON DELETE CASCADE,
	Amount MONEY DEFAULT 0 CHECK (Amount >= 0),
	Code VARCHAR(20),
	Method VARCHAR(10) NOT NULL DEFAULT 'Cash' CHECK (Method IN ('Cash', 'Bank')),
	[Status] VARCHAR(20) NOT NULL DEFAULT 'Waitting',
	PaymentDate DATETIME2 CHECK (PaymentDate <= GETDATE()),
	Expiry DATETIME2 NOT NULL DEFAULT DATEADD(day, 1, GETDATE()) CHECK (Expiry >= GETDATE()),
	TransactionId VARCHAR(20),
	Bank VARCHAR(20),
	Account VARCHAR(20)
)

CREATE UNIQUE NONCLUSTERED INDEX IDX_TransactionId_NOTNULL ON Payment(TransactionId) WHERE TransactionId IS NOT NULL
CREATE UNIQUE NONCLUSTERED INDEX IDX_Code_NOTNULL ON Payment(Code) WHERE Code IS NOT NULL

CREATE TABLE Review (
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](UserId) ON DELETE CASCADE,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Product(ProductId) ON DELETE CASCADE,
    Rating INT NOT NULL,
    Comment NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
	PRIMARY KEY (UserId, ProductId, CreatedAt),
)

GO
CREATE OR ALTER TRIGGER Tri_AddProduct ON Product
AFTER INSERT
AS
BEGIN
	DECLARE @Id INT = (SELECT ProductId from inserted)
	INSERT INTO Gallery (ProductId, IsPrimary) VALUES (@Id, 1)
END

GO
CREATE OR ALTER TRIGGER Tri_AddUserDetail ON [User]
AFTER INSERT
AS
BEGIN
	DECLARE @Id INT = (SELECT UserId from inserted)
	DECLARE @Email VARCHAR(320) = (SELECT Email FROM [User] WHERE UserId = @Id)
	INSERT INTO UserDetail (UserId, Name) VALUES (@Id, SUBSTRING(@Email, 1, CHARINDEX('@', @Email) - 1))
END
GO

GO
CREATE OR ALTER FUNCTION GetPrice(@ProductId INT)
RETURNS INT
AS
BEGIN
	DECLARE @Price INT = (SELECT IIF(PromoPrice IS NULL, Price, PromoPrice) FROM Product WHERE ProductId = @ProductId)
	RETURN @Price
END
GO
	
GO
CREATE OR ALTER TRIGGER Tri_CreateOrder ON [Order]
AFTER INSERT
AS
BEGIN
	DECLARE @OrderId INT = (SELECT OrderId FROM inserted)
	DECLARE @UserId INT = (SELECT UserId FROM inserted)
	DECLARE TMP CURSOR LOCAL SCROLL STATIC
	FOR SELECT ProductId, Quantity FROM Cart WHERE UserId = @UserId
	OPEN TMP
	DECLARE @ProductId INT, @Quantity INT
	FETCH NEXT FROM TMP INTO @ProductId, @Quantity
	WHILE (@@FETCH_STATUS=0)
	BEGIN
		INSERT INTO OrderDetail VALUES (@OrderId, @ProductId, dbo.GetPrice(@ProductId), @Quantity)
		FETCH NEXT FROM TMP INTO @ProductId, @Quantity
	END
	CLOSE TMP
	DEALLOCATE TMP
	INSERT INTO Payment (OrderId) VALUES (@OrderId)
END
GO

GO
CREATE OR ALTER PROC ReduceProductStock @UserId INT, @ProductId INT, @Quatity INT
AS
BEGIN
	DELETE FROM Cart WHERE UserId = @UserId AND ProductId = @ProductId
	UPDATE Product
	SET Stock = Stock - @Quatity
	WHERE ProductId = @ProductId
END
GO

GO
CREATE OR ALTER PROC UpdatePaymentStatus @UserId INT
AS
BEGIN
	UPDATE Payment
	SET [Status] = 'Failed'
	FROM [Order]
	WHERE UserId = @UserId AND Payment.OrderId = [Order].OrderId AND Payment.Expiry < GETDATE() AND Payment.Status = 'Waitting'
END
GO

GO
CREATE OR ALTER TRIGGER Tri_AddOderDetail ON OrderDetail
AFTER INSERT
AS
BEGIN
	DECLARE @UserId INT = (SELECT O.UserId FROM [Order] O JOIN inserted I ON O.OrderId = I.OrderId)
	DECLARE @ProductId INT = (SELECT ProductId FROM inserted)
	DECLARE @Quatity INT = (SELECT Quantity FROM inserted)
	IF ((SELECT Stock FROM Product WHERE ProductId = @ProductId) >= @Quatity)
		BEGIN
			EXEC ReduceProductStock @UserId, @ProductId, @Quatity
			UPDATE [Order]
			SET [Order].TotalPrice = [Order].TotalPrice + (inserted.Price * inserted.Quantity)
			FROM inserted
			WHERE [Order].OrderId = inserted.OrderId
		END
	ELSE
		THROW 50001, @ProductId, 1
END
GO

GO
CREATE OR ALTER TRIGGER Tri_UpdateCart ON Cart
AFTER INSERT, UPDATE
AS
BEGIN
	DECLARE @ProductId INT = (SELECT ProductId FROM inserted)
	IF (SELECT Quantity FROM inserted) > (SELECT Stock FROM Product WHERE ProductId = @ProductId)
		THROW 50001, @ProductId, 1
END
GO

GO
CREATE OR ALTER PROC PaymentFailed @PaymentId INT
AS
BEGIN
	DECLARE @Status VARCHAR(20) = (SELECT [Status] FROM Payment WHERE PaymentId = @PaymentId)
	DECLARE @OrderId INT = (SELECT OrderId FROM Payment WHERE PaymentId = @PaymentId)
	IF (@Status = 'Failed')
		BEGIN
			DECLARE TMP CURSOR LOCAL SCROLL STATIC
			FOR SELECT ProductId, Quantity FROM OrderDetail WHERE OrderId = @OrderId
			OPEN TMP
			DECLARE @ProductId INT, @Quantity INT
			FETCH NEXT FROM TMP INTO @ProductId, @Quantity
			WHILE (@@FETCH_STATUS=0)
			BEGIN
				UPDATE Product
				SET Stock = Stock + @Quantity
				WHERE ProductId = @ProductId
				FETCH NEXT FROM TMP INTO @ProductId, @Quantity
			END
			CLOSE TMP
			DEALLOCATE TMP
		END
END
GO

GO
CREATE OR ALTER TRIGGER Tri_UpdateOrder ON Payment
AFTER INSERT, UPDATE
AS
BEGIN
	DECLARE @Status VARCHAR(20) = (SELECT [Status] FROM inserted)
	IF (@Status = 'Waitting')
		BEGIN
			UPDATE [Order]
			SET [Status] = N'Chờ thanh toán'
			WHERE OrderId = (SELECT OrderId FROM inserted)
		END
	ELSE IF (@Status = 'Failed')
		BEGIN
			UPDATE [Order]
			SET [Status] = N'Thanh toán thất bại'
			WHERE OrderId = (SELECT OrderId FROM inserted)
			DECLARE @PaymentId INT = (SELECT PaymentId FROM inserted)
			EXEC PaymentFailed @PaymentId			
			UPDATE Payment
			SET Expiry = GETDATE()
			FROM inserted
			WHERE Payment.OrderId = inserted.OrderId 
		END
	ELSE IF (@Status = 'Accepted')
		BEGIN
			UPDATE [Order]
			SET [Status] = N'Thanh toán chấp nhận'
			WHERE OrderId = (SELECT OrderId FROM inserted)
			DECLARE @PaymentId1 INT = (SELECT PaymentId FROM inserted)
			EXEC PaymentFailed @PaymentId1			
			UPDATE Payment
			SET Expiry = GETDATE()
			FROM inserted
			WHERE Payment.OrderId = inserted.OrderId 
		END
	ELSE IF (@Status = 'Refunding')
		BEGIN
			UPDATE [Order]
			SET [Status] = N'Thanh toán đang hoàn tiền'
			WHERE OrderId = (SELECT OrderId FROM inserted)
			DECLARE @PaymentId2 INT = (SELECT PaymentId FROM inserted)
			EXEC PaymentFailed @PaymentId2			
			UPDATE Payment
			SET Expiry = GETDATE()
			FROM inserted
			WHERE Payment.OrderId = inserted.OrderId 
		END
	ELSE IF (@Status = 'Refunded')
		BEGIN
			UPDATE [Order]
			SET [Status] = N'Thanh toán đã hoàn tiền'
			WHERE OrderId = (SELECT OrderId FROM inserted)
			DECLARE @PaymentId3 INT = (SELECT PaymentId FROM inserted)
			EXEC PaymentFailed @PaymentId3			
			UPDATE Payment
			SET Expiry = GETDATE()
			FROM inserted
			WHERE Payment.OrderId = inserted.OrderId 
		END
	ELSE
		BEGIN
			UPDATE [Order]
			SET [Status] = N'Thanh toán chờ duyệt'
			WHERE OrderId = (SELECT OrderId FROM inserted)
		END
END
GO

GO
CREATE OR ALTER FUNCTION GetProductDiscountPercent(@ProductId INT)
RETURNS INT
AS
BEGIN
    DECLARE @DiscountPercent INT
    SELECT @DiscountPercent = 
        CASE 
            WHEN PromoPrice IS NULL THEN 0
            ELSE (100 * (Price - PromoPrice)) / Price
        END
    FROM Product
    WHERE ProductId = @ProductId
    RETURN @DiscountPercent
END
GO

GO
CREATE OR ALTER PROC UpdatePromoPrice
    @BrandId INT = NULL,
    @CategoryId INT = NULL,
    @DiscountPercent INT = NULL, 
    @DiscountMoney INT = NULL 
AS
BEGIN
    IF @BrandId IS NULL AND @CategoryId IS NULL
    BEGIN
        RAISERROR('Cần ít nhất một giá trị cho @BrandId hoặc @CategoryId.', 16, 1);
        RETURN;
    END

    IF @DiscountPercent IS NOT NULL AND (@DiscountPercent <= 0 OR @DiscountPercent > 100)
    BEGIN
        RAISERROR('DiscountPercent phải nằm trong khoảng từ 1 đến 100.', 16, 1);
        RETURN;
    END

	IF @DiscountMoney IS NOT NULL AND (@DiscountMoney <= 1000)
    BEGIN
        RAISERROR('DiscountMoney phải lớn hơn 1.000 VND.', 16, 1);
        RETURN;
    END

    IF @DiscountPercent IS NULL AND @DiscountMoney IS NULL
    BEGIN
        RAISERROR('Cần ít nhất một giá trị @DiscountPercent hoặc @DiscountMoney.', 16, 1);
        RETURN;
    END

    UPDATE Product
    SET PromoPrice = CASE 
                        WHEN @DiscountPercent IS NOT NULL THEN Price - (Price * @DiscountPercent / 100)
                        WHEN @DiscountMoney IS NOT NULL THEN Price - @DiscountMoney
                     END,
        UpdatedAt = GETDATE()
    WHERE
        (@CategoryId IS NULL OR CategoryId = @CategoryId)
        AND (@BrandId IS NULL OR BrandId = @BrandId);
END;
GO

GO
CREATE OR ALTER FUNCTION CountReviews(@ProductId INT)
RETURNS INT
AS
BEGIN
	DECLARE @Count INT = (SELECT COUNT(*) FROM Review WHERE ProductId = @ProductId)
	RETURN @Count
END
GO

SELECT dbo.CountReviews(4)

GO
CREATE OR ALTER FUNCTION StarAVG(@ProductId INT)
RETURNS FLOAT
AS
BEGIN
	DECLARE @Out FLOAT = (SELECT AVG(CAST(Rating AS float)) FROM Review WHERE ProductId = @ProductId)
	RETURN @Out
END
GO

SELECT dbo.StarAVG(4)

GO
CREATE OR ALTER PROC ChangeActiveState(@UserId INT)
AS
BEGIN
	DECLARE @CurrState BIT = (SELECT IsActive FROM [User] WHERE UserId = @UserId)
	IF @CurrState = 1
		BEGIN
			UPDATE [User]
			SET IsActive = 0
			WHERE UserId = @UserId
		END
	ELSE
		BEGIN
			UPDATE [User]
			SET IsActive = 1
			WHERE UserId = @UserId
		END
END
GO

DECLARE @Out BIT
EXEC @Out = ChangeActiveState 28
SELECT @Out

-- Thêm thương hiệu
INSERT INTO Brand (Name)
VALUES 
('Dell'),
('HP'),
('Asus'),
('Lenovo'),
('Acer')

-- Thêm danh mục sản phẩm
INSERT INTO Category (Name)
VALUES 
('Laptop'),
('Desktop'),
('Tablet'),
('Monitor'),
('Accessory')


