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

var connectionString = builder.Configuration.GetConnectionString("Azure");
builder.Services.AddDbContext<Banco>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5, 
            maxRetryDelay: TimeSpan.FromSeconds(20), 
            errorNumbersToAdd: null);
    }));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000") // localhost do front
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Essa é a trava que permite o SignalR funcionar
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
    options.RequireHttpsMetadata = false;
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

    // O front envia: new HubConnectionBuilder().withUrl("/hubs/notificacoes?access_token=SEU_TOKEN")
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHttpClient<GeminiServices>();
builder.Services.AddSingleton<QdrantServices>();
builder.Services.AddHostedService<LimpezaBancoChat>();
builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<NotificacaoService>(); 
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapControllers();
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHub<NotificacaoHub>("/hubs/notificacoes");
    app.MapHub<GrupoHub>("/hubs/grupo");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("PermitirFrontend");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();