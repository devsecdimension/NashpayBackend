using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NashPay.API.Data;
using NashPay.API.Models;
using NashPay.API.Services;
using NashPay.API.Helpers;
using NashPay.API.Middlewares;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection String (MS SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Service Setup
builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 3. JWT Authentication Setup
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "Your_Very_Secret_Key_12345");

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 4. CORS Policy (Angular + WordPress Integration)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngularApp",
        policy => policy.WithOrigins(
                    "http://localhost:4200", 
                    "http://localhost:4201",
                    "http://localhost:3000",  // WordPress local
                    "http://localhost:3001",  // Alt localhost
                    "http://127.0.0.1:3000",
                    "http://nashpay.local",   // WordPress local domain
                    "http://*.local",         // Any local WordPress site
                    "https://*")              // Production domains
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
});

// 5. Register Custom Services
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<EncryptionHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IKYCService, KYCService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBankDetailsService, BankDetailsService>();
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();

// HttpClient for webhook delivery
builder.Services.AddHttpClient<IWebhookService, WebhookService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// 6. Add Controllers and API Documentation with Swagger/OpenAPI
builder.Services.AddControllers();
// Note: Skipping AddEndpointsApiExplorer to avoid OpenAPI source generator version incompatibility
builder.Services.AddSwaggerGen();

// 7. Add Logging
builder.Services.AddLogging(config => {
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// 8. Middleware Pipeline Configuration
if (app.Environment.IsDevelopment()) {
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "api-docs/{documentName}/swagger.json";
    });
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/api-docs/v1/swagger.json", "NashPay API v1.0");
        options.RoutePrefix = string.Empty; // Swagger UI at root
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
        
        // Custom HTML to improve UI
        options.InjectStylesheet("https://cdn.jsdelivr.net/npm/swagger-ui-dist@3/swagger-ui.css");
        options.InjectJavascript("https://cdn.jsdelivr.net/npm/swagger-ui-dist@3/swagger-ui-bundle.js");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp"); // Angular connection enabled

// Custom Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();        // Pehlay Login check
app.UseAuthorization();         // Phir Permissions check

app.MapControllers();

// 9. Create Default Admin Role and User (on startup)
using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var dbContext = services.GetRequiredService<AppDbContext>();

    try
    {
        // Create roles
        string[] roles = { "Admin", "Merchant", "Customer" };
        foreach (var role in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create default admin user
        var adminEmail = "admin@nashpay.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new User
            {
                Email = adminEmail,
                UserName = adminEmail,
                FullName = "NashPay Admin",
                Role = "Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsVerified = true,
                IsActive = true,
                KYCStatus = "Approved"
            };

            var result = await userManager.CreateAsync(admin, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");

                // Create wallet for admin
                var adminWallet = new Wallet
                {
                    UserId = admin.Id,
                    Balance = 0,
                    LockedBalance = 0,
                    Currency = "PKR",
                    Status = "Active"
                };
                dbContext.Wallets.Add(adminWallet);
                await dbContext.SaveChangesAsync();
            }
        }

        // Create default merchant user
        var merchantEmail = "merchant@nashpay.com";
        var merchant = await userManager.FindByEmailAsync(merchantEmail);
        if (merchant == null)
        {
            merchant = new User
            {
                Email = merchantEmail,
                UserName = merchantEmail,
                FullName = "Test Merchant",
                Role = "Merchant",
                BusinessName = "Test Business",
                BusinessType = "Retail",
                RegistrationNumber = "REG123456",
                TaxId = "TAX123456",
                EmailConfirmed = true,
                PhoneNumber = "+92-300-1234567",
                IsVerified = true,
                IsActive = true,
                KYCStatus = "Approved"
            };

            var result = await userManager.CreateAsync(merchant, "Merchant@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(merchant, "Merchant");

                // Create wallet for merchant
                var merchantWallet = new Wallet
                {
                    UserId = merchant.Id,
                    Balance = 500000,
                    LockedBalance = 50000,
                    Currency = "PKR",
                    Status = "Active",
                    TotalReceived = 850000,
                    TotalWithdrawn = 350000,
                    PendingAmount = 35000
                };
                dbContext.Wallets.Add(merchantWallet);
                await dbContext.SaveChangesAsync();
            }
        }

        // Create default customer user
        var customerEmail = "customer@nashpay.com";
        var customer = await userManager.FindByEmailAsync(customerEmail);
        if (customer == null)
        {
            customer = new User
            {
                Email = customerEmail,
                UserName = customerEmail,
                FullName = "Test Customer",
                Role = "Customer",
                PhoneNumber = "+92-300-9876543",
                EmailConfirmed = true,
                IsVerified = true,
                IsActive = true,
                KYCStatus = "Approved"
            };

            var result = await userManager.CreateAsync(customer, "Customer@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer, "Customer");

                // Create wallet for customer
                var customerWallet = new Wallet
                {
                    UserId = customer.Id,
                    Balance = 25000,
                    LockedBalance = 0,
                    Currency = "PKR",
                    Status = "Active",
                    TotalReceived = 50000,
                    TotalWithdrawn = 25000,
                    PendingAmount = 0
                };
                dbContext.Wallets.Add(customerWallet);
                await dbContext.SaveChangesAsync();
            }
        }
    }
    catch (Exception ex)
    {
        // Log error but don't fail startup
        Console.WriteLine($"Error creating default users: {ex.Message}");
    }
}

app.Run();