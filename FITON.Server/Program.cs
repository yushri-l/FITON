using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FITON.Server.Utils.Database;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secret = builder.Configuration.GetValue<string>("Secret");
    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
             .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.MapStaticAssets();

// CORS must come before Authentication and Authorization
app.UseCors("CorsPolicy");

//app.UseHttpsRedirection();  // Remove this line for Azure Container Apps

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

// ✅ IMPROVED DATABASE INITIALIZATION
// ✅ IMPROVED DATABASE INITIALIZATION
Console.WriteLine("🚀 Starting database initialization...");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("📊 Checking database connection...");
        var canConnect = dbContext.Database.CanConnect();
        Console.WriteLine($"✅ Database connection: {canConnect}");

        if (canConnect)
        {
            Console.WriteLine("🛠️ Creating database tables...");

            // Try multiple approaches
            dbContext.Database.EnsureCreated();
            Console.WriteLine("✅ EnsureCreated completed");

            // Additional check
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
            Console.WriteLine($"📋 Pending migrations: {pendingMigrations.Count}");

            if (pendingMigrations.Any())
            {
                Console.WriteLine("🔄 Applying migrations...");
                dbContext.Database.Migrate();
                Console.WriteLine("✅ Migrations applied");
            }

            // Final verification
            try
            {
                var userCount = dbContext.Users.Count();
                Console.WriteLine($"✅ Users table exists with {userCount} records");
            }
            catch
            {
                Console.WriteLine("❌ Users table still doesn't exist after EnsureCreated");
            }
        }
        else
        {
            Console.WriteLine("❌ Cannot connect to database - check connection string");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"💥 DATABASE INITIALIZATION FAILED: {ex.Message}");
        Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");

        // Log inner exception if exists
        if (ex.InnerException != null)
        {
            Console.WriteLine($"🔍 Inner exception: {ex.InnerException.Message}");
        }
    }
}

Console.WriteLine("🏁 Database initialization complete");


app.Run();
