# EasyJapanese (Hi Japan!)

> Nền tảng học tiếng Nhật trực tuyến cho thị trường Việt Nam — ASP.NET Core 8 MVC.

Landing page tham khảo: [hijapan.vercel.app](https://hijapan.vercel.app/)

---

## Tech stack

- **.NET 8** — ASP.NET Core MVC + Razor
- **Entity Framework Core 8** — SQL Server
- **Bootstrap 5** + jQuery 3 (client-side validation)
- **Inter** font (Google Fonts)
- **BCrypt.Net-Next** — password hashing
- **PayOS** — payment gateway (VN)
- **AWSSDK.S3** — Cloudflare R2 storage (S3-compatible)

## Repository structure

```
EasyJapanese/
├── CoreLibrary/                  Class library — domain + integrations
│   ├── Const/                    RoleConst, MessageConst
│   ├── Data/                     AppDbContext + Entities (EF Core)
│   ├── Payment/                  PayOS wrapper
│   ├── Storage/                  Cloudflare R2 wrapper
│   └── Utility/                  Password, Session, Number helpers
│
├── WebApplication1/              ASP.NET Core 8 MVC host
│   ├── Areas/{Admin,Learner}/    Khu vực quản trị & học viên
│   ├── Controllers/              Home, Login, Register (public)
│   ├── Views/                    Razor views + layouts
│   ├── wwwroot/                  CSS, JS, vendor libs
│   ├── Program.cs                Composition root
│   └── appsettings.json          ConnectionStrings, PayOS, R2
│
└── CLAUDE.md                     AI system prompt (conventions cho các session AI sau)
```

## Yêu cầu môi trường

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019+ (hoặc LocalDB)
- Tài khoản [Cloudflare R2](https://cloudflare.com/) (cho storage — tùy chọn khi dev)
- Tài khoản [PayOS](https://payos.vn/) (cho thanh toán — tùy chọn khi dev)

## Cài đặt & chạy

### 1. Clone & restore

```bash
git clone <repo-url>
cd EasyJapanese
dotnet restore
```

### 2. Cấu hình

Mở `WebApplication1/appsettings.json` và cập nhật:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EasyJapaneseDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "PayOS": {
    "ClientId": "...",
    "ApiKey": "...",
    "ChecksumKey": "..."
  },
  "R2": {
    "AccountId": "...",
    "AccessKey": "...",
    "SecretKey": "...",
    "BucketName": "easyjapanese-assets",
    "PublicBaseUrl": "https://pub-xxx.r2.dev"
  }
}
```

> ⚠️ **Không commit `appsettings.json` chứa credential thật.** Dùng `appsettings.Development.json` (gitignored) hoặc biến môi trường cho production.

### 3. Cài EF Core tool (chỉ cần lần đầu hoặc khi re-scaffold)

```bash
dotnet tool install --global dotnet-ef --version 8.0.10
```

### 4. Re-scaffold entities từ DB (nếu schema đã có sẵn)

```bash
dotnet-ef dbcontext scaffold "<conn-string>" Microsoft.EntityFrameworkCore.SqlServer \
  --project CoreLibrary/CoreLibrary.csproj \
  --output-dir Data/Entities \
  --context-dir Data \
  --context AppDbContext --force
```

### 5. Build & run

```bash
dotnet build WebApplication1/CoreWeb.csproj
dotnet run --project WebApplication1/CoreWeb.csproj
```

App mặc định chạy ở `https://localhost:5001`.

## Routes chính

| URL | Mô tả |
|---|---|
| `/` | Landing page công khai |
| `/Home/Privacy` | Trang privacy |
| `/Login` | Đăng nhập |
| `/Register` | Đăng ký |
| `/learn/...` | Khu vực học viên (yêu cầu auth) |
| `/admin/...` | Khu vực quản trị (yêu cầu role Admin) |

## Architecture notes

- **Service-extension pattern**: mỗi integration (PayOS, R2, DbContext) đăng ký qua extension `Add<Name>()` trong `CoreLibrary/<Domain>/`. Controller inject interface qua constructor, không `new` trực tiếp client ngoài.
- **Entity tự động scaffold** từ DB bằng `dotnet-ef`. Custom DbContext config bổ sung qua `partial class`.
- **Frontend** dùng design tokens ở `wwwroot/css/common.css` (CSS variables), component class prefix `hj-*` cho public site, Bootstrap thuần cho khu vực Learner/Admin.
- **i18n**: UI tiếng Việt, code identifier tiếng Anh, DB collation `Vietnamese_CI_AS`.

## Contributing

1. Tạo branch từ `main`: `git checkout -b feature/<ten-tinh-nang>`
2. Commit với message rõ ràng (ưu tiên tiếng Việt cho message commit)
3. Push branch & tạo Pull Request
4. Đảm bảo `dotnet build` pass trước khi review

## License

Private / Internal — chưa public license.
