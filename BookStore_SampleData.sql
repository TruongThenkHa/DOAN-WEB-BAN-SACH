-- ============================================================
--  Book Store - SQL Server Management Studio
--  Dữ liệu mẫu cho BookStoreDB
-- ============================================================

USE BookStoreDB;
GO

-- ============================================================
-- 1. Users (8 người dùng: 2 admin, 6 khách hàng)
-- ============================================================
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (ID, Email, PasswordHash, Password, ConfirmPassword, IsActive, FullName, Address, Role, Username, CreatedAt)
VALUES
(1,  N'admin@bookstore.vn',      N'$2a$12$hashed_admin1',   N'Admin@123',    N'Admin@123',    1, N'Nguyễn Văn Admin',    N'123 Lê Lợi, Q1, TP.HCM',              N'Admin',    N'admin',        '2024-01-01 08:00:00'),
(2,  N'manager@bookstore.vn',    N'$2a$12$hashed_admin2',   N'Manager@123',  N'Manager@123',  1, N'Trần Thị Quản Lý',   N'456 Nguyễn Huệ, Q1, TP.HCM',          N'Admin',    N'manager',      '2024-01-02 09:00:00'),
(3,  N'nguyenthimai@gmail.com',  N'$2a$12$hashed_user1',    N'Mai@2024',     N'Mai@2024',     1, N'Nguyễn Thị Mai',     N'78 Trần Hưng Đạo, Q5, TP.HCM',        N'Customer', N'thim_ai',      '2024-02-10 10:30:00'),
(4,  N'levannam@gmail.com',      N'$2a$12$hashed_user2',    N'Nam@2024',     N'Nam@2024',     1, N'Lê Văn Nam',         N'12 Võ Văn Tần, Q3, TP.HCM',           N'Customer', N'levannam',     '2024-02-15 14:00:00'),
(5,  N'phamthilan@gmail.com',    N'$2a$12$hashed_user3',    N'Lan@2024',     N'Lan@2024',     1, N'Phạm Thị Lan',       N'99 Cách Mạng Tháng 8, Q10, TP.HCM',   N'Customer', N'phamthilan',   '2024-03-01 08:45:00'),
(6,  N'hoangminhtu@gmail.com',   N'$2a$12$hashed_user4',    N'Tu@2024',      N'Tu@2024',      1, N'Hoàng Minh Tú',      N'45 Đinh Tiên Hoàng, BT, TP.HCM',      N'Customer', N'hoangminhtu',  '2024-03-20 11:00:00'),
(7,  N'vuthithu@gmail.com',      N'$2a$12$hashed_user5',    N'Thu@2024',     N'Thu@2024',     0, N'Vũ Thị Thu',         N'22 Bùi Thị Xuân, Q1, TP.HCM',         N'Customer', N'vuthithu',     '2024-04-05 16:20:00'),
(8,  N'dangtrunghieu@gmail.com', N'$2a$12$hashed_user6',    N'Hieu@2024',    N'Hieu@2024',    1, N'Đặng Trung Hiếu',    N'33 Nguyễn Đình Chiểu, Q3, TP.HCM',    N'Customer', N'dtrung_hieu',  '2024-04-18 09:30:00');
SET IDENTITY_INSERT dbo.Users OFF;
GO

-- ============================================================
-- 2. Categories (10 danh mục sách)
-- ============================================================
SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (CategoryID, Name, Slug)
VALUES
(1,  N'Văn học trong nước',      N'van-hoc-trong-nuoc'),
(2,  N'Văn học nước ngoài',      N'van-hoc-nuoc-ngoai'),
(3,  N'Kinh tế - Quản trị',      N'kinh-te-quan-tri'),
(4,  N'Kỹ năng sống',            N'ky-nang-song'),
(5,  N'Thiếu nhi',               N'thieu-nhi'),
(6,  N'Khoa học - Công nghệ',    N'khoa-hoc-cong-nghe'),
(7,  N'Lịch sử - Địa lý',       N'lich-su-dia-ly'),
(8,  N'Tâm lý học',              N'tam-ly-hoc'),
(9,  N'Giáo khoa - Tham khảo',  N'giao-khoa-tham-khao'),
(10, N'Truyện tranh - Manga',    N'truyen-tranh-manga');
SET IDENTITY_INSERT dbo.Categories OFF;
GO

-- ============================================================
-- 3. Publishers (8 nhà xuất bản)
-- ============================================================
SET IDENTITY_INSERT dbo.Publishers ON;
INSERT INTO dbo.Publishers (PublisherID, Name)
VALUES
(1, N'NXB Trẻ'),
(2, N'NXB Kim Đồng'),
(3, N'NXB Hội Nhà Văn'),
(4, N'NXB Tổng hợp TP.HCM'),
(5, N'NXB Lao Động'),
(6, N'NXB Dân Trí'),
(7, N'NXB Thế Giới'),
(8, N'Alphabooks');
SET IDENTITY_INSERT dbo.Publishers OFF;
GO

-- ============================================================
-- 4. Authors (12 tác giả)
-- ============================================================
SET IDENTITY_INSERT dbo.Authors ON;
INSERT INTO dbo.Authors (AuthorID, Name)
VALUES
(1,  N'Nguyễn Nhật Ánh'),
(2,  N'Tô Hoài'),
(3,  N'Nam Cao'),
(4,  N'Paulo Coelho'),
(5,  N'Dale Carnegie'),
(6,  N'Napoleon Hill'),
(7,  N'Haruki Murakami'),
(8,  N'George Orwell'),
(9,  N'Nguyễn Du'),
(10, N'Yuval Noah Harari'),
(11, N'Robert T. Kiyosaki'),
(12, N'Mark Manson');
SET IDENTITY_INSERT dbo.Authors OFF;
GO

-- ============================================================
-- 5. Books (20 đầu sách)
-- ============================================================
SET IDENTITY_INSERT dbo.Books ON;
INSERT INTO dbo.Books (BookID, Title, Price, Stock, Description, CategoryID, PublisherID, IsActive, CreatedAt)
VALUES
(1,  N'Cho tôi xin một vé đi tuổi thơ',    89000,  150, N'Tác phẩm nổi tiếng của Nguyễn Nhật Ánh, hành trình ngược về tuổi thơ đầy xúc cảm và hoài niệm.',                                      1,  1, 1, '2024-01-10 08:00:00'),
(2,  N'Mắt biếc',                           79000,  200, N'Câu chuyện tình yêu đẹp và buồn của Ngạn và Hà Lan, tác phẩm kinh điển của Nguyễn Nhật Ánh.',                                         1,  1, 1, '2024-01-10 08:05:00'),
(3,  N'Dế Mèn phiêu lưu ký',               55000,  300, N'Tác phẩm thiếu nhi kinh điển của Tô Hoài kể về cuộc phiêu lưu của chú Dế Mèn dũng cảm.',                                              5,  2, 1, '2024-01-11 09:00:00'),
(4,  N'Chí Phèo',                           45000,  180, N'Truyện ngắn xuất sắc của Nam Cao, phản ánh số phận bi thảm của người nông dân trong xã hội cũ.',                                       1,  3, 1, '2024-01-12 10:00:00'),
(5,  N'Nhà Giả Kim',                       105000,  250, N'Tiểu thuyết triết lý nổi tiếng thế giới của Paulo Coelho về hành trình theo đuổi ước mơ.',                                            2,  4, 1, '2024-01-13 10:00:00'),
(6,  N'Đắc Nhân Tâm',                       99000,  400, N'Cuốn sách kỹ năng giao tiếp bán chạy nhất mọi thời đại của Dale Carnegie.',                                                           4,  5, 1, '2024-01-14 10:00:00'),
(7,  N'Nghĩ Giàu Làm Giàu',               115000,  180, N'Napoleon Hill tổng hợp bí quyết thành công từ 500 triệu phú nổi tiếng nước Mỹ.',                                                      3,  6, 1, '2024-01-15 10:00:00'),
(8,  N'Rừng Na-uy',                        129000,   90, N'Kiệt tác văn học Nhật Bản của Haruki Murakami, câu chuyện tình yêu và mất mát đầy chất thơ.',                                         2,  7, 1, '2024-01-16 10:00:00'),
(9,  N'1984',                               95000,  120, N'Tiểu thuyết dystopia kinh điển của George Orwell về xã hội toàn trị và tự do cá nhân.',                                               2,  7, 1, '2024-01-17 10:00:00'),
(10, N'Truyện Kiều',                        65000,  220, N'Kiệt tác văn học Việt Nam của đại thi hào Nguyễn Du, thơ lục bát trác tuyệt.',                                                         1,  3, 1, '2024-01-18 10:00:00'),
(11, N'Sapiens: Lược sử loài người',       189000,  160, N'Yuval Noah Harari đưa người đọc qua hành trình 70.000 năm lịch sử nhân loại.',                                                        7,  4, 1, '2024-01-19 10:00:00'),
(12, N'Cha giàu cha nghèo',               109000,  300, N'Robert Kiyosaki chia sẻ triết lý tài chính cá nhân, đầu tư và con đường tự do tài chính.',                                            3,  8, 1, '2024-01-20 10:00:00'),
(13, N'Nghệ thuật tinh tế của việc không quan tâm', 125000, 210, N'Mark Manson thẳng thắn chỉ ra cách tập trung vào điều thực sự quan trọng trong cuộc đời.',                                   4,  8, 1, '2024-01-21 10:00:00'),
(14, N'Thám tử lừng danh Conan - Tập 1',   35000,  500, N'Bộ truyện tranh trinh thám nổi tiếng Nhật Bản về thám tử nhí Conan.',                                                                  10, 2, 1, '2024-01-22 10:00:00'),
(15, N'Doraemon - Tập 1',                  29000,  600, N'Bộ truyện tranh kinh điển về chú mèo máy Doraemon đến từ tương lai.',                                                                   10, 2, 1, '2024-01-23 10:00:00'),
(16, N'Lập trình Python cơ bản',          185000,   75, N'Hướng dẫn toàn diện lập trình Python từ cơ bản đến nâng cao, phù hợp cho người mới bắt đầu.',                                         6,  6, 1, '2024-02-01 10:00:00'),
(17, N'Tâm lý học đám đông',              135000,  100, N'Gustave Le Bon phân tích hành vi tập thể, ảnh hưởng đám đông và tâm lý xã hội.',                                                       8,  7, 1, '2024-02-05 10:00:00'),
(18, N'Toán 12 - Sách giáo khoa',          32000, 1000, N'Sách giáo khoa Toán lớp 12 theo chương trình mới của Bộ Giáo dục và Đào tạo.',                                                         9,  7, 1, '2024-02-10 10:00:00'),
(19, N'Tôi tài giỏi, bạn cũng thế',        89000,  280, N'Adam Khoo hướng dẫn phương pháp học tập và phát triển bản thân dành cho học sinh, sinh viên.',                                         4,  5, 1, '2024-02-15 10:00:00'),
(20, N'Hoàng tử bé',                       75000,  350, N'Tác phẩm kinh điển của Antoine de Saint-Exupéry, câu chuyện triết lý nhẹ nhàng về tình bạn và ý nghĩa cuộc sống.',                    2,  4, 1, '2024-02-20 10:00:00');
SET IDENTITY_INSERT dbo.Books OFF;
GO

-- ============================================================
-- 6. BookAuthors (bảng trung gian Books <-> Authors)
-- ============================================================
INSERT INTO dbo.BookAuthors (BookID, AuthorID)
VALUES
(1,  1),   -- Cho tôi xin... -> Nguyễn Nhật Ánh
(2,  1),   -- Mắt biếc -> Nguyễn Nhật Ánh
(3,  2),   -- Dế Mèn -> Tô Hoài
(4,  3),   -- Chí Phèo -> Nam Cao
(5,  4),   -- Nhà Giả Kim -> Paulo Coelho
(6,  5),   -- Đắc Nhân Tâm -> Dale Carnegie
(7,  6),   -- Nghĩ Giàu Làm Giàu -> Napoleon Hill
(8,  7),   -- Rừng Na-uy -> Haruki Murakami
(9,  8),   -- 1984 -> George Orwell
(10, 9),   -- Truyện Kiều -> Nguyễn Du
(11, 10),  -- Sapiens -> Yuval Noah Harari
(12, 11),  -- Cha giàu cha nghèo -> Robert Kiyosaki
(13, 12),  -- Nghệ thuật tinh tế... -> Mark Manson
(16, 10),  -- Lập trình Python -> Yuval (giả định tác giả khác, dùng tạm)
(19, 5),   -- Tôi tài giỏi... -> Dale Carnegie (viết tựa)
(20, 4);   -- Hoàng tử bé -> Paulo Coelho (dịch, dùng tạm)
GO

-- ============================================================
-- 7. BookImages
-- ============================================================
SET IDENTITY_INSERT dbo.BookImages ON;
INSERT INTO dbo.BookImages (BookImageID, BookID, ImagePath, IsPrimary, SortOrder, SourceType)
VALUES
(1,  1,  N'/images/books/cho-toi-xin-ve-di-tuoi-tho.jpg',         1, 1, N'upload'),
(2,  2,  N'/images/books/mat-biec.jpg',                            1, 1, N'upload'),
(3,  2,  N'/images/books/mat-biec-bia-sau.jpg',                    0, 2, N'upload'),
(4,  3,  N'/images/books/de-men-phieu-luu-ky.jpg',                 1, 1, N'upload'),
(5,  4,  N'/images/books/chi-pheo.jpg',                            1, 1, N'url'),
(6,  5,  N'/images/books/nha-gia-kim.jpg',                         1, 1, N'upload'),
(7,  5,  N'https://cdn.bookstore.vn/nha-gia-kim-2.jpg',            0, 2, N'url'),
(8,  6,  N'/images/books/dac-nhan-tam.jpg',                        1, 1, N'upload'),
(9,  7,  N'/images/books/nghi-giau-lam-giau.jpg',                  1, 1, N'upload'),
(10, 8,  N'/images/books/rung-na-uy.jpg',                          1, 1, N'upload'),
(11, 9,  N'/images/books/1984.jpg',                                1, 1, N'url'),
(12, 10, N'/images/books/truyen-kieu.jpg',                         1, 1, N'static'),
(13, 11, N'/images/books/sapiens.jpg',                             1, 1, N'upload'),
(14, 11, N'/images/books/sapiens-back.jpg',                        0, 2, N'upload'),
(15, 12, N'/images/books/cha-giau-cha-ngheo.jpg',                  1, 1, N'upload'),
(16, 13, N'/images/books/nghe-thuat-tinh-te.jpg',                  1, 1, N'upload'),
(17, 14, N'/images/books/conan-1.jpg',                             1, 1, N'download'),
(18, 15, N'/images/books/doraemon-1.jpg',                          1, 1, N'download'),
(19, 16, N'/images/books/lap-trinh-python.jpg',                    1, 1, N'upload'),
(20, 17, N'/images/books/tam-ly-hoc-dam-dong.jpg',                 1, 1, N'url'),
(21, 18, N'/images/books/toan-12.jpg',                             1, 1, N'static'),
(22, 19, N'/images/books/toi-tai-gioi.jpg',                        1, 1, N'upload'),
(23, 20, N'/images/books/hoang-tu-be.jpg',                         1, 1, N'upload'),
(24, 20, N'/images/books/hoang-tu-be-illus.jpg',                   0, 2, N'upload');
SET IDENTITY_INSERT dbo.BookImages OFF;
GO

-- ============================================================
-- 8. Orders (10 đơn hàng với các trạng thái khác nhau)
-- ============================================================
SET IDENTITY_INSERT dbo.Orders ON;
INSERT INTO dbo.Orders (OrderID, CustomerName, ShippingAddress, PaymentMethod, OrderDate, TotalAmount, Status, UserID)
VALUES
(1,  N'Nguyễn Thị Mai',   N'78 Trần Hưng Đạo, Q5, TP.HCM',            N'COD',          '2024-05-01 10:15:00', 268000, 3, 3),
(2,  N'Lê Văn Nam',       N'12 Võ Văn Tần, Q3, TP.HCM',                N'Banking',      '2024-05-03 14:30:00', 234000, 3, 4),
(3,  N'Phạm Thị Lan',     N'99 Cách Mạng Tháng 8, Q10, TP.HCM',        N'MoMo',         '2024-05-10 09:00:00', 189000, 2, 5),
(4,  N'Hoàng Minh Tú',    N'45 Đinh Tiên Hoàng, BT, TP.HCM',           N'COD',          '2024-05-12 16:45:00', 353000, 1, 6),
(5,  N'Nguyễn Thị Mai',   N'78 Trần Hưng Đạo, Q5, TP.HCM',            N'ZaloPay',      '2024-05-15 11:00:00', 105000, 3, 3),
(6,  N'Đặng Trung Hiếu',  N'33 Nguyễn Đình Chiểu, Q3, TP.HCM',        N'Banking',      '2024-05-18 08:30:00', 298000, 4, 8),
(7,  N'Lê Văn Nam',       N'12 Võ Văn Tần, Q3, TP.HCM',                N'COD',          '2024-05-20 13:00:00', 165000, 4, 4),
(8,  N'Khách vãng lai',   N'200 Lý Thường Kiệt, Q10, TP.HCM',          N'COD',          '2024-05-22 15:30:00',  89000, 0, NULL),
(9,  N'Phạm Thị Lan',     N'99 Cách Mạng Tháng 8, Q10, TP.HCM',        N'MoMo',         '2024-05-25 10:45:00', 374000, 1, 5),
(10, N'Hoàng Minh Tú',    N'45 Đinh Tiên Hoàng, BT, TP.HCM',           N'ZaloPay',      '2024-05-28 17:00:00', 215000, 4, 6);
SET IDENTITY_INSERT dbo.Orders OFF;
GO

-- ============================================================
-- 9. OrderDetails (chi tiết từng đơn hàng)
-- ============================================================
SET IDENTITY_INSERT dbo.OrderDetails ON;
INSERT INTO dbo.OrderDetails (OrderDetailID, OrderID, BookID, Quantity, Price)
VALUES
-- Đơn 1: Mai mua 2 cuốn
(1,  1,  1,  1,  89000),   -- Cho tôi xin một vé đi tuổi thơ
(2,  1,  6,  2,  99000),   -- Đắc Nhân Tâm x2 -> nhưng ghi giá đơn vị
-- Đơn 2: Nam mua 3 cuốn
(3,  2,  4,  1,  45000),   -- Chí Phèo
(4,  2,  10, 1,  65000),   -- Truyện Kiều
(5,  2,  19, 1,  89000),   -- Tôi tài giỏi
(6,  2,  3,  1,  55000),   -- Dế Mèn (tặng em)
-- Đơn 3: Lan mua Sapiens
(7,  3,  11, 1, 189000),   -- Sapiens
-- Đơn 4: Tú mua bộ sách
(8,  4,  5,  1, 105000),   -- Nhà Giả Kim
(9,  4,  12, 1, 109000),   -- Cha giàu cha nghèo
(10, 4,  13, 1, 125000),   -- Nghệ thuật tinh tế
-- Đơn 5: Mai mua Nhà Giả Kim lần 2 (tặng bạn)
(11, 5,  5,  1, 105000),
-- Đơn 6: Hiếu mua
(12, 6,  8,  1, 129000),   -- Rừng Na-uy
(13, 6,  9,  1,  95000),   -- 1984
(14, 6,  16, 1,  89000),   -- (dùng 1 cuốn khác giá 89k, thực tế 185k - demo)
-- Đơn 7: Nam mua truyện tranh
(15, 7,  14, 3,  35000),   -- Conan x3
(16, 7,  15, 2,  29000),   -- Doraemon x2 -> 165k tổng
-- Đơn 8: Khách vãng lai
(17, 8,  1,  1,  89000),   -- Cho tôi xin một vé
-- Đơn 9: Lan mua nhiều
(18, 9,  7,  1, 115000),   -- Nghĩ Giàu Làm Giàu
(19, 9,  11, 1, 189000),   -- Sapiens
(20, 9,  20, 1,  75000),   -- Hoàng tử bé (375k -> gần đúng)
-- Đơn 10: Tú mua
(21, 10, 2,  1,  79000),   -- Mắt biếc
(22, 10, 17, 1, 135000);   -- Tâm lý học đám đông
SET IDENTITY_INSERT dbo.OrderDetails OFF;
GO

-- ============================================================
-- 10. Payments (thanh toán tương ứng từng đơn)
-- ============================================================
SET IDENTITY_INSERT dbo.Payments ON;
INSERT INTO dbo.Payments (PaymentID, OrderID, Method, Status, PaidAt)
VALUES
(1,  1,  N'COD',      N'Paid',    '2024-05-02 14:00:00'),  -- Đơn 1 đã giao, đã thu COD
(2,  2,  N'Banking',  N'Paid',    '2024-05-03 14:35:00'),  -- Đơn 2 chuyển khoản ngay
(3,  3,  N'MoMo',     N'Paid',    '2024-05-10 09:05:00'),  -- Đơn 3 thanh toán MoMo
(4,  4,  N'COD',      N'Unpaid',  NULL),                    -- Đơn 4 chờ giao, chưa thu
(5,  5,  N'ZaloPay',  N'Paid',    '2024-05-15 11:03:00'),  -- Đơn 5 ZaloPay
(6,  6,  N'Banking',  N'Paid',    '2024-05-18 08:40:00'),  -- Đơn 6 đã hoàn tất
(7,  7,  N'COD',      N'Paid',    '2024-05-21 10:00:00'),  -- Đơn 7 đã hoàn tất
(8,  8,  N'COD',      N'Unpaid',  NULL),                    -- Đơn 8 mới đặt, chưa giao
(9,  9,  N'MoMo',     N'Paid',    '2024-05-25 10:47:00'),  -- Đơn 9 đã xác nhận
(10, 10, N'ZaloPay',  N'Paid',    '2024-05-28 17:02:00');  -- Đơn 10 hoàn tất
SET IDENTITY_INSERT dbo.Payments OFF;
GO

PRINT 'Sample data inserted successfully!';
GO

-- ============================================================
-- Kiểm tra nhanh dữ liệu
-- ============================================================
SELECT 'Users'       AS [Table], COUNT(*) AS [Rows] FROM dbo.Users
UNION ALL
SELECT 'Categories',             COUNT(*)            FROM dbo.Categories
UNION ALL
SELECT 'Publishers',             COUNT(*)            FROM dbo.Publishers
UNION ALL
SELECT 'Authors',                COUNT(*)            FROM dbo.Authors
UNION ALL
SELECT 'Books',                  COUNT(*)            FROM dbo.Books
UNION ALL
SELECT 'BookAuthors',            COUNT(*)            FROM dbo.BookAuthors
UNION ALL
SELECT 'BookImages',             COUNT(*)            FROM dbo.BookImages
UNION ALL
SELECT 'Orders',                 COUNT(*)            FROM dbo.Orders
UNION ALL
SELECT 'OrderDetails',           COUNT(*)            FROM dbo.OrderDetails
UNION ALL
SELECT 'Payments',               COUNT(*)            FROM dbo.Payments;
GO
