-- Create TinTuc table
CREATE TABLE [dbo].[TinTucs] (
    [TinTucId] int IDENTITY(1,1) NOT NULL,
    [TieuDe] nvarchar(200) NOT NULL,
    [MoTaNgan] nvarchar(500) NOT NULL,
    [NoiDung] nvarchar(max) NOT NULL,
    [AnhDaiDien] nvarchar(500) NULL,
    [NgayTao] datetime2 NOT NULL DEFAULT GETDATE(),
    [NgayCapNhat] datetime2 NULL,
    [NgayXuatBan] datetime2 NULL,
    [TacGiaId] int NULL,
    [TenTacGia] nvarchar(100) NULL,
    [LuotXem] int NOT NULL DEFAULT 0,
    [TrangThai] nvarchar(20) NOT NULL DEFAULT 'DRAFT',
    [NoiBat] bit NOT NULL DEFAULT 0,
    [Slug] nvarchar(200) NULL,
    [MetaTitle] nvarchar(160) NULL,
    [MetaDescription] nvarchar(160) NULL,
    [MetaKeywords] nvarchar(500) NULL,
    CONSTRAINT [PK_TinTucs] PRIMARY KEY ([TinTucId]),
    CONSTRAINT [FK_TinTucs_NguoiDungs_TacGiaId] 
        FOREIGN KEY ([TacGiaId]) REFERENCES [dbo].[NguoiDungs] ([NguoiDungId]) 
        ON DELETE SET NULL
);

-- Create indexes
CREATE UNIQUE INDEX [IX_TinTucs_Slug] ON [dbo].[TinTucs] ([Slug]) WHERE [Slug] IS NOT NULL;
CREATE INDEX [IX_TinTucs_TacGiaId] ON [dbo].[TinTucs] ([TacGiaId]);
CREATE INDEX [IX_TinTucs_TrangThai] ON [dbo].[TinTucs] ([TrangThai]);
CREATE INDEX [IX_TinTucs_NgayXuatBan] ON [dbo].[TinTucs] ([NgayXuatBan]);


-- Dữ liệu mẫu cho bảng TinTucs
INSERT INTO TinTucs (TieuDe, MoTaNgan, NoiDung, AnhDaiDien, NgayXuatBan, TrangThai, NoiBat, Slug, LuotXem, NgayTao, NgayCapNhat, TacGiaId, MetaTitle, MetaDescription, MetaKeywords) VALUES

-- Tin tức 1: Nổi bật
(N'Khai trương phòng gym hiện đại với trang thiết bị tối tân', 
 N'Chúng tôi tự hào giới thiệu phòng gym mới với hệ thống trang thiết bị hiện đại nhất, không gian rộng rãi và dịch vụ chuyên nghiệp.', 
 N'<h2>Phòng gym hiện đại nhất khu vực</h2><p>Sau nhiều tháng chuẩn bị, chúng tôi đã chính thức khai trương phòng gym với diện tích 1000m² cùng hệ thống trang thiết bị hiện đại từ các thương hiệu hàng đầu thế giới như Technogym, Life Fitness.</p><h3>Trang thiết bị nổi bật:</h3><ul><li>Hệ thống máy chạy bộ thông minh với màn hình cảm ứng</li><li>Khu vực tập tạ với đầy đủ máy móc chuyên nghiệp</li><li>Phòng tập group fitness với hệ thống âm thanh hiện đại</li><li>Khu vực yoga và pilates yên tĩnh</li></ul><p>Đội ngũ huấn luyện viên chuyên nghiệp của chúng tôi sẽ hỗ trợ bạn đạt được mục tiêu tập luyện một cách hiệu quả nhất.</p>', 
 '/images/tin-tuc/khai-truong-gym.jpg', 
 GETDATE(), 
 'PUBLISHED', 
 1, 
 'khai-truong-phong-gym-hien-dai-voi-trang-thiet-bi-toi-tan', 
 156, 
 GETDATE(), 
 GETDATE(), 
 1, 
 N'Khai trương phòng gym hiện đại - Trang thiết bị tối tân 2025', 
 N'Phòng gym mới khai trương với trang thiết bị hiện đại, không gian rộng rãi và dịch vụ chuyên nghiệp. Đăng ký ngay để nhận ưu đãi đặc biệt!', 
 N'phòng gym, khai trương, trang thiết bị hiện đại, fitness, tập luyện'),

-- Tin tức 2: Chương trình khuyến mãi
(N'Chương trình ưu đãi đặc biệt tháng 8 - Giảm giá lên đến 50%', 
 N'Nhân dịp khai trương, chúng tôi áp dụng chương trình ưu đãi đặc biệt với mức giảm giá lên đến 50% cho tất cả các gói tập.', 
 N'<h2>Ưu đãi cực khủng trong tháng 8!</h2><p>Để chào mừng sự kiện khai trương, GYM Manager mang đến chương trình ưu đãi đặc biệt dành cho các thành viên mới:</p><h3>🎯 Các gói ưu đãi hấp dẫn:</h3><ul><li><strong>Gói Basic (3 tháng):</strong> Chỉ 1.500.000đ (Giá gốc: 3.000.000đ) - Tiết kiệm 50%</li><li><strong>Gói Premium (6 tháng):</strong> Chỉ 4.500.000đ (Giá gốc: 7.200.000đ) - Tiết kiệm 37.5%</li><li><strong>Gói VIP (12 tháng):</strong> Chỉ 7.200.000đ (Giá gốc: 12.000.000đ) - Tiết kiệm 40%</li></ul><h3>🎁 Quà tặng kèm theo:</h3><ul><li>Tư vấn chế độ dinh dưỡng miễn phí</li><li>Đánh giá thể trạng ban đầu</li><li>Áo thun gym cao cấp</li><li>Bình nước thể thao</li></ul><p><strong>⏰ Thời gian áp dụng:</strong> Từ 01/08/2025 đến 31/08/2025</p><p>Liên hệ ngay hotline: <strong>1900-XXXX</strong> để đăng ký và nhận ưu đãi!</p>', 
 '/images/tin-tuc/chuong-trinh-uu-dai.jpg', 
 GETDATE(), 
 'PUBLISHED', 
 1, 
 'chuong-trinh-uu-dai-dac-biet-thang-8-giam-gia-len-den-50', 
 89, 
 GETDATE(), 
 GETDATE(), 
 1, 
 N'Ưu đãi đặc biệt tháng 8 - Giảm giá 50% gói tập gym', 
 N'Chương trình khuyến mãi lớn nhất năm! Giảm giá lên đến 50% tất cả gói tập gym kèm quà tặng hấp dẫn. Đăng ký ngay!', 
 N'ưu đãi gym, khuyến mãi, giảm giá gói tập, fitness, promotion'),

-- Tin tức 3: Lớp học mới
(N'Ra mắt lớp Yoga buổi sáng và Zumba buổi tối', 
 N'Chúng tôi chính thức ra mắt hai lớp học mới: Yoga buổi sáng giúp bạn bắt đầu ngày mới tràn đầy năng lượng và Zumba buổi tối để thư giãn sau ngày làm việc căng thẳng.', 
 N'<h2>Lớp học mới đầy thú vị!</h2><p>Để đáp ứng nhu cầu đa dạng của các thành viên, chúng tôi vui mừng ra mắt hai lớp học hoàn toàn mới:</p><h3>🧘‍♀️ Lớp Yoga buổi sáng</h3><ul><li><strong>Thời gian:</strong> 6:00 - 7:00 AM (Thứ 2, 4, 6)</li><li><strong>Huấn luyện viên:</strong> Ms. Linh - Chứng chỉ Yoga quốc tế</li><li><strong>Phù hợp:</strong> Mọi trình độ, đặc biệt tốt cho người mới bắt đầu</li><li><strong>Lợi ích:</strong> Tăng sự linh hoạt, giảm stress, cải thiện tư thế</li></ul><h3>💃 Lớp Zumba buổi tối</h3><ul><li><strong>Thời gian:</strong> 7:30 - 8:30 PM (Thứ 3, 5, 7)</li><li><strong>Huấn luyện viên:</strong> Mr. Nam - Chuyên gia Zumba với 5 năm kinh nghiệm</li><li><strong>Phù hợp:</strong> Những người yêu thích nhảy múa và âm nhạc</li><li><strong>Lợi ích:</strong> Đốt cháy calo hiệu quả, cải thiện thể lực, giải tỏa stress</li></ul><h3>💰 Học phí ưu đãi:</h3><ul><li>Thành viên gym: MIỄN PHÍ</li><li>Khách vãng lai: 50.000đ/buổi</li><li>Gói 12 buổi: 500.000đ (tiết kiệm 100.000đ)</li></ul><p>Đăng ký ngay tại quầy lễ tân hoặc qua hotline để đảm bảo chỗ!</p>', 
 '/images/tin-tuc/lop-yoga-zumba.jpg', 
 DATEADD(day, -3, GETDATE()), 
 'PUBLISHED', 
 0, 
 'ra-mat-lop-yoga-buoi-sang-va-zumba-buoi-toi', 
 67, 
 DATEADD(day, -3, GETDATE()), 
 DATEADD(day, -3, GETDATE()), 
 1, 
 N'Lớp Yoga buổi sáng và Zumba buổi tối mới ra mắt', 
 N'Ra mắt lớp Yoga buổi sáng và Zumba buổi tối với huấn luyện viên chuyên nghiệp. Đăng ký ngay để nhận ưu đãi đặc biệt!', 
 N'yoga, zumba, lớp học group, fitness class, tập nhóm'),

-- Tin tức 4: Thành tích thành viên
(N'Chúc mừng thành viên Nguyễn Văn A đạt giải Nhất cuộc thi Bodybuilding khu vực', 
 N'Anh Nguyễn Văn A, thành viên thân thiết của phòng gym, đã xuất sắc giành giải Nhất cuộc thi Bodybuilding khu vực Nam Bộ 2025 sau 2 năm tập luyện chăm chỉ.', 
 N'<h2>Thành tích đáng tự hào!</h2><p>Chúng tôi vô cùng tự hào khi thông báo anh Nguyễn Văn A - thành viên VIP của phòng gym đã giành chiến thắng cuộc thi Bodybuilding khu vực Nam Bộ 2025 hạng mục Men''s Physique.</p><h3>🏆 Hành trình chiến thắng:</h3><ul><li><strong>Bắt đầu:</strong> Tháng 6/2023 với cân nặng 65kg, chưa có kinh nghiệm tập luyện</li><li><strong>Mục tiêu:</strong> Tăng cơ bắp, cải thiện vóc dáng</li><li><strong>Quá trình:</strong> 24 tháng tập luyện đều đặn 6 ngày/tuần</li><li><strong>Kết quả:</strong> Tăng lên 78kg với tỷ lệ mỡ cơ thể chỉ 8%</li></ul><h3>💪 Bí quyết thành công:</h3><ul><li><strong>Kế hoạch tập luyện khoa học:</strong> Được thiết kế riêng bởi HLV Minh Tuấn</li><li><strong>Chế độ dinh dưỡng cân bằng:</strong> Tư vấn bởi chuyên gia dinh dưỡng</li><li><strong>Nghị lực kiên trì:</strong> Không bỏ lỡ bất kỳ buổi tập nào</li><li><strong>Hỗ trợ từ cộng đồng:</strong> Các thành viên và HLV luôn động viên</li></ul><h3>🎯 Chia sẻ từ anh Văn A:</h3><blockquote><p>"Tôi không bao giờ nghĩ mình có thể đạt được thành tích này. Cảm ơn đội ngũ HLV và các bạn thành viên đã luôn ủng hộ tôi. Gym không chỉ là nơi tập luyện mà còn là ngôi nhà thứ hai của tôi."</p></blockquote><p>Chúc mừng anh Văn A và hy vọng thành tích này sẽ truyền cảm hứng cho tất cả thành viên khác!</p>', 
 '/images/tin-tuc/thanh-vien-vo-dich.jpg', 
 DATEADD(day, -7, GETDATE()), 
 'PUBLISHED', 
 1, 
 'chuc-mung-thanh-vien-nguyen-van-a-dat-giai-nhat-cuoc-thi-bodybuilding-khu-vuc', 
 134, 
 DATEADD(day, -7, GETDATE()), 
 DATEADD(day, -7, GETDATE()), 
 1, 
 N'Thành viên đạt giải Nhất Bodybuilding - Thành tích đáng tự hào', 
 N'Thành viên Nguyễn Văn A giành giải Nhất cuộc thi Bodybuilding khu vực sau 2 năm tập luyện chăm chỉ tại gym. Cảm hứng cho mọi người!', 
 N'bodybuilding, thành viên, giải thưởng, thành tích, gym success story'),

-- Tin tức 5: Mẹo tập luyện
(N'5 bài tập cơ bản giúp người mới bắt đầu tập gym hiệu quả', 
 N'Nếu bạn là người mới bắt đầu tập gym, đây là 5 bài tập cơ bản mà bạn nên làm quen trước để xây dựng nền tảng thể lực vững chắc.', 
 N'<h2>Hướng dẫn cho người mới bắt đầu</h2><p>Việc bắt đầu tập gym có thể khiến nhiều người cảm thấy overwhelmed. Dưới đây là 5 bài tập cơ bản mà mọi người mới nên thành thạo:</p><h3>1. 🏃‍♂️ Squat (Gánh tạ ngồi)</h3><ul><li><strong>Mục tiêu:</strong> Cơ đùi, cơ mông, cơ lõi</li><li><strong>Cách thực hiện:</strong> Đứng thẳng, chân rộng bằng vai, từ từ hạ người xuống như ngồi ghế</li><li><strong>Lưu ý:</strong> Giữ lưng thẳng, đầu gối không được vượt quá mũi chân</li><li><strong>Số lần:</strong> 3 sets x 12-15 reps</li></ul><h3>2. 💪 Push-up (Hít đất)</h3><ul><li><strong>Mục tiêu:</strong> Cơ ngực, cơ vai, cơ tay sau</li><li><strong>Cách thực hiện:</strong> Nằm sấp, tay đặt rộng bằng vai, đẩy người lên xuống</li><li><strong>Biến thể:</strong> Hít đất trên đầu gối cho người mới</li><li><strong>Số lần:</strong> 3 sets x 8-12 reps</li></ul><h3>3. 🎯 Plank (Chống đẩy tĩnh)</h3><ul><li><strong>Mục tiêu:</strong> Cơ lõi, cơ vai, cơ lưng</li><li><strong>Cách thực hiện:</strong> Nằm sấp, chống tay và mũi chân, giữ thẳng người</li><li><strong>Lưu ý:</strong> Không để hông cao hoặc thấp</li><li><strong>Thời gian:</strong> 3 sets x 30-60 giây</li></ul><h3>4. 🚶‍♀️ Lunges (Chùng chân)</h3><ul><li><strong>Mục tiêu:</strong> Cơ đùi, cơ mông, cải thiện thensăng bằng</li><li><strong>Cách thực hiện:</strong> Bước chân về phía trước, hạ người xuống đến khi cả hai đầu gối góc 90°</li><li><strong>Số lần:</strong> 3 sets x 10 reps mỗi chân</li></ul><h3>5. 🏋️‍♀️ Deadlift (Nâng tạ đất)</h3><ul><li><strong>Mục tiêu:</strong> Cơ lưng, cơ đùi sau, cơ mông</li><li><strong>Cách thực hiện:</strong> Đứng thẳng, cầm tạ, hạ xuống bằng cách đẩy hông về sau</li><li><strong>Lưu ý:</strong> Luôn giữ lưng thẳng, tạ sát người</li><li><strong>Số lần:</strong> 3 sets x 8-10 reps</li></ul><h3>📝 Lịch tập gợi ý cho tuần đầu:</h3><ul><li><strong>Ngày 1:</strong> Squat + Push-up + Plank</li><li><strong>Ngày 2:</strong> Nghỉ hoặc đi bộ nhẹ</li><li><strong>Ngày 3:</strong> Lunges + Deadlift + Plank</li><li><strong>Ngày 4:</strong> Nghỉ</li><li><strong>Ngày 5:</strong> Tất cả 5 bài tập với cường độ nhẹ</li></ul><p><strong>💡 Lời khuyên:</strong> Hãy đến gym và nhờ HLV hướng dẫn để đảm bảo kỹ thuật chính xác!</p>', 
 '/images/tin-tuc/bai-tap-co-ban.jpg', 
 DATEADD(day, -10, GETDATE()), 
 'PUBLISHED', 
 0, 
 '5-bai-tap-co-ban-giup-nguoi-moi-bat-dau-tap-gym-hieu-qua', 
 92, 
 DATEADD(day, -10, GETDATE()), 
 DATEADD(day, -10, GETDATE()), 
 1, 
 N'5 bài tập cơ bản cho người mới bắt đầu tập gym', 
 N'Hướng dẫn chi tiết 5 bài tập cơ bản giúp người mới bắt đầu tập gym hiệu quả và an toàn. Xây dựng nền tảng thể lực vững chắc.', 
 N'bài tập gym, người mới bắt đầu, squat, push-up, plank, lunges, deadlift'),

-- Tin tức 6: Dinh dưỡng
(N'Thực đơn dinh dưỡng một tuần cho người tập gym tăng cơ', 
 N'Một chế độ dinh dưỡng khoa học là yếu tố then chốt giúp bạn đạt được mục tiêu tăng cơ. Cùng tham khảo thực đơn chi tiết cho 7 ngày.', 
 N'<h2>Dinh dưỡng - Chìa khóa thành công</h2><p>Nếu bạn đang tập luyện với mục tiêu tăng cơ, thì dinh dưỡng đóng vai trò 70% thành công. Dưới đây là thực đơn chi tiết cho một tuần:</p><h3>📊 Nguyên tắc cơ bản:</h3><ul><li><strong>Protein:</strong> 1.6-2.2g/kg cân nặng/ngày</li><li><strong>Carb:</strong> 3-5g/kg cân nặng/ngày</li><li><strong>Fat:</strong> 0.8-1.2g/kg cân nặng/ngày</li><li><strong>Nước:</strong> Tối thiểu 2.5-3 lít/ngày</li></ul><h3>🍽️ Thực đơn 7 ngày:</h3><h4>NGÀY 1 & 4: CHỦ NHẬT & THỨ TƯ</h4><ul><li><strong>Sáng:</strong> Yến mạch + chuối + sữa tươi + 2 lòng trắng trứng</li><li><strong>Phụ:</strong> Bánh mì nguyên cám + bơ đậu phộng</li><li><strong>Trưa:</strong> Cơm gạo lứt + ức gà nướng + rau củ</li><li><strong>Chiều:</strong> Sữa chua Hy Lạp + hạnh nhân</li><li><strong>Tối:</strong> Cá hồi nướng + khoai lang + salad</li></ul><h4>NGÀY 2 & 5: THỨ HAI & THỨ NĂM</h4><ul><li><strong>Sáng:</strong> Trứng chiên + bánh mì đen + avocado</li><li><strong>Phụ:</strong> Sinh tố whey protein + chuối</li><li><strong>Trưa:</strong> Cơm + thịt bò xào + rau cải</li><li><strong>Chiều:</strong> Hạt hạnh nhân + nho khô</li><li><strong>Tối:</strong> Ức gà nướng + quinoa + bông cải xanh</li></ul><h4>NGÀY 3 & 6: THỨ BA & THỨ SÁU</h4><ul><li><strong>Sáng:</strong> Pancake yến mạch + mật ong + quả berry</li><li><strong>Phụ:</strong> Sữa đậu nành + chuối</li><li><strong>Trưa:</strong> Cơm + cá thu nướng + súp lơ</li><li><strong>Chiều:</strong> Greek yogurt + granola</li><li><strong>Tối:</strong> Thịt heo nạc + khoai tây + salad trộn</li></ul><h4>NGÀY 7: THỨ BẢY (Cheat meal nhẹ)</h4><ul><li><strong>Sáng:</strong> Phở bò + trứng</li><li><strong>Phụ:</strong> Nước ép trái cây tươi</li><li><strong>Trưa:</strong> Pizza + salad (moderate portion)</li><li><strong>Chiều:</strong> Smoothie protein</li><li><strong>Tối:</strong> Cơm + tôm rang + rau củ</li></ul><h3>💊 Thực phẩm bổ sung gợi ý:</h3><ul><li><strong>Whey Protein:</strong> Ngay sau tập (20-30g)</li><li><strong>Creatine:</strong> 5g/ngày</li><li><strong>BCAA:</strong> Trong lúc tập</li><li><strong>Multivitamin:</strong> 1 viên/ngày sau ăn</li></ul><h3>⏰ Thời gian ăn quan trọng:</h3><ul><li><strong>Pre-workout (1-2h trước tập):</strong> Carb + ít protein</li><li><strong>Post-workout (30 phút sau tập):</strong> Protein + carb nhanh</li><li><strong>Trước ngủ:</strong> Casein protein hoặc sữa</li></ul><p><strong>📝 Lưu ý:</strong> Thực đơn này cho người 70kg. Hãy điều chỉnh khẩu phần theo cân nặng và mục tiêu của bạn!</p>', 
 '/images/tin-tuc/thuc-don-dinh-duong.jpg', 
 DATEADD(day, -14, GETDATE()), 
 'PUBLISHED', 
 0, 
 'thuc-don-dinh-duong-mot-tuan-cho-nguoi-tap-gym-tang-co', 
 78, 
 DATEADD(day, -14, GETDATE()), 
 DATEADD(day, -14, GETDATE()), 
 1, 
 N'Thực đơn dinh dưỡng 7 ngày cho người tập gym tăng cơ', 
 N'Thực đơn chi tiết 7 ngày giúp người tập gym tăng cơ hiệu quả. Bao gồm protein, carb, fat cân bằng và thời gian ăn khoa học.', 
 N'thực đơn gym, dinh dưỡng tăng cơ, protein, carb, thực phẩm bổ sung'),

-- Tin tức 7: Sự kiện
(N'Sự kiện "Fitness Challenge 2025" - Thử thách bản thân cùng phòng gym', 
 N'Đăng ký ngay sự kiện Fitness Challenge 2025 với nhiều thử thách thú vị và giải thưởng hấp dẫn. Cơ hội để bạn thử thách bản thân và kết nối với cộng đồng.', 
 N'<h2>🎯 FITNESS CHALLENGE 2025</h2><p>Bạn đã sẵn sàng cho một thử thách đầy thú vị? Hãy tham gia sự kiện lớn nhất năm của chúng tôi!</p><h3>📅 Thông tin sự kiện:</h3><ul><li><strong>Thời gian:</strong> 15/08/2025 - 15/09/2025 (1 tháng)</li><li><strong>Địa điểm:</strong> Tại phòng gym</li><li><strong>Đối tượng:</strong> Tất cả thành viên và khách vãng lai</li><li><strong>Phí tham gia:</strong> Miễn phí cho thành viên, 100.000đ cho khách vãng lai</li></ul><h3>🏆 Các hạng mục thi đấu:</h3><h4>1. STRENGTH CHALLENGE (Thử thách sức mạnh)</h4><ul><li>Bench Press tối đa</li><li>Squat tối đa</li><li>Deadlift tối đa</li><li><strong>Giải thưởng:</strong> 5 triệu đồng + 1 năm tập miễn phí</li></ul><h4>2. ENDURANCE CHALLENGE (Thử thách sức bền)</h4><ul><li>Chạy bộ 5km</li><li>Burpees 100 cái</li><li>Plank tối đa</li><li><strong>Giải thưởng:</strong> 3 triệu đồng + 6 tháng tập miễn phí</li></ul><h4>3. TRANSFORMATION CHALLENGE (Thử thách thay đổi)</h4><ul><li>Giảm cân hiệu quả nhất</li><li>Tăng cơ ấn tượng nhất</li><li>Cải thiện thể lực tổng thể</li><li><strong>Giải thưởng:</strong> 2 triệu đồng + gói PT 3 tháng</li></ul><h4>4. TEAM CHALLENGE (Thử thách nhóm)</h4><ul><li>5 người/team</li><li>Hoàn thành các thử thách theo nhóm</li><li><strong>Giải thưởng:</strong> 5 triệu đồng/team</li></ul><h3>🎁 Phần thưởng đặc biệt:</h3><ul><li><strong>Giải NHẤT tổng sắp:</strong> 10 triệu đồng + 2 năm tập miễn phí</li><li><strong>Giải NHÌ tổng sắp:</strong> 7 triệu đồng + 1 năm tập miễn phí</li><li><strong>Giải BA tổng sắp:</strong> 5 triệu đồng + 6 tháng tập miễn phí</li><li><strong>Giải Khuyến khích:</strong> Áo thun gym + bình nước + gói whey protein</li></ul><h3>📋 Cách thức tham gia:</h3><ol><li>Đăng ký tại quầy lễ tân từ ngày 01/08/2025</li><li>Thực hiện đo đạc thể trạng ban đầu</li><li>Nhận lịch thi đấu chi tiết</li><li>Tham gia các vòng thi theo lịch</li><li>Đo đạc thể trạng cuối kỳ</li></ol><h3>👥 Ban tổ chức:</h3><ul><li><strong>Trưởng ban:</strong> HLV Minh Tuấn (Master Trainer)</li><li><strong>Thành viên:</strong> Đội ngũ HLV chuyên nghiệp</li><li><strong>Trọng tài:</strong> Liên đoàn Thể hình Việt Nam</li></ul><p><strong>📞 Đăng ký ngay:</strong> Liên hệ quầy lễ tân hoặc hotline 1900-XXXX</p><p><em>Số lượng có hạn - Đăng ký ngay để không bỏ lỡ cơ hội!</em></p>', 
 '/images/tin-tuc/fitness-challenge-2025.jpg', 
 DATEADD(day, -5, GETDATE()), 
 'PUBLISHED', 
 1, 
 'su-kien-fitness-challenge-2025-thu-thach-ban-than-cung-phong-gym', 
 203, 
 DATEADD(day, -5, GETDATE()), 
 DATEADD(day, -5, GETDATE()), 
 1, 
 N'Fitness Challenge 2025 - Sự kiện lớn nhất năm với giải thưởng 10 triệu', 
 N'Tham gia Fitness Challenge 2025 với nhiều hạng mục thú vị và giải thưởng lên đến 10 triệu đồng. Đăng ký ngay để thử thách bản thân!', 
 N'fitness challenge, sự kiện gym, thử thách, giải thưởng, thi đấu'),

-- Tin tức 8: Health & Wellness
(N'Tầm quan trọng của giấc ngủ đối với người tập thể hình', 
 N'Giấc ngủ chất lượng là yếu tố quan trọng không kém dinh dưỡng và tập luyện. Hãy cùng tìm hiểu tại sao và làm thế nào để có giấc ngủ tốt nhất.', 
 N'<h2>💤 Giấc ngủ - Yếu tố bị bỏ qua nhất</h2><p>Nhiều người tập gym chỉ tập trung vào tập luyện và dinh dưỡng mà quên mất tầm quan trọng của giấc ngủ. Thực tế, đây là lúc cơ thể phục hồi và phát triển.</p><h3>🔬 Tại sao giấc ngủ quan trọng?</h3><h4>1. Phục hồi cơ bắp</h4><ul><li>80% hormone tăng trưởng được tiết ra trong giai đoạn ngủ sâu</li><li>Protein synthesis diễn ra mạnh nhất khi ngủ</li><li>Cơ bắp được sửa chữa và phát triển</li></ul><h4>2. Cân bằng hormone</h4><ul><li><strong>Testosterone:</strong> Tăng 15% sau 1 đêm ngủ đủ 8 tiếng</li><li><strong>Cortisol:</strong> Giảm xuống mức tối ưu</li><li><strong>Insulin:</strong> Cải thiện độ nhạy cảm</li></ul><h4>3. Hiệu suất tập luyện</h4><ul><li>Tăng sức mạnh 10-15%</li><li>Cải thiện thời gian phản ứng</li><li>Giảm nguy cơ chấn thương</li></ul><h3>⏰ Chu kỳ giấc ngủ lý tưởng:</h3><ul><li><strong>Thời gian:</strong> 7-9 tiếng/đêm</li><li><strong>Đi ngủ:</strong> 22:00 - 23:00</li><li><strong>Thức dậy:</strong> 6:00 - 7:00</li><li><strong>Nap:</strong> 20-30 phút vào buổi chiều (nếu cần)</li></ul><h3>💡 10 mẹo để có giấc ngủ chất lượng:</h3><ol><li><strong>Tạo môi trường ngủ tối ưu:</strong><ul><li>Nhiệt độ 18-22°C</li><li>Phòng tối, yên tĩnh</li><li>Giường thoải mái</li></ul></li><li><strong>Thói quen trước ngủ:</strong><ul><li>Tắt điện thoại 1 tiếng trước ngủ</li><li>Đọc sách hoặc nghe nhạc nhẹ</li><li>Tắm nước ấm</li></ul></li><li><strong>Chế độ ăn uống:</strong><ul><li>Không ăn no 3 tiếng trước ngủ</li><li>Tránh caffeine sau 14:00</li><li>Uống trà chamomile</li></ul></li><li><strong>Tập luyện thông minh:</strong><ul><li>Không tập high-intensity 3 tiếng trước ngủ</li><li>Yoga nhẹ buổi tối</li><li>Stretching thư giãn</li></ul></li></ol><h3>⚠️ Dấu hiệu thiếu ngủ:</h3><ul><li>Giảm sức mạnh trong tập luyện</li><li>Tăng cảm giác đói và thèm ăn</li><li>Khó tập trung</li><li>Tâm trạng không ổn định</li><li>Hệ miễn dịch suy yếu</li></ul><h3>📱 Apps hữu ích:</h3><ul><li><strong>Sleep Cycle:</strong> Theo dõi chất lượng giấc ngủ</li><li><strong>Calm:</strong> Meditation trước ngủ</li><li><strong>f.lux:</strong> Lọc ánh sáng xanh trên máy tính</li></ul><h3>🥛 Thực phẩm hỗ trợ giấc ngủ:</h3><ul><li><strong>Magnesium:</strong> 400mg trước ngủ 30 phút</li><li><strong>Melatonin:</strong> 0.5-3mg (tham khảo bác sĩ)</li><li><strong>Casein protein:</strong> Cung cấp amino acid suốt đêm</li><li><strong>Cherry tart:</strong> Nguồn melatonin tự nhiên</li></ul><p><strong>💪 Kết luận:</strong> Hãy coi giấc ngủ như một phần không thể thiếu trong kế hoạch fitness của bạn. Sleep hard, train hard!</p>', 
 '/images/tin-tuc/tam-quan-trong-giac-ngu.jpg', 
 DATEADD(day, -12, GETDATE()), 
 'PUBLISHED', 
 0, 
 'tam-quan-trong-cua-giac-ngu-doi-voi-nguoi-tap-the-hinh', 
 45, 
 DATEADD(day, -12, GETDATE()), 
 DATEADD(day, -12, GETDATE()), 
 1, 
 N'Tầm quan trọng của giấc ngủ đối với người tập gym', 
 N'Giấc ngủ chất lượng ảnh hưởng 80% đến kết quả tập luyện. Tìm hiểu cách có giấc ngủ tốt để tối ưu hóa quá trình phục hồi và phát triển cơ bắp.', 
 N'giấc ngủ, phục hồi cơ bắp, hormone, chất lượng ngủ, sleep quality');

-- Cập nhật lại một số trường có thể bị thiếu
UPDATE TinTucs SET TacGiaId = 1 WHERE TacGiaId IS NULL;