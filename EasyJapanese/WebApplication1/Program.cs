using CoreLibrary.Authentication;
using CoreLibrary.Data;
using CoreLibrary.Email;
using CoreLibrary.Payment;
using CoreLibrary.Storage;
using WebApplication1.Areas.Admin;
using WebApplication1.Areas.Learner;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // HttpContextAccessor (dùng cho partial view _Header đọc session)
            builder.Services.AddHttpContextAccessor();

            // Session - lưu trạng thái auth, pending registration
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".HiJapan.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromHours(2);
            });

            // Đăng ký DbContext - đọc connection string "DefaultConnection" từ appsettings.json
            builder.Services.AddAppDbContext(builder.Configuration);

            // Đăng ký Email (Gmail SMTP)
            builder.Services.AddGmailEmail(builder.Configuration);

            // Đăng ký Authentication service
            builder.Services.AddAuthenticationService(builder.Configuration);

            // Đăng ký PayOS service (đọc config từ section "PayOS" trong appsettings.json)
            builder.Services.AddPayOS(builder.Configuration);

            // Đăng ký Cloudflare R2 storage service (đọc config từ section "R2" trong appsettings.json)
            builder.Services.AddCloudflareR2(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            // Route mặc định cho website công khai (Controllers/, không có Area)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Khu vực học viên: /learn/...
            app.MapLearnerArea();

            // Khu vực quản trị: /admin/...
            app.MapAdminArea();

            app.Run();
        }
    }
}
