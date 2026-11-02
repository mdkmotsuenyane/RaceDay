using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RaceDay.Data;
using RaceDay.Api.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq; // Required for .Any()

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Register the BlobService (Handles Azure and Local Fallback)
builder.Services.AddScoped<RaceDay.Api.Services.BlobService>();

// 4. JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key missing from config");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// 5. Swagger Configuration with JWT Bearer Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT (no 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });
});

var app = builder.Build();

// 6. AUTOMATIC DATABASE MIGRATIONS & SEEDING (CRUCIAL FOR DOCKER)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    db.Database.Migrate();

    // SEED DATA IF EMPTY (For Docker environment)
    if (!db.User.Any())
    {
        // 1. Create an Organiser User
        var user = new RaceDay.Models.User
        {
            Email = "organiser@indela.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = "Organiser",
            FullName = "iNDLELA Admin",
            CreatedAt = DateTime.UtcNow
        };
        db.User.Add(user);
        db.SaveChanges();

        // 2. Create the Organiser Profile
        var org = new RaceDay.Models.Organiser { UserID = user.UserId, Name = "iNDLELA Events", Address = "Cape Town" };
        db.Organisers.Add(org);
        db.SaveChanges();

        // 3. Create a Test Event with a real image URL from the web
        db.Events.Add(new RaceDay.Models.Event
        {
            OrganiserID = org.OrganiserID,
            Name = "Cape Town Marathon 2026",
            Description = "The ultimate 42.2km scenic route through the Mother City.",
            EventDate = DateTime.Today.AddDays(30),
            StartTime = "06:00",
            Location = "Cape Town Stadium",
            Status = "Open",
            ImageUrl = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5?auto=format&fit=crop&w=800&q=80"
        });
        db.SaveChanges();
    }
}

// 7. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Commented out for Docker/HTTP compatibility
// app.UseHttpsRedirection();

// THIS IS CRUCIAL FOR THE LOCAL FALLBACK IMAGES TO DISPLAY:
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Exposes the auto-generated Program class to the test project.
public partial class Program { }
