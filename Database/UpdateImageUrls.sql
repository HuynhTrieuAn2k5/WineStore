-- ============================================================
-- CHILLIQUER – Cập nhật ImageUrl cho sản phẩm
-- Chạy script này trên database ChilliquerDB để fix ảnh sản phẩm
-- ============================================================
USE ChilliquerDB;
GO

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

PRINT N'Đã cập nhật ImageUrl cho tất cả sản phẩm!';
GO
