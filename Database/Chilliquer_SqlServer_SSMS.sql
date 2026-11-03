/* ============================================================
   CHILLIQUER – SQL SERVER / SSMS DATABASE
   Môi trường: ASP.NET Core MVC + Entity Framework Core + SQL Server
   Ghi chú chuyển đổi từ Supabase/PostgreSQL:
   - UUID -> UNIQUEIDENTIFIER DEFAULT NEWID()
   - BOOLEAN -> BIT
   - TIMESTAMPTZ -> DATETIME2
   - GENERATED AS IDENTITY -> IDENTITY(1,1)
   - Supabase Auth/RLS -> UserAccounts + Roles + Session trong ASP.NET Core
   - gen_random_uuid(), NOW(), CURRENT_DATE -> NEWID(), SYSDATETIME(), CONVERT(date, GETDATE())
   ============================================================ */

IF DB_ID(N'ChilliquerDB') IS NULL
BEGIN
    CREATE DATABASE ChilliquerDB;
END
GO

USE ChilliquerDB;
GO

/* ===================== DROP OBJECTS ===================== */
IF OBJECT_ID('dbo.trg_stock_batches_update_stock', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_stock_batches_update_stock;
IF OBJECT_ID('dbo.trg_return_items_restore_stock', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_return_items_restore_stock;
IF OBJECT_ID('dbo.trg_invoice_items_decrease_stock', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_invoice_items_decrease_stock;
IF OBJECT_ID('dbo.trg_products_updated_at', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_products_updated_at;
IF OBJECT_ID('dbo.trg_customers_updated_at', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_customers_updated_at;
IF OBJECT_ID('dbo.trg_user_accounts_updated_at', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_user_accounts_updated_at;
IF OBJECT_ID('dbo.trg_member_cards_updated_at', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_member_cards_updated_at;
IF OBJECT_ID('dbo.trg_invoices_auto_number', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_invoices_auto_number;
IF OBJECT_ID('dbo.trg_member_cards_auto_number', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_member_cards_auto_number;
GO

DROP TABLE IF EXISTS dbo.AccessLogs;
DROP TABLE IF EXISTS dbo.ReturnItems;
DROP TABLE IF EXISTS dbo.Returns;
DROP TABLE IF EXISTS dbo.DepositNotifications;
DROP TABLE IF EXISTS dbo.Deposits;
DROP TABLE IF EXISTS dbo.InvoiceItems;
DROP TABLE IF EXISTS dbo.Invoices;
DROP TABLE IF EXISTS dbo.MemberCards;
DROP TABLE IF EXISTS dbo.Customers;
DROP TABLE IF EXISTS dbo.StockBatches;
DROP TABLE IF EXISTS dbo.Products;
DROP TABLE IF EXISTS dbo.WarehouseLocations;
DROP TABLE IF EXISTS dbo.Suppliers;
DROP TABLE IF EXISTS dbo.Manufacturers;
DROP TABLE IF EXISTS dbo.ProductCategories;
DROP TABLE IF EXISTS dbo.DiscountPolicies;
DROP TABLE IF EXISTS dbo.CancellationPolicies;
DROP TABLE IF EXISTS dbo.UserAccounts;
DROP TABLE IF EXISTS dbo.Roles;
GO

DROP FUNCTION IF EXISTS dbo.FnCalculateDiscount;
DROP FUNCTION IF EXISTS dbo.FnCheckMembershipValid;
DROP FUNCTION IF EXISTS dbo.FnCustomerAgeCheck;
DROP FUNCTION IF EXISTS dbo.FnStockAvailable;
DROP FUNCTION IF EXISTS dbo.FnReportRevenue;
DROP FUNCTION IF EXISTS dbo.FnReportLiquidation;
DROP FUNCTION IF EXISTS dbo.FnReportStockWarning;
DROP FUNCTION IF EXISTS dbo.FnReportRevenueSummary;
GO

DROP SEQUENCE IF EXISTS dbo.InvoiceSeq;
DROP SEQUENCE IF EXISTS dbo.CardSeq;
GO

/* ===================== TABLES ===================== */
CREATE TABLE dbo.Roles (
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(50)  NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);

CREATE TABLE dbo.UserAccounts (
    UserId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserAccounts PRIMARY KEY DEFAULT NEWID(),
    Username     NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FullName     NVARCHAR(200) NOT NULL,
    Email        NVARCHAR(200) NULL,
    Phone        NVARCHAR(20) NULL,
    RoleId       INT NOT NULL,
    IsActive     BIT NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_UserAccounts_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);

CREATE TABLE dbo.Manufacturers (
    ManufacturerId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name           NVARCHAR(200) NOT NULL,
    Country        NVARCHAR(100) NULL,
    Website        NVARCHAR(300) NULL,
    ContactPhone   NVARCHAR(30) NULL,
    ContactEmail   NVARCHAR(200) NULL,
    IsActive       BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Suppliers (
    SupplierId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name       NVARCHAR(200) NOT NULL,
    Address    NVARCHAR(500) NULL,
    Phone      NVARCHAR(30) NULL,
    Email      NVARCHAR(200) NULL,
    TaxCode    NVARCHAR(20) NULL,
    IsActive   BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.ProductCategories (
    CategoryId   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description  NVARCHAR(500) NULL
);

CREATE TABLE dbo.Products (
    ProductId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ProductCode   NVARCHAR(30) NOT NULL UNIQUE,
    Name          NVARCHAR(300) NOT NULL,
    CategoryId    INT NOT NULL,
    ManufacturerId UNIQUEIDENTIFIER NOT NULL,
    SupplierId    UNIQUEIDENTIFIER NOT NULL,
    Origin        NVARCHAR(100) NULL,
    VolumeMl      INT NULL,
    AlcoholPct    DECIMAL(5,2) NULL,
    SellPrice     DECIMAL(15,0) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    MinStockAlert INT NOT NULL DEFAULT 5,
    ImageUrl      NVARCHAR(1000) NULL,
    Description   NVARCHAR(MAX) NULL,
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.ProductCategories(CategoryId),
    CONSTRAINT FK_Products_Manufacturers FOREIGN KEY (ManufacturerId) REFERENCES dbo.Manufacturers(ManufacturerId),
    CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(SupplierId),
    CONSTRAINT CK_Products_SellPrice CHECK (SellPrice >= 0),
    CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0)
);

CREATE TABLE dbo.WarehouseLocations (
    LocationId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    LocationCode NVARCHAR(20) NOT NULL UNIQUE,
    Zone         NVARCHAR(10) NOT NULL,
    Shelf        NVARCHAR(10) NOT NULL,
    Compartment  NVARCHAR(10) NOT NULL,
    Description  NVARCHAR(500) NULL
);

CREATE TABLE dbo.StockBatches (
    BatchId         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ProductId       UNIQUEIDENTIFIER NOT NULL,
    LocationId      UNIQUEIDENTIFIER NULL,
    ImportPrice     DECIMAL(15,0) NOT NULL,
    QuantityIn      INT NOT NULL,
    QuantityRemain  INT NOT NULL,
    ManufactureYear SMALLINT NULL,
    ExpiryDate      DATE NULL,
    ImportDate      DATE NOT NULL DEFAULT CONVERT(date, GETDATE()),
    ImportedBy      UNIQUEIDENTIFIER NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT N'active',
    Note            NVARCHAR(MAX) NULL,
    CONSTRAINT FK_StockBatches_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId),
    CONSTRAINT FK_StockBatches_Locations FOREIGN KEY (LocationId) REFERENCES dbo.WarehouseLocations(LocationId),
    CONSTRAINT FK_StockBatches_Users FOREIGN KEY (ImportedBy) REFERENCES dbo.UserAccounts(UserId),
    CONSTRAINT CK_StockBatches_Quantity CHECK (QuantityIn > 0 AND QuantityRemain >= 0),
    CONSTRAINT CK_StockBatches_Status CHECK (Status IN (N'active',N'expired',N'liquidated',N'damaged'))
);

CREATE TABLE dbo.DiscountPolicies (
    PolicyId       INT IDENTITY(1,1) PRIMARY KEY,
    CardType       NVARCHAR(30) NOT NULL UNIQUE,
    CardNameVn     NVARCHAR(50) NOT NULL,
    DiscountPct    DECIMAL(5,2) NOT NULL,
    AnnualFee      DECIMAL(10,0) NOT NULL DEFAULT 20000,
    ValidityMonths INT NOT NULL DEFAULT 12,
    IsActive       BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.CancellationPolicies (
    PolicyId   INT IDENTITY(1,1) PRIMARY KEY,
    HoursLimit INT NOT NULL,
    FeePct     DECIMAL(5,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive   BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Customers (
    CustomerId  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Cccd        NVARCHAR(20) NOT NULL UNIQUE,
    FullName    NVARCHAR(200) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender      NVARCHAR(10) NULL,
    Address     NVARCHAR(500) NULL,
    Phone       NVARCHAR(20) NULL,
    Email       NVARCHAR(200) NULL,
    Occupation  NVARCHAR(200) NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Customers_Gender CHECK (Gender IS NULL OR Gender IN (N'male',N'female',N'other'))
);

CREATE SEQUENCE dbo.CardSeq START WITH 10000 INCREMENT BY 1;
CREATE TABLE dbo.MemberCards (
    CardId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CardNumber   NVARCHAR(20) NULL,
    CustomerId   UNIQUEIDENTIFIER NOT NULL UNIQUE,
    CardType     NVARCHAR(30) NOT NULL DEFAULT N'silver',
    IssueDate    DATE NOT NULL DEFAULT CONVERT(date, GETDATE()),
    ExpiryDate   DATE NOT NULL,
    RenewalCount INT NOT NULL DEFAULT 0,
    FeePaid      DECIMAL(10,0) NOT NULL DEFAULT 20000,
    IssuedBy     UNIQUEIDENTIFIER NULL,
    Status       NVARCHAR(20) NOT NULL DEFAULT N'active',
    CancelledAt  DATETIME2 NULL,
    CreatedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_MemberCards_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT FK_MemberCards_Users FOREIGN KEY (IssuedBy) REFERENCES dbo.UserAccounts(UserId),
    CONSTRAINT CK_MemberCards_Status CHECK (Status IN (N'active',N'expired',N'cancelled'))
);
CREATE UNIQUE INDEX UX_MemberCards_CardNumber_NotNull ON dbo.MemberCards(CardNumber) WHERE CardNumber IS NOT NULL;

CREATE SEQUENCE dbo.InvoiceSeq START WITH 1 INCREMENT BY 1;
CREATE TABLE dbo.Invoices (
    InvoiceId            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InvoiceNumber        NVARCHAR(30) NULL,
    CashierId            UNIQUEIDENTIFIER NOT NULL,
    CardId               UNIQUEIDENTIFIER NULL,
    CustomerName         NVARCHAR(200) NULL,
    SaleDate             DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    SaleType             NVARCHAR(20) NOT NULL DEFAULT N'direct',
    TotalBeforeDiscount  DECIMAL(15,0) NOT NULL,
    DiscountPct          DECIMAL(5,2) NOT NULL DEFAULT 0,
    DiscountAmount       DECIMAL(15,0) NOT NULL DEFAULT 0,
    TotalAmount          DECIMAL(15,0) NOT NULL,
    PaymentMethod        NVARCHAR(20) NOT NULL DEFAULT N'cash',
    Status               NVARCHAR(20) NOT NULL DEFAULT N'completed',
    Note                 NVARCHAR(MAX) NULL,
    CreatedAt            DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Invoices_Cashier FOREIGN KEY (CashierId) REFERENCES dbo.UserAccounts(UserId),
    CONSTRAINT FK_Invoices_MemberCards FOREIGN KEY (CardId) REFERENCES dbo.MemberCards(CardId),
    CONSTRAINT CK_Invoices_SaleType CHECK (SaleType IN (N'direct',N'online',N'delivery')),
    CONSTRAINT CK_Invoices_PaymentMethod CHECK (PaymentMethod IN (N'cash',N'transfer',N'card')),
    CONSTRAINT CK_Invoices_Status CHECK (Status IN (N'completed',N'cancelled',N'returned'))
);
CREATE UNIQUE INDEX UX_Invoices_InvoiceNumber_NotNull ON dbo.Invoices(InvoiceNumber) WHERE InvoiceNumber IS NOT NULL;

CREATE TABLE dbo.InvoiceItems (
    ItemId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InvoiceId   UNIQUEIDENTIFIER NOT NULL,
    ProductId   UNIQUEIDENTIFIER NOT NULL,
    ProductName NVARCHAR(300) NOT NULL,
    UnitPrice   DECIMAL(15,0) NOT NULL,
    Quantity    INT NOT NULL,
    Subtotal    DECIMAL(15,0) NOT NULL,
    CONSTRAINT FK_InvoiceItems_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(InvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_InvoiceItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId),
    CONSTRAINT CK_InvoiceItems_Quantity CHECK (Quantity > 0)
);

CREATE TABLE dbo.Deposits (
    DepositId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InvoiceId      UNIQUEIDENTIFIER NOT NULL,
    CustomerId     UNIQUEIDENTIFIER NOT NULL,
    DepositAmount  DECIMAL(15,0) NOT NULL,
    DepositDate    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ExpectedPickup DATE NOT NULL,
    ActualPickup   DATETIME2 NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT N'pending',
    Note           NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Deposits_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(InvoiceId),
    CONSTRAINT FK_Deposits_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId),
    CONSTRAINT CK_Deposits_Status CHECK (Status IN (N'pending',N'completed',N'overdue',N'refunded',N'forfeited'))
);

CREATE TABLE dbo.DepositNotifications (
    NotifId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    DepositId   UNIQUEIDENTIFIER NOT NULL,
    SentAt      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    DaysOverdue INT NOT NULL,
    Channel     NVARCHAR(20) NOT NULL DEFAULT N'system',
    Message     NVARCHAR(MAX) NULL,
    CONSTRAINT FK_DepositNotifications_Deposits FOREIGN KEY (DepositId) REFERENCES dbo.Deposits(DepositId),
    CONSTRAINT CK_DepositNotifications_Channel CHECK (Channel IN (N'system',N'email',N'sms'))
);

CREATE TABLE dbo.Returns (
    ReturnId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InvoiceId     UNIQUEIDENTIFIER NOT NULL,
    ProcessedBy   UNIQUEIDENTIFIER NOT NULL,
    ReturnDate    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ReturnType    NVARCHAR(20) NOT NULL DEFAULT N'return',
    CancelFeePct  DECIMAL(5,2) NOT NULL DEFAULT 0,
    CancelFeeAmt  DECIMAL(15,0) NOT NULL DEFAULT 0,
    RefundAmount  DECIMAL(15,0) NOT NULL,
    Reason        NVARCHAR(MAX) NULL,
    Status        NVARCHAR(20) NOT NULL DEFAULT N'approved',
    CONSTRAINT FK_Returns_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoices(InvoiceId),
    CONSTRAINT FK_Returns_Users FOREIGN KEY (ProcessedBy) REFERENCES dbo.UserAccounts(UserId),
    CONSTRAINT CK_Returns_ReturnType CHECK (ReturnType IN (N'return',N'exchange')),
    CONSTRAINT CK_Returns_Status CHECK (Status IN (N'approved',N'rejected'))
);

CREATE TABLE dbo.ReturnItems (
    RetItemId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ReturnId    UNIQUEIDENTIFIER NOT NULL,
    ProductId   UNIQUEIDENTIFIER NOT NULL,
    ProductName NVARCHAR(300) NOT NULL,
    Quantity    INT NOT NULL,
    UnitPrice   DECIMAL(15,0) NOT NULL,
    Subtotal    DECIMAL(15,0) NOT NULL,
    ConditionOk BIT NOT NULL DEFAULT 1,
    Note        NVARCHAR(MAX) NULL,
    CONSTRAINT FK_ReturnItems_Returns FOREIGN KEY (ReturnId) REFERENCES dbo.Returns(ReturnId) ON DELETE CASCADE,
    CONSTRAINT FK_ReturnItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId),
    CONSTRAINT CK_ReturnItems_Quantity CHECK (Quantity > 0)
);

CREATE TABLE dbo.AccessLogs (
    LogId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId    UNIQUEIDENTIFIER NULL,
    LoginAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    IpAddress NVARCHAR(45) NULL,
    Status    NVARCHAR(20) NOT NULL,
    Note      NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AccessLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.UserAccounts(UserId),
    CONSTRAINT CK_AccessLogs_Status CHECK (Status IN (N'success',N'failed'))
);
GO

/* ===================== INDEXES ===================== */
CREATE INDEX IX_Products_Name ON dbo.Products(Name);
CREATE INDEX IX_Products_Category ON dbo.Products(CategoryId);
CREATE INDEX IX_Products_Active ON dbo.Products(IsActive);
CREATE INDEX IX_StockBatches_Product ON dbo.StockBatches(ProductId);
CREATE INDEX IX_StockBatches_Expiry ON dbo.StockBatches(ExpiryDate);
CREATE INDEX IX_Invoices_Date ON dbo.Invoices(SaleDate);
CREATE INDEX IX_Invoices_Cashier ON dbo.Invoices(CashierId);
CREATE INDEX IX_InvoiceItems_Invoice ON dbo.InvoiceItems(InvoiceId);
CREATE INDEX IX_Customers_Cccd ON dbo.Customers(Cccd);
CREATE INDEX IX_MemberCards_Status ON dbo.MemberCards(Status);
CREATE INDEX IX_Deposits_Status ON dbo.Deposits(Status);
GO

/* ===================== FUNCTIONS ===================== */
CREATE FUNCTION dbo.FnCalculateDiscount(@CardType NVARCHAR(30))
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @Discount DECIMAL(5,2);
    SELECT @Discount = DiscountPct
    FROM dbo.DiscountPolicies
    WHERE CardType = @CardType AND IsActive = 1;
    RETURN ISNULL(@Discount, 0);
END
GO

CREATE FUNCTION dbo.FnCheckMembershipValid(@CardId UNIQUEIDENTIFIER)
RETURNS BIT
AS
BEGIN
    DECLARE @Result BIT = 0;
    IF EXISTS (
        SELECT 1 FROM dbo.MemberCards
        WHERE CardId = @CardId AND Status = N'active' AND ExpiryDate >= CONVERT(date, GETDATE())
    ) SET @Result = 1;
    RETURN @Result;
END
GO

CREATE FUNCTION dbo.FnCustomerAgeCheck(@CustomerId UNIQUEIDENTIFIER)
RETURNS BIT
AS
BEGIN
    DECLARE @Dob DATE, @Result BIT = 0;
    SELECT @Dob = DateOfBirth FROM dbo.Customers WHERE CustomerId = @CustomerId;
    IF @Dob IS NOT NULL AND DATEADD(YEAR, 18, @Dob) <= CONVERT(date, GETDATE()) SET @Result = 1;
    RETURN @Result;
END
GO

CREATE FUNCTION dbo.FnStockAvailable(@ProductId UNIQUEIDENTIFIER, @QuantityNeeded INT)
RETURNS BIT
AS
BEGIN
    DECLARE @Result BIT = 0;
    IF EXISTS (SELECT 1 FROM dbo.Products WHERE ProductId = @ProductId AND StockQuantity >= @QuantityNeeded)
        SET @Result = 1;
    RETURN @Result;
END
GO

CREATE FUNCTION dbo.FnReportRevenue(@FromDate DATE, @ToDate DATE)
RETURNS TABLE
AS
RETURN
(
    SELECT
        p.ProductId,
        p.ProductCode,
        ii.ProductName,
        pc.CategoryName,
        SUM(ii.Quantity) AS TotalQty,
        SUM(ii.Subtotal) AS TotalRevenue
    FROM dbo.InvoiceItems ii
    INNER JOIN dbo.Invoices inv ON inv.InvoiceId = ii.InvoiceId
    INNER JOIN dbo.Products p ON p.ProductId = ii.ProductId
    INNER JOIN dbo.ProductCategories pc ON pc.CategoryId = p.CategoryId
    WHERE inv.SaleDate >= @FromDate
      AND inv.SaleDate < DATEADD(DAY, 1, @ToDate)
      AND inv.Status = N'completed'
    GROUP BY p.ProductId, p.ProductCode, ii.ProductName, pc.CategoryName
);
GO

CREATE FUNCTION dbo.FnReportLiquidation()
RETURNS TABLE
AS
RETURN
(
    SELECT
        p.ProductCode,
        p.Name AS ProductName,
        m.Name AS ManufacturerName,
        sb.ManufactureYear,
        sb.ImportDate,
        sb.ExpiryDate,
        sb.QuantityRemain,
        sb.Status AS BatchStatus,
        wl.LocationCode
    FROM dbo.StockBatches sb
    INNER JOIN dbo.Products p ON p.ProductId = sb.ProductId
    INNER JOIN dbo.Manufacturers m ON m.ManufacturerId = p.ManufacturerId
    LEFT JOIN dbo.WarehouseLocations wl ON wl.LocationId = sb.LocationId
    WHERE sb.Status IN (N'expired', N'damaged')
       OR (sb.ExpiryDate IS NOT NULL AND sb.ExpiryDate < CONVERT(date, GETDATE()))
);
GO

CREATE FUNCTION dbo.FnReportStockWarning(@Threshold INT = NULL)
RETURNS TABLE
AS
RETURN
(
    SELECT
        p.ProductCode,
        p.Name AS ProductName,
        pc.CategoryName,
        p.StockQuantity,
        p.MinStockAlert,
        p.SellPrice
    FROM dbo.Products p
    INNER JOIN dbo.ProductCategories pc ON pc.CategoryId = p.CategoryId
    WHERE p.IsActive = 1
      AND p.StockQuantity <= ISNULL(@Threshold, p.MinStockAlert)
);
GO

CREATE FUNCTION dbo.FnReportRevenueSummary(@Year INT)
RETURNS TABLE
AS
RETURN
(
    SELECT
        MONTH(SaleDate) AS MonthNumber,
        CONCAT(N'Tháng ', MONTH(SaleDate)) AS PeriodLabel,
        COUNT(*) AS InvoiceCount,
        SUM(TotalAmount) AS TotalRevenue
    FROM dbo.Invoices
    WHERE YEAR(SaleDate) = @Year AND Status = N'completed'
    GROUP BY MONTH(SaleDate)
);
GO

/* ===================== TRIGGERS ===================== */
CREATE TRIGGER dbo.trg_products_updated_at ON dbo.Products
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p SET UpdatedAt = SYSDATETIME()
    FROM dbo.Products p INNER JOIN inserted i ON p.ProductId = i.ProductId;
END
GO

CREATE TRIGGER dbo.trg_customers_updated_at ON dbo.Customers
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE c SET UpdatedAt = SYSDATETIME()
    FROM dbo.Customers c INNER JOIN inserted i ON c.CustomerId = i.CustomerId;
END
GO

CREATE TRIGGER dbo.trg_user_accounts_updated_at ON dbo.UserAccounts
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u SET UpdatedAt = SYSDATETIME()
    FROM dbo.UserAccounts u INNER JOIN inserted i ON u.UserId = i.UserId;
END
GO

CREATE TRIGGER dbo.trg_member_cards_updated_at ON dbo.MemberCards
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mc SET UpdatedAt = SYSDATETIME()
    FROM dbo.MemberCards mc INNER JOIN inserted i ON mc.CardId = i.CardId;
END
GO

CREATE TRIGGER dbo.trg_invoices_auto_number ON dbo.Invoices
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE inv
    SET InvoiceNumber = CONCAT('HD', CONVERT(CHAR(8), GETDATE(), 112), '-', RIGHT(CONCAT('0000', NEXT VALUE FOR dbo.InvoiceSeq), 4))
    FROM dbo.Invoices inv INNER JOIN inserted i ON inv.InvoiceId = i.InvoiceId
    WHERE inv.InvoiceNumber IS NULL OR inv.InvoiceNumber = N'';
END
GO

CREATE TRIGGER dbo.trg_member_cards_auto_number ON dbo.MemberCards
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE mc
    SET CardNumber = CONCAT('TV', RIGHT(CONCAT('00000000', NEXT VALUE FOR dbo.CardSeq), 8))
    FROM dbo.MemberCards mc INNER JOIN inserted i ON mc.CardId = i.CardId
    WHERE mc.CardNumber IS NULL OR mc.CardNumber = N'';
END
GO

CREATE TRIGGER dbo.trg_stock_batches_update_stock ON dbo.StockBatches
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET StockQuantity = p.StockQuantity + x.TotalQuantity
    FROM dbo.Products p
    INNER JOIN (
        SELECT ProductId, SUM(QuantityIn) AS TotalQuantity
        FROM inserted
        GROUP BY ProductId
    ) x ON p.ProductId = x.ProductId;
END
GO

CREATE TRIGGER dbo.trg_invoice_items_decrease_stock ON dbo.InvoiceItems
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM dbo.Products p
        INNER JOIN (
            SELECT ProductId, SUM(Quantity) AS Qty
            FROM inserted
            GROUP BY ProductId
        ) x ON p.ProductId = x.ProductId
        WHERE p.StockQuantity < x.Qty
    )
    BEGIN
        RAISERROR(N'Sản phẩm không đủ số lượng tồn kho.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE p
    SET StockQuantity = p.StockQuantity - x.Qty
    FROM dbo.Products p
    INNER JOIN (
        SELECT ProductId, SUM(Quantity) AS Qty
        FROM inserted
        GROUP BY ProductId
    ) x ON p.ProductId = x.ProductId;
END
GO

CREATE TRIGGER dbo.trg_return_items_restore_stock ON dbo.ReturnItems
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET StockQuantity = p.StockQuantity + x.Qty
    FROM dbo.Products p
    INNER JOIN (
        SELECT ProductId, SUM(Quantity) AS Qty
        FROM inserted
        WHERE ConditionOk = 1
        GROUP BY ProductId
    ) x ON p.ProductId = x.ProductId;
END
GO

/* ===================== SEED DATA ===================== */
INSERT INTO dbo.Roles (RoleName, Description) VALUES
(N'admin', N'Quản trị hệ thống – toàn quyền'),
(N'manager', N'Quản lý – bán hàng, kho hàng, sản phẩm, báo cáo'),
(N'sales', N'Nhân viên bán hàng'),
(N'customer', N'Tài khoản khách hàng');

-- Mật khẩu mẫu cho tất cả tài khoản: Admin@123
DECLARE @PasswordHash NVARCHAR(500) = N'PBKDF2$100000$n2JGBwKrh0QHME3u9gLjHQ==$gY8jyD6udK98Wvyh2vMxT2+/OfdZP3Y1cxWj6amYZKo=';
INSERT INTO dbo.UserAccounts (UserId, Username, PasswordHash, FullName, Email, Phone, RoleId) VALUES
('11111111-1111-1111-1111-111111111111', N'admin', @PasswordHash, N'Nguyễn Quản Trị', N'admin@chilliquer.vn', N'0901000001', 1),
('22222222-2222-2222-2222-222222222222', N'quanly', @PasswordHash, N'Trần Văn Quản Lý', N'quanly@chilliquer.vn', N'0901000002', 2),
('44444444-4444-4444-4444-444444444444', N'banhang1', @PasswordHash, N'Phạm Nhân Viên', N'nv1@chilliquer.vn', N'0901000004', 3),
('55555555-5555-5555-5555-555555555555', N'banhang2', @PasswordHash, N'Hoàng Thị Bán Hàng', N'nv2@chilliquer.vn', N'0901000005', 3);

INSERT INTO dbo.DiscountPolicies (CardType, CardNameVn, DiscountPct, AnnualFee, ValidityMonths) VALUES
(N'none', N'Khách vãng lai', 0, 0, 0),
(N'silver', N'Thành viên Bạc', 5, 20000, 12),
(N'gold', N'Thành viên Vàng', 10, 20000, 12),
(N'diamond', N'Thành viên Kim Cương', 15, 20000, 12);

INSERT INTO dbo.CancellationPolicies (HoursLimit, FeePct, Description) VALUES
(0, 10, N'Hủy trong ngày – phí 10%'),
(24, 20, N'Hủy sau 24 giờ – phí 20%');

INSERT INTO dbo.Manufacturers (ManufacturerId, Name, Country, Website) VALUES
('AAAA0001-0000-0000-0000-000000000001', N'Château Margaux', N'Pháp', N'https://www.chateau-margaux.com'),
('AAAA0002-0000-0000-0000-000000000002', N'Johnnie Walker (Diageo)', N'Scotland', N'https://www.johnniewalker.com'),
('AAAA0003-0000-0000-0000-000000000003', N'Absolut Vodka (Pernod Ricard)', N'Thụy Điển', N'https://www.absolut.com'),
('AAAA0004-0000-0000-0000-000000000004', N'Vang Đà Lạt', N'Việt Nam', NULL),
('AAAA0005-0000-0000-0000-000000000005', N'Heineken N.V.', N'Hà Lan', N'https://www.heineken.com'),
('AAAA0006-0000-0000-0000-000000000006', N'Glenfiddich (William Grant)', N'Scotland', N'https://www.glenfiddich.com'),
('AAAA0007-0000-0000-0000-000000000007', N'Nước Trắng Lúa Mới', N'Việt Nam', NULL);

INSERT INTO dbo.Suppliers (SupplierId, Name, Address, Phone, TaxCode) VALUES
('BBBB0001-0000-0000-0000-000000000001', N'Công ty TNHH Rượu Nhập Khẩu Miền Trung', N'12 Nguyễn Văn Linh, Đà Nẵng', N'0236 3333 001', N'0400123456'),
('BBBB0002-0000-0000-0000-000000000002', N'Đại lý Bia Rượu Khánh Hòa', N'56 Lê Lợi, Nha Trang', N'0258 3888 002', N'0400234567'),
('BBBB0003-0000-0000-0000-000000000003', N'Công ty CP Phân Phối Đồ Uống Sài Gòn', N'88 Đinh Tiên Hoàng, TP.HCM', N'028 3999 003', N'0300345678');

INSERT INTO dbo.ProductCategories (CategoryName, Description) VALUES
(N'Vang đỏ', N'Rượu vang đỏ các loại'),
(N'Vang trắng', N'Rượu vang trắng'),
(N'Vang hồng', N'Rượu vang rosé'),
(N'Whisky', N'Whisky Scotch, Irish, Bourbon'),
(N'Vodka', N'Rượu Vodka'),
(N'Bia', N'Bia các loại'),
(N'Rượu gạo', N'Rượu trắng và rượu gạo truyền thống Việt Nam'),
(N'Sake', N'Rượu Sake Nhật Bản'),
(N'Gin', N'Rượu Gin');

INSERT INTO dbo.Products (ProductId, ProductCode, Name, CategoryId, ManufacturerId, SupplierId, Origin, VolumeMl, AlcoholPct, SellPrice, StockQuantity, MinStockAlert, ImageUrl) VALUES
('CCCC0001-0000-0000-0000-000000000001',N'SP001',N'Vang đỏ Château Margaux 2018',1,'AAAA0001-0000-0000-0000-000000000001','BBBB0001-0000-0000-0000-000000000001',N'Pháp',750,13.5,3500000,0,5,N'/Images/vang đỏ/Cabernet Sauvignon.jpg'),
('CCCC0002-0000-0000-0000-000000000002',N'SP002',N'Johnnie Walker Black Label 750ml',4,'AAAA0002-0000-0000-0000-000000000002','BBBB0001-0000-0000-0000-000000000001',N'Scotland',750,40,850000,0,5,N'/Images/Whisky/Johnnie Walker Black Label.jpg'),
('CCCC0003-0000-0000-0000-000000000003',N'SP003',N'Absolut Vodka Original 700ml',5,'AAAA0003-0000-0000-0000-000000000003','BBBB0002-0000-0000-0000-000000000002',N'Thụy Điển',700,40,420000,0,10,N'/Images/Vodka/Absolut Vodka.jpg'),
('CCCC0004-0000-0000-0000-000000000004',N'SP004',N'Vang đỏ Đà Lạt Premium 750ml',1,'AAAA0004-0000-0000-0000-000000000004','BBBB0002-0000-0000-0000-000000000002',N'Việt Nam',750,12,180000,0,10,N'/Images/vang đỏ/Merlot.jpg'),
('CCCC0005-0000-0000-0000-000000000005',N'SP005',N'Heineken 330ml (thùng 24 lon)',6,'AAAA0005-0000-0000-0000-000000000005','BBBB0002-0000-0000-0000-000000000002',N'Hà Lan',330,5,380000,0,20,NULL),
('CCCC0006-0000-0000-0000-000000000006',N'SP006',N'Glenfiddich 12 Year Single Malt 700ml',4,'AAAA0006-0000-0000-0000-000000000006','BBBB0001-0000-0000-0000-000000000001',N'Scotland',700,40,1200000,0,3,N'/Images/Whisky/Glenfiddich 12.jpg'),
('CCCC0007-0000-0000-0000-000000000007',N'SP007',N'Rượu Lúa Mới 29 độ 500ml',7,'AAAA0007-0000-0000-0000-000000000007','BBBB0003-0000-0000-0000-000000000003',N'Việt Nam',500,29,85000,0,20,NULL),
('CCCC0008-0000-0000-0000-000000000008',N'SP008',N'Vang trắng Sauvignon Blanc 750ml',2,'AAAA0001-0000-0000-0000-000000000001','BBBB0001-0000-0000-0000-000000000001',N'Pháp',750,12.5,420000,0,5,N'/Images/vang trắng/Sauvignon Blanc.jpg'),
('CCCC0009-0000-0000-0000-000000000009',N'SP009',N'Absolut Vodka Citron 700ml',5,'AAAA0003-0000-0000-0000-000000000003','BBBB0001-0000-0000-0000-000000000001',N'Thụy Điển',700,40,450000,0,5,N'/Images/Vodka/Absolut Vodka.jpg'),
('CCCC0010-0000-0000-0000-000000000010',N'SP010',N'Johnnie Walker Red Label 1L',4,'AAAA0002-0000-0000-0000-000000000002','BBBB0003-0000-0000-0000-000000000003',N'Scotland',1000,40,750000,0,8,N'/Images/Whisky/Johnnie Walker Red Label.jpg');

-- Update ImageUrl for products (for databases that already have the data)
UPDATE dbo.Products SET ImageUrl = N'/Images/vang đỏ/Cabernet Sauvignon.jpg' WHERE ProductCode = N'SP001';
UPDATE dbo.Products SET ImageUrl = N'/Images/Whisky/Johnnie Walker Black Label.jpg' WHERE ProductCode = N'SP002';
UPDATE dbo.Products SET ImageUrl = N'/Images/Vodka/Absolut Vodka.jpg' WHERE ProductCode = N'SP003';
UPDATE dbo.Products SET ImageUrl = N'/Images/vang đỏ/Merlot.jpg' WHERE ProductCode = N'SP004';
UPDATE dbo.Products SET ImageUrl = NULL WHERE ProductCode = N'SP005';
UPDATE dbo.Products SET ImageUrl = N'/Images/Whisky/Glenfiddich 12.jpg' WHERE ProductCode = N'SP006';
UPDATE dbo.Products SET ImageUrl = NULL WHERE ProductCode = N'SP007';
UPDATE dbo.Products SET ImageUrl = N'/Images/vang trắng/Sauvignon Blanc.jpg' WHERE ProductCode = N'SP008';
UPDATE dbo.Products SET ImageUrl = N'/Images/Vodka/Absolut Vodka.jpg' WHERE ProductCode = N'SP009';
UPDATE dbo.Products SET ImageUrl = N'/Images/Whisky/Johnnie Walker Red Label.jpg' WHERE ProductCode = N'SP010';
GO

INSERT INTO dbo.WarehouseLocations (LocationCode, Zone, Shelf, Compartment) VALUES
(N'A-K1-N1',N'A',N'K1',N'N1'),(N'A-K1-N2',N'A',N'K1',N'N2'),(N'A-K2-N1',N'A',N'K2',N'N1'),
(N'B-K1-N1',N'B',N'K1',N'N1'),(N'B-K1-N2',N'B',N'K1',N'N2'),(N'C-K1-N1',N'C',N'K1',N'N1');

INSERT INTO dbo.StockBatches (ProductId, LocationId, ImportPrice, QuantityIn, QuantityRemain, ManufactureYear, ExpiryDate, ImportDate, ImportedBy, Status) VALUES
('CCCC0001-0000-0000-0000-000000000001',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'A-K1-N1'),2800000,20,20,2018,'2030-12-31','2026-01-10','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0002-0000-0000-0000-000000000002',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'A-K1-N2'),680000,35,35,2023,'2035-06-30','2026-01-15','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0003-0000-0000-0000-000000000003',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'A-K2-N1'),330000,50,50,2024,'2036-01-01','2026-01-20','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0004-0000-0000-0000-000000000004',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'B-K1-N1'),140000,60,60,2024,'2028-05-31','2026-02-01','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0005-0000-0000-0000-000000000005',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'B-K1-N2'),290000,80,80,2026,'2027-08-15','2026-03-01','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0006-0000-0000-0000-000000000006',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'C-K1-N1'),960000,15,15,2022,'2040-01-01','2026-01-05','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0007-0000-0000-0000-000000000007',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'B-K1-N1'),65000,100,100,2025,'2028-12-31','2026-02-10','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0008-0000-0000-0000-000000000008',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'A-K1-N1'),320000,25,25,2024,'2029-03-01','2026-01-18','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0009-0000-0000-0000-000000000009',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'A-K2-N1'),350000,30,30,2024,'2036-01-01','2026-02-05','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0010-0000-0000-0000-000000000010',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'A-K1-N2'),590000,40,40,2023,'2035-12-31','2026-01-25','22222222-2222-2222-2222-222222222222',N'active'),
('CCCC0004-0000-0000-0000-000000000004',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'B-K1-N1'),130000,10,3,2021,'2024-12-31','2024-12-01','22222222-2222-2222-2222-222222222222',N'expired'),
('CCCC0005-0000-0000-0000-000000000005',(SELECT LocationId FROM dbo.WarehouseLocations WHERE LocationCode=N'B-K1-N2'),280000,5,5,2024,'2025-03-01','2025-02-01','22222222-2222-2222-2222-222222222222',N'expired');

INSERT INTO dbo.Customers (CustomerId, Cccd, FullName, DateOfBirth, Gender, Address, Phone, Email, Occupation) VALUES
('DDDD0001-0000-0000-0000-000000000001',N'079199001234',N'Nguyễn Văn An','1990-05-15',N'male',N'12 Trần Phú, Nha Trang',N'0901234001',N'an.nguyen@gmail.com',N'Kỹ sư'),
('DDDD0002-0000-0000-0000-000000000002',N'079198002345',N'Trần Thị Bình','1988-11-20',N'female',N'34 Lê Lợi, Nha Trang',N'0901234002',N'binh.tran@gmail.com',N'Giáo viên'),
('DDDD0003-0000-0000-0000-000000000003',N'079200003456',N'Lê Minh Cường','2000-07-08',N'male',N'56 Nguyễn Thiện Thuật, Nha Trang',N'0901234003',N'cuong.le@gmail.com',N'Sinh viên'),
('DDDD0004-0000-0000-0000-000000000004',N'079195004567',N'Phạm Thị Dung','1995-03-25',N'female',N'78 Bạch Đằng, Cam Ranh',N'0901234004',N'dung.pham@gmail.com',N'Doanh nhân'),
('DDDD0005-0000-0000-0000-000000000005',N'079185005678',N'Hoàng Ngọc Hùng','1985-09-12',N'male',N'90 Hùng Vương, Ninh Hòa',N'0901234005',N'hung.hoang@gmail.com',N'Bác sĩ');

INSERT INTO dbo.MemberCards (CardId, CardNumber, CustomerId, CardType, IssueDate, ExpiryDate, FeePaid, IssuedBy, Status) VALUES
('EEEE0001-0000-0000-0000-000000000001',N'TV00010001','DDDD0001-0000-0000-0000-000000000001',N'silver','2025-01-10','2026-01-10',20000,'44444444-4444-4444-4444-444444444444',N'active'),
('EEEE0002-0000-0000-0000-000000000002',N'TV00010002','DDDD0002-0000-0000-0000-000000000002',N'gold','2025-03-15','2026-03-15',20000,'44444444-4444-4444-4444-444444444444',N'active'),
('EEEE0003-0000-0000-0000-000000000003',N'TV00010003','DDDD0003-0000-0000-0000-000000000003',N'diamond','2025-06-01','2026-06-01',20000,'44444444-4444-4444-4444-444444444444',N'active'),
('EEEE0004-0000-0000-0000-000000000004',N'TV00010004','DDDD0004-0000-0000-0000-000000000004',N'silver','2024-12-01','2025-12-01',20000,'44444444-4444-4444-4444-444444444444',N'expired'),
('EEEE0005-0000-0000-0000-000000000005',N'TV00010005','DDDD0005-0000-0000-0000-000000000005',N'gold','2026-01-01','2027-01-01',20000,'44444444-4444-4444-4444-444444444444',N'active');

INSERT INTO dbo.Invoices (InvoiceId, InvoiceNumber, CashierId, CardId, CustomerName, SaleDate, SaleType, TotalBeforeDiscount, DiscountPct, DiscountAmount, TotalAmount, PaymentMethod, Status) VALUES
('FFFF0001-0000-0000-0000-000000000001',N'HD20260510-0001','44444444-4444-4444-4444-444444444444','EEEE0002-0000-0000-0000-000000000002',N'Trần Thị Bình','2026-05-10 10:30:00',N'direct',1270000,10,127000,1143000,N'cash',N'completed'),
('FFFF0002-0000-0000-0000-000000000002',N'HD20260515-0002','55555555-5555-5555-5555-555555555555',NULL,N'Khách vãng lai','2026-05-15 14:00:00',N'direct',420000,0,0,420000,N'transfer',N'completed'),
('FFFF0003-0000-0000-0000-000000000003',N'HD20260520-0003','44444444-4444-4444-4444-444444444444','EEEE0003-0000-0000-0000-000000000003',N'Lê Minh Cường','2026-05-20 09:15:00',N'direct',3500000,15,525000,2975000,N'card',N'completed'),
('FFFF0004-0000-0000-0000-000000000004',N'HD20260522-0004','44444444-4444-4444-4444-444444444444',NULL,N'Phạm Thị Dung','2026-05-22 16:00:00',N'direct',850000,0,0,850000,N'cash',N'cancelled'),
('FFFF0005-0000-0000-0000-000000000005',N'HD20260524-0005','55555555-5555-5555-5555-555555555555','EEEE0001-0000-0000-0000-000000000001',N'Nguyễn Văn An','2026-05-24 11:00:00',N'direct',505000,5,25250,479750,N'cash',N'completed');

INSERT INTO dbo.InvoiceItems (InvoiceId, ProductId, ProductName, UnitPrice, Quantity, Subtotal) VALUES
('FFFF0001-0000-0000-0000-000000000001','CCCC0002-0000-0000-0000-000000000002',N'Johnnie Walker Black Label 750ml',850000,1,850000),
('FFFF0001-0000-0000-0000-000000000001','CCCC0004-0000-0000-0000-000000000004',N'Vang đỏ Đà Lạt Premium 750ml',180000,2,360000),
('FFFF0002-0000-0000-0000-000000000002','CCCC0003-0000-0000-0000-000000000003',N'Absolut Vodka Original 700ml',420000,1,420000),
('FFFF0003-0000-0000-0000-000000000001','CCCC0001-0000-0000-0000-000000000001',N'Vang đỏ Château Margaux 2018',3500000,1,3500000);
GO

INSERT INTO dbo.AccessLogs (UserId, IpAddress, Status, Note) VALUES
('11111111-1111-1111-1111-111111111111', N'127.0.0.1', N'success', N'Seed log');
GO

/* ===================== TEST QUERY ===================== */
-- SELECT * FROM dbo.FnReportStockWarning(NULL);
-- SELECT * FROM dbo.FnReportRevenue('2026-05-01','2026-05-31');
-- SELECT dbo.FnStockAvailable('CCCC0001-0000-0000-0000-000000000001', 5);
-- Tài khoản mẫu: admin / Admin@123
