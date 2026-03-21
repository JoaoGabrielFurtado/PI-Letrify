using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System.Text;

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

var chaveSecreta = builder.Configuration["Jwt:Key"];
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("PermitirTudo");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
