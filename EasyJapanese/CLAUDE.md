# EasyJapanese — AI System Prompt

> Nền tảng học tiếng Nhật trực tuyến cho thị trường Việt Nam, brand **"Hi Japan!"**. ASP.NET Core 8 MVC, tiếng Việt first, scaffolded từ SQL Server.

## Repository layout

```
EasyJapanese/
├── CoreLibrary/                  # Class library — domain + integrations (KHÔNG chứa appsettings)
│   ├── Const/                    # RoleConst, MessageConst, StorageConst
│   ├── Data/                     # AppDbContext (partial) + AppDbContextExtensions + Entities/
│   ├── Payment/                  # IPaymentService + PayOS wrapper + AddPayOS extension
│   ├── Storage/                  # IStorageService + R2 (S3) + AddCloudflareR2 extension
│   └── Utility/                  # PasswordUtil (BCrypt), SessionUtil, NumberUtil
│
├── WebApplication1/              # ASP.NET Core 8 MVC host — KHÔNG đặt entity/DbContext ở đây
│   ├── Areas/
│   │   ├── Admin/                # /admin/...   (no footer layout)
│   │   └── Learner/              # /learn/...
│   ├── Controllers/              # Home, Login (public, không Area)
│   ├── Views/                    # Home, Login, Shared/_Layout, Shared/_Header
│   ├── wwwroot/
│   │   ├── css/common.css        # Design tokens (:root) + .hj-* components
│   │   ├── css/site.css          # Minimal layout baseline
│   │   ├── lib/                  # Bootstrap 5, jQuery 3 (vendored)
│   │   └── js/{site,core}.js     # Hiện trống — đặt JS mới ở đây
│   ├── Program.cs                # Composition root duy nhất
│   └── appsettings.json          # ConnectionStrings + PayOS + R2
│
├── EasyJapanese.slnx
└── CLAUDE.md                     # File này
```

## Tech stack

- **.NET 8**, EF Core 8.0.10 (SQL Server), Bootstrap 5 + jQuery 3, Inter font (Google Fonts 400-800)
- `BCrypt.Net-Next` 4.2.0 (password)
- `PayOS` 2.1.0 (payment gateway VN)
- `AWSSDK.S3` 3.7.0 (Cloudflare R2 — S3-compatible)
- **Host target**: Kestrel / IIS / Azure App Service / Docker. Frontend demo `hijapan.vercel.app` chỉ là tham khảo, **không phải** deploy target.
- **Database**: local SQL Server `DESKTOP-BF81UL9\...` — connection string đọc từ `appsettings.json:ConnectionStrings:DefaultConnection`. Không commit credential thật lên remote.

## Code conventions

### DI pattern (BẮT BUỘC cho mọi integration mới)

Mỗi external integration đặt trong `CoreLibrary/<Domain>/` theo cấu trúc 4 file:

```
CoreLibrary/<Domain>/
├── IFooService.cs
├── FooService.cs
├── FooOptions.cs
└── FooServiceCollectionExtensions.cs   # chứa public static IServiceCollection AddFoo(...)
```

- Register trong `Program.cs` qua extension: `builder.Services.Add<Name>(builder.Configuration);`
- **Controller không được `new` trực tiếp client ngoài** — inject qua constructor.
- Options bind từ `appsettings.json` section cùng tên (e.g. `PayOS`, `R2`).
- Mặc định `Singleton` cho stateless wrapper, `Scoped` cho EF DbContext.

Ví dụ đang có: `Payment/` (PayOS), `Storage/` (R2), `Data/AppDbContextExtensions.cs` (DbContext).

### Data layer

- **Entity classes** scaffold từ DB vào `CoreLibrary/Data/Entities/` — **không sửa tay**.
- **`AppDbContext.cs` scaffold nguyên khối, không edit** (gồm 24 DbSet + `OnModelCreating`).
- Custom DbContext config → thêm `partial class` (xem `AppDbContext.Configuration.cs` cho pattern) hoặc implement `OnModelCreatingPartial(modelBuilder)` hook.
- **Connection string**: pattern `Name=DefaultConnection` trong `OnConfiguring` fallback — WebApplication1 inject qua DI, không hardcode.

### Roles & messages

- Roles: `"Admin" | "Mentor" | "Student"` — dùng constants từ `CoreLibrary/Const/RoleConst.cs`, không viết string literal.
- Messages VN: `CoreLibrary/Const/MessageConst.cs`.

### Constants vs config — quy tắc phân biệt

- **Constants** (đặt trong `CoreLibrary/Const/<Domain>Const.cs`) là **quy ước nghiệp vụ cố định** mà code nào cũng phải dùng đúng giá trị giống nhau. Ví dụ: role name (`RoleConst`), folder key trong storage bucket (`StorageConst.FOLDER_AVATARS = "avatars/"`).
  - Tên constant dạng `SCREAMING_SNAKE_CASE` (`ADMIN`, `FOLDER_IMAGES`).
  - **Không** đưa vào `appsettings.json` vì đổi sang môi trường khác không được phép sai (upload vào folder A, build URL trỏ folder B → 404 silent).
- **Config** (`appsettings.json`) là thứ **thay đổi theo môi trường**: connection string, secret key, `AccountId`/`BucketName`/`PublicBaseUrl` của R2, PayOS credentials.
- Khi thêm domain mới có quy ước cố định (vd: enum trạng thái đơn hàng, MIME type whitelist) → tạo file `<Domain>Const.cs` mới, đừng nhét vào file Const có sẵn trừ khi cùng domain.

### Areas

- Thêm controller mới → `Areas/<Name>/Controllers/`.
- Layout tương ứng → `Areas/<Name>/Views/Shared/_<Name>Layout.cshtml`.
- URL prefix đăng ký qua extension method trong `Areas/<Name>/<Name>AreaRegistration.cs` (gọi từ `Program.cs`).
- Mỗi Area có `_ViewImports.cshtml` riêng.

## Frontend conventions

### CSS tokens

- Tất cả màu/typography/shadow → `wwwroot/css/common.css` `:root` variables (`--color-primary`, `--color-bg`, `--color-text`, `--color-border`, `--shadow-*`).
- **Không hardcode màu** trong view hoặc file CSS khác — luôn dùng `var(--color-...)`.

### Class naming

- Public site (`_Layout`, `Index`): prefix `hj-*` (BEM-ish, modifier bằng `--`). Ví dụ: `hj-header`, `hj-navbar`, `hj-hero-title`, `hj-price-card--featured`, `hj-btn-login`.
- Login page: class riêng (`login-wrapper`, `brand-logo`, `form-control-custom`, `btn-login`).
- **Learner/Admin layouts**: giữ Bootstrap thuần (chưa migrate sang `hj-*`).
- Ảnh hiện dùng URL remote (Unsplash/pravatar) cho demo — sẽ chuyển sang local sau.

### Icons

- **Mặc định KHÔNG dùng icon** (Bootstrap Icons, emoji, SVG,...) trong view, CSS, JS — chỉ dùng text/typography thuần.
- **Chỉ thêm icon khi user yêu cầu rõ ràng**, hoặc cho những chỗ thực sự cần thiết mà text không thay thế được:
  - Action universal đã thành chuẩn UI (close `×`, search `🔍`, hamburger menu `☰`, checkbox/radio trạng thái checked, loading spinner).
  - Icon nhãn hệ thống đã được user chốt trong design cố định (vd: logo brand ở header).
- **Không** tự thêm icon "trang trí" kiểu `📋 ⏱ 🎯 🗺️` cạnh text — nếu cần visual hierarchy thì dùng badge, background, color, font-weight.
- Khi nghi ngờ → hỏi user trước khi thêm.

### Layouts

| Layout | Route | Đặc điểm |
|---|---|---|
| `_Layout.cshtml` | Public site "Hi Japan!" | Sticky header + dark footer + Inter font |
| `_LearnerLayout.cshtml` | `/learn/...` | Bootstrap navbar `bg-white`, có footer |
| `_AdminLayout.cshtml` | `/admin/...` | Bootstrap navbar `bg-dark`, **không footer** |
| Login page | `Layout = null` | Self-rendered full HTML |

## i18n

- UI copy, docstring, comment trong Razor view, log message = **tiếng Việt**.
- Code identifier (class, method, biến, table name) = **tiếng Anh**.
- `<html lang="vi">`, DB collation `Vietnamese_CI_AS`.

## Common commands

```bash
# Restore
dotnet restore

# Build (phải pass trước khi coi như xong task)
dotnet build WebApplication1/CoreWeb.csproj

# Re-scaffold entities (khi schema DB đổi)
dotnet-ef dbcontext scaffold "<conn-string>" Microsoft.EntityFrameworkCore.SqlServer \
  --project CoreLibrary/CoreLibrary.csproj \
  --output-dir Data/Entities \
  --context-dir Data \
  --context AppDbContext --force
```

`dotnet-ef` đã cài global (`dotnet tool install --global dotnet-ef --version 8.0.10`).

## AI workflow rules

### Git safety
- **Không tự `git commit`, `git push`, force-push, hay sửa `git config`.**
- Hỏi user trước khi thực hiện hành động không reversible (xóa file, reset, push).
- Có thể tự do tạo/sửa file local và chạy build/test.

### No over-engineering
- Không thêm abstraction, helper, validation cho case không tồn tại.
- Không tạo file `.md`/`README` mới khi user không yêu cầu.
- Không refactor ngoài scope task hiện tại.
- Không thêm comment giải thích `WHAT` code làm — code tự nói.

### Vietnamese-first UI
- Hiển thị, comment trong Razor view, log message = tiếng Việt.
- Code identifier = tiếng Anh.

### Service-extension pattern
- Mọi integration mới (Redis, SendGrid, Email,...) → theo đúng 4-file pattern ở mục Code conventions.

### Comments
- Chỉ thêm comment khi `WHY` không hiển nhiên: constraint ẩn, workaround bug cụ thể, behavior gây bất ngờ.
- Không viết docstring đa dòng. Một dòng ngắn tối đa.

### When unsure
- Tham khảo file hiện có làm theo cùng pattern (xem `Payment/` và `Storage/` làm tham chiếu).
- Hỏi user khi gặp quyết định kiến trúc không có trong CLAUDE.md.
