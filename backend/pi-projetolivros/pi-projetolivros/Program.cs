using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pi_projetolivros.Hubs;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("Azure");
builder.Services.AddDbContext<Banco>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var chaveSecreta = builder.Configuration["Jwt:Key"] ?? throw new Exception("A chave secreta do JWT não foi encontrada!");
var chaveEmBytes = Encoding.UTF8.GetBytes(chaveSecreta);
 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // alterar caso deixe o projeto online (= true)
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, 
        IssuerSigningKey = new SymmetricSecurityKey(chaveEmBytes),

        ValidateIssuer = true, 
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true, 
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateLifetime = true, 
        ClockSkew = TimeSpan.Zero 
    };
});

builder.Services.AddHttpClient<GeminiServices>();
builder.Services.AddSingleton<QdrantServices>();
builder.Services.AddHostedService<LimpezaBancoChat>();
builder.Services.AddSignalR();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ChatAntiSpam", httpContext =>
    {
        var usuarioId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "desconhecido";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey: usuarioId, factory: _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, 
                Window = TimeSpan.FromMinutes(1), 
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 
            });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapControllers();
    app.MapHub<ChatHub>("/hubs/chat");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("PermitirTudo");

app.UseAuthentication();
app.UseRateLimiter();    
app.UseAuthorization();

app.MapControllers();

app.Run();
