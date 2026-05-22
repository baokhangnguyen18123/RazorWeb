using System.Configuration;
using App.Services;
using CS58.Models;
using CS58.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<MyBlogContext>(option =>
{
    // builder.Configuration.GetConnectionString("MyBlogContext") get connection string in appSetting.json
    option.UseSqlServer(builder.Configuration.GetConnectionString("MyBlogContext"));
});
builder.Services.AddIdentity<AppUser, IdentityRole>()
.AddEntityFrameworkStores<MyBlogContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();
// builder.Services.AddDefaultIdentity<AppUser>()
// .AddEntityFrameworkStores<MyBlogContext>()
// .AddDefaultTokenProviders();

//
builder.Services.AddAuthentication()
.AddGoogle(options =>
{
    var googleConfig = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleConfig["ClientId"];
    options.ClientSecret = googleConfig["ClientSecret"];
    options.CallbackPath = "/login-google";
})
// .AddTwitter()
.AddFacebook(options =>
{
    // Lấy cấu hình từ appsettings.json
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    options.CallbackPath = "/login-facebook";
})
// .AddMicrosoftAccount()
;

builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions> (options => {
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;

});

// Đọc cấu hình từ appsettings.json và gán vào MailSettings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Đăng ký SendMailService vào hệ thống với interface IEmailSender
builder.Services.AddTransient<IEmailSender, SendMailService>();

builder.Services.ConfigureApplicationCookie(options =>
{
   options.LoginPath ="/Login/";
   options.LogoutPath="/Logout/";
   options.AccessDeniedPath = "/khongduoctruycap.html";


});

builder.Services.AddAuthorization(options =>
{
   options.AddPolicy("AllowEditRole", policyBuilder =>
   {    
        // Policy based  on role authorization 
        policyBuilder.RequireAuthenticatedUser();
        // policyBuilder.RequireRole("Admin");
        // policyBuilder.RequireRole("Editor");
        // Policy based on claim authorization
        policyBuilder.RequireClaim("manage_role", "add", "update", "True");
        

   });
   
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

IdentityUser user;
IdentityDbContext context;


app.Run();

/*
    dotnet new page --name EditUserRoleClaim --output Areas/Admin/Pages/User --namespace CS58.Areas.Admin.Pages.User
    dotnet aspnet-codegenerator razorpage -m CS58.Models.Article -dc CS58.Models.MyBlogContext -outDir Pages/Blog -udl --referenceScriptLibraries
*/