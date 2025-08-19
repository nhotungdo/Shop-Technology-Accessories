# Shop Technology Accessories - ASP.NET Core Web Application

## Mô tả
Ứng dụng web ASP.NET Core cho cửa hàng phụ kiện công nghệ với đầy đủ các chức năng quản lý sản phẩm, đơn hàng, người dùng và thanh toán.

## Tính năng chính

### 🛍️ Chức năng cho người dùng (Front-end)
- **Duyệt sản phẩm**: Hiển thị danh sách sản phẩm với phân loại, tìm kiếm, lọc theo giá
- **Giỏ hàng**: Thêm/xóa/sửa số lượng sản phẩm, tính tổng giá trị
- **Thanh toán**: Quy trình checkout với các phương thức thanh toán
- **Tài khoản người dùng**: Đăng ký/đăng nhập, quản lý hồ sơ, lịch sử đơn hàng
- **Wishlist**: Danh sách yêu thích sản phẩm
- **Đánh giá và bình luận**: Cho phép người dùng đánh giá sản phẩm

### 🔧 Chức năng quản trị (Admin Panel)
- **Quản lý sản phẩm**: CRUD sản phẩm, quản lý hình ảnh, tồn kho
- **Quản lý đơn hàng**: Xem danh sách, cập nhật trạng thái, xuất báo cáo
- **Quản lý người dùng**: Phân quyền, khóa/mở khóa tài khoản
- **Quản lý danh mục**: CRUD danh mục sản phẩm
- **Dashboard**: Thống kê tổng quan, báo cáo doanh thu

### 🚀 Chức năng kỹ thuật
- **API RESTful**: Đầy đủ API endpoints cho frontend và mobile
- **Xác thực JWT**: Bảo mật API với JWT tokens
- **AutoMapper**: Mapping tự động giữa Models và DTOs
- **Entity Framework Core**: ORM với SQL Server
- **Responsive Design**: Giao diện tương thích mobile

## Cấu trúc dự án

```
ShopTechnology/
├── Areas/
│   └── Admin/                 # Admin Panel
│       ├── Controllers/       # Admin Controllers
│       └── Views/            # Admin Views
├── Controllers/
│   ├── Api/                  # API Controllers
│   ├── AccountController.cs  # Authentication
│   ├── CartController.cs     # Shopping Cart
│   ├── HomeController.cs     # Home Page
│   ├── OrderController.cs    # Orders
│   ├── ProductController.cs  # Products
│   └── WishlistController.cs # Wishlist
├── DTOs/                     # Data Transfer Objects
├── Models/                   # Entity Models
├── Services/                 # Business Logic
├── ViewModels/              # View Models
└── Views/                   # Razor Views
```

## Cài đặt và chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server (LocalDB hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code

### Bước 1: Clone repository
```bash
git clone <repository-url>
cd ShopTechnology
```

### Bước 2: Cài đặt database
1. Mở SQL Server Management Studio
2. Chạy script `db/Demo1_ShopTechnologyAccessories.sql`
3. Cập nhật connection string trong `appsettings.json`

### Bước 3: Restore packages
```bash
dotnet restore
```

### Bước 4: Chạy ứng dụng
```bash
dotnet run
```

Truy cập: `https://localhost:7000` (hoặc port được cấu hình)

## API Endpoints

### Products API
- `GET /api/products` - Lấy danh sách sản phẩm
- `GET /api/products/{id}` - Lấy chi tiết sản phẩm
- `GET /api/products/category/{categoryId}` - Sản phẩm theo danh mục
- `GET /api/products/search?q={term}` - Tìm kiếm sản phẩm
- `GET /api/products/featured` - Sản phẩm nổi bật
- `GET /api/products/newest` - Sản phẩm mới nhất

### Cart API
- `GET /api/cart?userId={id}` - Lấy giỏ hàng
- `POST /api/cart/add` - Thêm vào giỏ hàng
- `PUT /api/cart/update/{cartItemId}` - Cập nhật số lượng
- `DELETE /api/cart/remove/{cartItemId}` - Xóa khỏi giỏ hàng
- `DELETE /api/cart/clear?userId={id}` - Xóa toàn bộ giỏ hàng

### Orders API
- `GET /api/orders` - Lấy danh sách đơn hàng
- `GET /api/orders/{id}` - Lấy chi tiết đơn hàng
- `POST /api/orders` - Tạo đơn hàng mới
- `PUT /api/orders/{id}/status` - Cập nhật trạng thái

### Users API
- `GET /api/users` - Lấy danh sách người dùng
- `GET /api/users/{id}` - Lấy thông tin người dùng
- `POST /api/users` - Tạo người dùng mới
- `PUT /api/users/{id}` - Cập nhật thông tin người dùng

## Database Schema

### Bảng chính
- **Users**: Thông tin người dùng
- **Roles**: Vai trò (Admin, User)
- **Categories**: Danh mục sản phẩm
- **Products**: Sản phẩm
- **ProductImages**: Hình ảnh sản phẩm
- **Carts**: Giỏ hàng
- **CartItems**: Chi tiết giỏ hàng
- **Orders**: Đơn hàng
- **OrderDetails**: Chi tiết đơn hàng
- **Payments**: Thanh toán
- **Wishlists**: Danh sách yêu thích

## Tính năng nâng cao

### 🔐 Bảo mật
- Mã hóa mật khẩu với BCrypt
- JWT Authentication cho API
- Role-based Authorization
- CSRF Protection

### 📊 Báo cáo và thống kê
- Dashboard với thống kê real-time
- Báo cáo doanh thu theo thời gian
- Top sản phẩm bán chạy
- Sản phẩm tồn kho thấp

### 🛒 Quản lý đơn hàng
- Trạng thái đơn hàng: Pending, Paid, Shipped, Completed, Canceled
- Xuất báo cáo CSV
- Lọc và tìm kiếm đơn hàng
- Cập nhật trạng thái hàng loạt

### 🎨 Giao diện
- Responsive design với Bootstrap
- Modern UI/UX
- Loading states và error handling
- Toast notifications

## Đóng góp

1. Fork project
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Liên hệ

- Email: donhotung2004@gmail.com
- Project Link: [https://github.com/yourusername/ShopTechnology](https://github.com/yourusername/ShopTechnology)

## Roadmap

### Phiên bản 1.1
- [ ] Tích hợp thanh toán online (VNPay, PayPal)
- [ ] Gửi email xác nhận đơn hàng
- [ ] Quản lý kho hàng nâng cao
- [ ] Báo cáo chi tiết hơn

### Phiên bản 1.2
- [ ] Mobile app với React Native
- [ ] Chatbot hỗ trợ khách hàng
- [ ] Hệ thống đánh giá và review
- [ ] Tích hợp social login

### Phiên bản 2.0
- [ ] Microservices architecture
- [ ] Real-time notifications
- [ ] AI-powered product recommendations
- [ ] Multi-language support
