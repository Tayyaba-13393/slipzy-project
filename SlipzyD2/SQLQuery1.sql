
USE [SlipzyIdentityDb];
GO

-- 1. Categories table
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        CategoryId INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL
    );
END;
GO

-- 2. Purchases table
IF OBJECT_ID('dbo.Purchases', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchases
    (
        PurchaseId INT IDENTITY(1,1) PRIMARY KEY,

        UserId NVARCHAR(450) NOT NULL,

        StoreName NVARCHAR(150) NOT NULL,

        PurchaseDate DATE NOT NULL,

        CategoryId INT NOT NULL,

        TotalAmount DECIMAL(18,2) NOT NULL,

        WarrantyStart DATE NULL,

        WarrantyMonths INT NULL,

        WarrantyExpiry DATE NULL,

        ReturnDeadline DATE NULL,

        CreatedAt DATETIME2 NOT NULL
            DEFAULT GETDATE(),

        CONSTRAINT FK_Purchases_AspNetUsers
            FOREIGN KEY (UserId)
            REFERENCES dbo.AspNetUsers(Id),

        CONSTRAINT FK_Purchases_Categories
            FOREIGN KEY (CategoryId)
            REFERENCES dbo.Categories(CategoryId)
    );
END;
GO

-- 3. PurchaseItems table
IF OBJECT_ID('dbo.PurchaseItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseItems
    (
        PurchaseItemId INT IDENTITY(1,1) PRIMARY KEY,

        PurchaseId INT NOT NULL,

        ProductName NVARCHAR(150) NOT NULL,

        Quantity INT NOT NULL,

        UnitPrice DECIMAL(18,2) NOT NULL,

        LineTotal AS (Quantity * UnitPrice),

        CONSTRAINT FK_PurchaseItems_Purchases
            FOREIGN KEY (PurchaseId)
            REFERENCES dbo.Purchases(PurchaseId)
            ON DELETE CASCADE
    );
END;
GO

-- 4. ReceiptFiles table
IF OBJECT_ID('dbo.ReceiptFiles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReceiptFiles
    (
        ReceiptFileId INT IDENTITY(1,1) PRIMARY KEY,

        PurchaseId INT NOT NULL,

        OriginalFileName NVARCHAR(255) NOT NULL,

        RelativePath NVARCHAR(500) NOT NULL,

        UploadedAt DATETIME2 NOT NULL
            DEFAULT GETDATE(),

        CONSTRAINT FK_ReceiptFiles_Purchases
            FOREIGN KEY (PurchaseId)
            REFERENCES dbo.Purchases(PurchaseId)
            ON DELETE CASCADE
    );
END;
GO

-- 5. Create indexes
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Purchases_UserId'
      AND object_id = OBJECT_ID('dbo.Purchases')
)
BEGIN
    CREATE INDEX IX_Purchases_UserId
    ON dbo.Purchases(UserId);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PurchaseItems_PurchaseId'
      AND object_id = OBJECT_ID('dbo.PurchaseItems')
)
BEGIN
    CREATE INDEX IX_PurchaseItems_PurchaseId
    ON dbo.PurchaseItems(PurchaseId);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ReceiptFiles_PurchaseId'
      AND object_id = OBJECT_ID('dbo.ReceiptFiles')
)
BEGIN
    CREATE INDEX IX_ReceiptFiles_PurchaseId
    ON dbo.ReceiptFiles(PurchaseId);
END;
GO

-- 6. Insert default categories
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (CategoryName)
    VALUES
        ('Electronics'),
        ('Clothing'),
        ('Home Appliances'),
        ('Furniture'),
        ('Beauty'),
        ('Other');
END;
GO

PRINT 'Slipzy D3 tables created successfully!';
GO
