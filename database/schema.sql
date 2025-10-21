-- =============================================
-- Transaction Upload Service - Database Schema
-- =============================================
-- This script creates the complete database structure
-- for the Transaction Upload Service application
-- =============================================

USE master;
GO

-- Drop database if exists (for clean setup)
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'TransactionsDb')
BEGIN
    ALTER DATABASE TransactionsDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TransactionsDb;
END
GO

-- Create new database
CREATE DATABASE TransactionsDb;
GO

USE TransactionsDb;
GO

-- =============================================
-- Table: imports
-- Purpose: Store file upload metadata and hash for idempotency
-- =============================================
CREATE TABLE imports (
    import_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    received_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    source_format NVARCHAR(10) NOT NULL,  -- 'CSV' or 'XML'
    sha256 NVARCHAR(100) NOT NULL,        -- File hash for duplicate detection
    record_count INT NOT NULL,
    status NVARCHAR(20) NOT NULL          -- 'Completed', 'Failed'
);
GO

-- =============================================
-- Table: transactions
-- Purpose: Store transaction data from uploaded files
-- =============================================
CREATE TABLE transactions (
    id NVARCHAR(50) PRIMARY KEY,
    amount DECIMAL(18,2) NOT NULL,
    currency_code CHAR(3) NOT NULL,       -- ISO4217 format (USD, EUR, GBP, etc.)
    transaction_date DATETIME2 NOT NULL,
    status_code CHAR(1) NOT NULL,         -- 'A'=Approved, 'R'=Rejected/Failed, 'D'=Done/Finished
    source_format NVARCHAR(10) NOT NULL,  -- 'CSV' or 'XML'
    import_id UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_transactions_imports FOREIGN KEY (import_id) 
        REFERENCES imports(import_id) ON DELETE CASCADE
);
GO

-- =============================================
-- Table: import_errors
-- Purpose: Store validation errors for failed uploads
-- =============================================
CREATE TABLE import_errors (
    id INT PRIMARY KEY IDENTITY(1,1),
    import_id UNIQUEIDENTIFIER NOT NULL,
    row_number INT NOT NULL,
    field NVARCHAR(50) NOT NULL,
    message NVARCHAR(500) NOT NULL,
    CONSTRAINT FK_import_errors_imports FOREIGN KEY (import_id) 
        REFERENCES imports(import_id) ON DELETE CASCADE
);
GO

-- =============================================
-- Indexes for Performance Optimization
-- =============================================

-- Index for idempotency check (prevent duplicate uploads)
CREATE UNIQUE INDEX IX_imports_sha256 ON imports(sha256);
GO

-- Index for import errors lookup
CREATE INDEX IX_import_errors_import_id ON import_errors(import_id);
GO

-- Indexes for transaction queries
CREATE INDEX IX_transactions_currency_code ON transactions(currency_code);
GO

CREATE INDEX IX_transactions_status_code ON transactions(status_code);
GO

CREATE INDEX IX_transactions_transaction_date ON transactions(transaction_date);
GO

CREATE INDEX IX_transactions_import_id ON transactions(import_id);
GO

-- =============================================
-- Verification
-- =============================================
PRINT '==============================================';
PRINT 'Database Schema Created Successfully';
PRINT '==============================================';
PRINT 'Tables Created:';
PRINT '  - imports';
PRINT '  - transactions';
PRINT '  - import_errors';
PRINT '';
PRINT 'Indexes Created:';
PRINT '  - IX_imports_sha256 (UNIQUE)';
PRINT '  - IX_import_errors_import_id';
PRINT '  - IX_transactions_currency_code';
PRINT '  - IX_transactions_status_code';
PRINT '  - IX_transactions_transaction_date';
PRINT '  - IX_transactions_import_id';
PRINT '==============================================';
GO

-- Display table structures
SELECT 'Table: imports' AS Info;
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'imports';
GO

SELECT 'Table: transactions' AS Info;
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'transactions';
GO

SELECT 'Table: import_errors' AS Info;
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'import_errors';
GO