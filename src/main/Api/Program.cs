using System.Text;
using Api.Configuration;
using Api.Middleware;
using Application;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.Configure<AdminAuthOptions>(builder.Configuration.GetSection(AdminAuthOptions.SectionName));
var apiOptions = builder.Configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>() ?? new ApiOptions();
var authOptions = builder.Configuration.GetSection(AdminAuthOptions.SectionName).Get<AdminAuthOptions>()
    ?? throw new InvalidOperationException("Admin auth configuration is missing.");
ValidateApi(apiOptions);
ValidateAdminAuth(authOptions);
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtSecret));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authOptions.JwtIssuer,
            ValidAudience = authOptions.JwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "Autenticacao administrativa obrigatoria. Faca login novamente e tente outra vez."
                }));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "Seu usuario nao tem permissao para acessar este recurso."
                }));
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseMiddleware<ExceptionMiddleware>();
app.UseStaticFiles();
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DbSeeder.SeedAsync(app.Services);

app.Run();

static void ValidateAdminAuth(AdminAuthOptions options)
{
    if (string.IsNullOrWhiteSpace(options.Email))
        throw new InvalidOperationException("AdminAuth.Email is required.");

    if (string.IsNullOrWhiteSpace(options.PasswordHash))
        throw new InvalidOperationException("AdminAuth.PasswordHash is required.");

    if (string.IsNullOrWhiteSpace(options.JwtIssuer))
        throw new InvalidOperationException("AdminAuth.JwtIssuer is required.");

    if (string.IsNullOrWhiteSpace(options.JwtAudience))
        throw new InvalidOperationException("AdminAuth.JwtAudience is required.");

    if (string.IsNullOrWhiteSpace(options.JwtSecret) || options.JwtSecret.Length < 32)
        throw new InvalidOperationException("AdminAuth.JwtSecret must be at least 32 characters long.");

    if (options.TokenExpirationMinutes <= 0)
        throw new InvalidOperationException("AdminAuth.TokenExpirationMinutes must be greater than zero.");
}

static void ValidateApi(ApiOptions options)
{
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
        throw new InvalidOperationException("Api.BaseUrl is required.");
}
