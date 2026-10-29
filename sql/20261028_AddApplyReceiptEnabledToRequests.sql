-- Equivalent of EF migration 20261028120000_AddApplyReceiptEnabledToRequests.
-- Use this only if you apply schema changes by script instead of `dotnet ef database update`.
-- Safe to run more than once.

IF COL_LENGTH('dbo.Requests', 'ApplyReceiptEnabled') IS NULL
BEGIN
    ALTER TABLE dbo.Requests
        ADD ApplyReceiptEnabled bit NOT NULL
        CONSTRAINT DF_Requests_ApplyReceiptEnabled DEFAULT (0);
END
GO

-- Backfill: requests that already have saved invoice applications were submitted with Apply Receipt on.
UPDATE r
SET r.ApplyReceiptEnabled = 1
FROM dbo.Requests r
WHERE EXISTS (
    SELECT 1 FROM dbo.ReceiptInvoiceApplications a
    WHERE a.RequestId = r.Id AND a.IsActive = 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = '20261028120000_AddApplyReceiptEnabledToRequests')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20261028120000_AddApplyReceiptEnabledToRequests', '9.0.9');
GO
