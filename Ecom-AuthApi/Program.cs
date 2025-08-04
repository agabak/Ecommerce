using ECom.Infrastructure.DataAccess.User.Repositories;
using ECom.Infrastructure.DataAccess.User.Services;
using Ecom_AuthApi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IUserRepository>
    (_ => new UserMySqlRepository(builder.Configuration.GetConnectionString("mysqlConnection")!));



// ---------- JWT ----------
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new("Jwt:Key not found");
var issuer = builder.Configuration["Jwt:Issuer"] ?? throw new("Jwt:Issuer not found");
var audience = builder.Configuration["Jwt:Audience"] ?? throw new("Jwt:Audience not found");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;   // for [Authorize]
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;   // when 401
        options.DefaultSignInScheme       = CookieAuthenticationDefaults.AuthenticationScheme;  // <-- NEW
    })
    // ---------- API bearer tokens ----------
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidIssuer   = issuer,
            ValidAudience = audience
        };
    })
    // ---------- Minimal, transient cookie just for external login hand-off ----------
    .AddCookie(options =>
    {
        options.Cookie.Name     = "__ExternalLogin";   // short-lived, secure
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = false;
    })
    // ---------- Google ---------- 
    .AddGoogle(opts =>
    {
        opts.ClientId     = builder.Configuration["Auth:Google:ClientId"]!;
        opts.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"]!;
        opts.CallbackPath = "/signin-google";
        opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;      // <-- NEW
    })
    // ---------- Microsoft ----------
    .AddMicrosoftAccount(opts =>
    {
        opts.ClientId     = builder.Configuration["Auth:Microsoft:ClientId"]!;
        opts.ClientSecret = builder.Configuration["Auth:Microsoft:ClientSecret"]!;
        opts.CallbackPath = "/signin-microsoft";        // give it its own path
        opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;      // <-- NEW
    });

builder.Services.AddAuthorization();
// ---------- Facebook ----------
//.AddFacebook(opts =>
//{
//    opts.AppId        = builder.Configuration["Auth:Facebook:AppId"]!;
//    opts.AppSecret    = builder.Configuration["Auth:Facebook:AppSecret"]!;
//    opts.CallbackPath = "/signin-facebook";                       // unique path
//    opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    // Optional extras:
//    opts.Scope.Add("email");
//    opts.Fields.Add("email");
//});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();  // <-- Must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
