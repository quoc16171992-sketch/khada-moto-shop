# KhadaShop

KhadaShop là website bán mũ bảo hiểm fullface phong cách racing, xây dựng trực tiếp trên project ASP.NET Core Razor Pages `Demo1`. Website hỗ trợ duyệt sản phẩm, chọn biến thể size/màu, giỏ hàng bằng Session, đặt hàng thanh toán khi nhận (COD), tra cứu đơn và khu vực quản trị.

> Dữ liệu và hình ảnh mẫu đều do project tự tạo. Không sử dụng tên, logo hoặc nội dung của nhãn hiệu bên thứ ba.

## Công nghệ

- .NET 10.0, ASP.NET Core Razor Pages
- Entity Framework Core 10.0.2, SQLite
- ASP.NET Core Identity và role `Admin`
- Bootstrap 5, CSS và JavaScript thuần
- Session lưu giỏ hàng; xUnit cho kiểm thử

## Cấu trúc chính

- `Demo1/Data`: DbContext, migration và seed dữ liệu
- `Demo1/Models`: banner, cài đặt, danh mục, sản phẩm, biến thể, đơn hàng
- `Demo1/ViewModels`: input model và dữ liệu hiển thị
- `Demo1/Services`: giỏ hàng, đặt hàng, banner, ảnh và cài đặt cửa hàng
- `Demo1/Pages`: các trang Razor công khai và quản trị
- `Demo1/wwwroot/uploads`: ảnh banner/sản phẩm do Admin tải lên
- `Demo1.Tests`: 10 kiểm thử nghiệp vụ chính

## Cài đặt và chạy

Mở PowerShell tại thư mục chứa `Demo1.slnx`, sau đó chạy:

```powershell
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\Demo1\Demo1.csproj --startup-project .\Demo1\Demo1.csproj
dotnet run --project .\Demo1\Demo1.csproj
```

Database SQLite mặc định là `Demo1/khadashop.db` với connection string `Data Source=khadashop.db`. Lần chạy đầu ứng dụng sẽ seed 3 danh mục, 3 banner, 6 sản phẩm, các biến thể và cài đặt mặc định.

## Cấu hình tài khoản Admin bằng User Secrets

Không đặt mật khẩu Admin trong `appsettings.json` hoặc source code. Tại thư mục solution, chạy:

```powershell
dotnet user-secrets init --project .\Demo1\Demo1.csproj
dotnet user-secrets set "AdminSeed:Email" "admin@khadashop.local" --project .\Demo1\Demo1.csproj
dotnet user-secrets set "AdminSeed:Password" "MatKhauManhThayDoiNgay!123" --project .\Demo1\Demo1.csproj
```

Giá trị mật khẩu trên chỉ là ví dụ theo yêu cầu MVP; hãy đổi thành mật khẩu riêng đủ mạnh trước khi chạy. Khởi động lại ứng dụng để tạo/gán role `Admin`, rồi mở `/Admin/Login`.

## Migration

Local tool manifest đã chứa `dotnet-ef` 10.0.2. Khi thay đổi model:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add TenMigration --project .\Demo1\Demo1.csproj --startup-project .\Demo1\Demo1.csproj --output-dir Data\Migrations
dotnet tool run dotnet-ef database update --project .\Demo1\Demo1.csproj --startup-project .\Demo1\Demo1.csproj
```

Project đã có migration `InitialCreate`; không dùng `EnsureCreated` trong ứng dụng chính.

## Sử dụng trang quản trị

- Banner: `/Admin/Banners` — thêm, sửa, bật/tắt, sắp thứ tự, đặt thời gian và thay ảnh. Ảnh hợp lệ: JPG/JPEG/PNG/WEBP, tối đa 8 MB.
- Sản phẩm: `/Admin/Products` — thêm, sửa, tìm/lọc, bật/tắt, đánh dấu nổi bật và thay ảnh. Ảnh tối đa 5 MB.
- Biến thể: chọn **Quản lý** tại danh sách sản phẩm để cập nhật size, màu, SKU, tồn kho và giá cộng thêm.
- Danh mục: `/Admin/Categories` — không thể xóa danh mục đang có sản phẩm.
- Đơn hàng: `/Admin/Orders` — tìm theo mã/số điện thoại, lọc trạng thái và cập nhật. Hủy đơn sẽ hoàn kho đúng một lần.
- Thông tin cửa hàng: `/Admin/Settings` — đổi tên shop, điện thoại, Facebook, email, địa chỉ, giờ hỗ trợ và nội dung footer mà không sửa code.

Số điện thoại, Facebook và địa chỉ trên navbar/footer/trang liên hệ/nút nổi đều đọc từ bảng `SiteSettings`. Link Facebook chỉ chấp nhận `http` hoặc `https`.

## Phí giao hàng

Logic nằm trong `Services/OrderService.cs`:

- Tạm tính dưới 1.500.000 VNĐ: phí 30.000 VNĐ
- Tạm tính từ 1.500.000 VNĐ: miễn phí

Muốn đổi chính sách, cập nhật `FreeShippingThreshold` và `StandardShippingFee`, sau đó chạy lại test.

## Build và test

```powershell
dotnet build .\Demo1.slnx
dotnet test .\Demo1.slnx
```

Các test kiểm tra định dạng mã đơn, phí giao hàng, thành tiền, quantity, checkout vượt tồn kho, hoàn kho một lần, URL Facebook nguy hiểm và thời gian hiệu lực banner.

## URL chính

- `/` — trang chủ
- `/san-pham` — danh sách và bộ lọc sản phẩm
- `/san-pham/{slug}` — chi tiết sản phẩm
- `/gio-hang` — giỏ hàng
- `/dat-hang` — checkout COD
- `/tra-cuu-don-hang` — tra cứu đơn bằng mã đơn + số điện thoại
- `/lien-he` — liên hệ
- `/Admin/Login` — đăng nhập quản trị
- `/Admin` — dashboard quản trị

## Giới hạn MVP

- Chỉ hỗ trợ thanh toán khi nhận hàng; chưa có thanh toán trực tuyến.
- Chưa tích hợp đơn vị vận chuyển, email/SMS tự động hoặc mã giảm giá.
- Mỗi sản phẩm có một ảnh chính; chưa có thư viện nhiều ảnh.
- SQLite phù hợp MVP và cửa hàng nhỏ; production tải lớn nên cân nhắc SQL Server/PostgreSQL.
- Tồn kho được kiểm tra và trừ trong transaction nhưng chưa có cơ chế đặt giữ hàng dài hạn.

## Checklist deploy production

- Dùng connection string production qua biến môi trường hoặc secret store; không commit database/secret.
- Tạo tài khoản Admin bằng mật khẩu riêng, xoay vòng mật khẩu ví dụ.
- Chạy migration trước khi nhận traffic và sao lưu database.
- Bật HTTPS/HSTS, reverse proxy tin cậy và cấu hình cookie an toàn.
- Cấp quyền ghi tối thiểu cho `wwwroot/uploads`; giới hạn kích thước request ở proxy.
- Thiết lập log, giám sát, backup ảnh và database; không log mật khẩu hay dữ liệu nhạy cảm.
- Rà soát nhãn hiệu/hình ảnh và chỉ đưa nội dung đã có quyền sử dụng lên website.
- Chạy `dotnet restore`, `dotnet build`, `dotnet test` và smoke test toàn bộ luồng trước deploy.
