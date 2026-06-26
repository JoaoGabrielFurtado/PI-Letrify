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

// 1. CONFIGURAÇÃO DO BANCO DE DADOS (AZURE / SQL SERVER)
var connectionString = builder.Configuration.GetConnectionString("Azure");
builder.Services.AddDbContext<Banco>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(20),
            errorNumbersToAdd: null);
    }));

// 2. CONTROLLERS E TRATAMENTO DE CICLOS DE JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();

// 3. POLÍTICA DE CORS (PERMITIR CREDENCIAIS PARA SIGNALR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend",
        policy => policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://letrify.vercel.app" // Produção Vercel
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Obrigatório para o SignalR funcionar via WebSockets
});

// 4. CONFIGURAÇÃO DE AUTENTICAÇÃO E VALIDAÇÃO JWT
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

    // CORREÇÃO CRUCIAL: Captura automática do token enviado pelo SignalR via Query String
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // Se a requisição for para qualquer um dos hubs, injeta o token no contexto
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// 5. INJEÇÃO DE DEPENDÊNCIAS DOS SERVIÇOS DO ECOSSISTEMA
builder.Services.AddHttpClient<OpenAIServices>();
builder.Services.AddScoped<OpenAIServices>();
builder.Services.AddHttpClient<LivroIngestaoService>();
builder.Services.AddScoped<LivroIngestaoService>();
builder.Services.AddHttpClient<GeminiServices>();
builder.Services.AddHostedService<VerificadorStreak>();
builder.Services.AddSingleton<QdrantServices>();
builder.Services.AddHostedService<LimpezaBancoChat>();
builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddScoped<NotificacaoService>();

// 6. ADICIONA SUPORTE AO SIGNALR
builder.Services.AddSignalR();

// 7. POLÍTICA ANTI-SPAM (RATE LIMITING)
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

// 8. PIPELINE DE EXECUÇÃO HTTP (MIDDLEWARES)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// O CORS DEVE vir estritamente antes da Autenticação e Autorização!
app.UseCors("PermitirFrontend");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

// 9. MAPEAMENTO DE CONTROLLERS DA API
app.MapControllers();

// 10. MAPEAMENTO DE HUBS DO SIGNALR (Sincronizados milimetricamente com o CORS)
app.MapHub<ChatHub>("/hubs/chat").RequireCors("PermitirFrontend");
app.MapHub<NotificacaoHub>("/hubs/notificacoes").RequireCors("PermitirFrontend");
app.MapHub<GrupoHub>("/hubs/grupo").RequireCors("PermitirFrontend");
app.MapHub<DMHub>("/hubs/dm").RequireCors("PermitirFrontend");

app.Run();