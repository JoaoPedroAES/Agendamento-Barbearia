using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Configuração do CORS para permitir requisições do frontend (localhost:3000)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000") // Permite apenas esta origem
                                .AllowAnyHeader() // Permite qualquer cabeçalho
                                .AllowAnyMethod(); // Permite qualquer método HTTP
                      });
});

// Configuração da conexão com o banco de dados PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("AppDbConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// Adiciona suporte a controladores (API)
builder.Services.AddControllers();

// Configuração do Swagger para documentação da API
builder.Services.AddSwaggerGen(options =>
{
    // Define o esquema de segurança para autenticação JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Autenticação JWT (Bearer Token). Insira 'Bearer' [espaço] e o token.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Define os requisitos de segurança para os endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configuração do Identity para autenticação e autorização
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{ 
    // Configurações de senha
    options.Password.RequireDigit = false; // Não exige dígito
    options.Password.RequireLowercase = false; // Não exige letra minúscula
    options.Password.RequireNonAlphanumeric = false; // Não exige caractere especial
    options.Password.RequireUppercase = false; // Não exige letra maiúscula
    options.Password.RequiredLength = 6; // Exige no mínimo 6 caracteres
})
    .AddRoles<IdentityRole>() // Adiciona suporte a papéis (roles)
    .AddEntityFrameworkStores<AppDbContext>() // Usa o AppDbContext para armazenar dados do Identity
    .AddApiEndpoints(); // Adiciona suporte a endpoints da API

// Configuração da autenticação com Bearer Token
builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);

// Configuração da autorização
builder.Services.AddAuthorizationBuilder();

// Registro de serviços no container de injeção de dependência
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBarberService, BarberService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServicesService, ServicesService>();
builder.Services.AddScoped<IWorkScheduleService, WorkScheduleService>();

var app = builder.Build();

// Inicializa os papéis e o administrador padrão
await app.SeedRolesAndAdminAsync();

// Configuração do ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita o Swagger
    app.UseSwaggerUI(); // Habilita a interface do Swagger
}

// Habilita o CORS com a política definida
app.UseCors(MyAllowSpecificOrigins);

app.MapIdentityApi<ApplicationUser>(); // Mapeia os endpoints do Identity

app.UseHttpsRedirection(); // Redireciona para HTTPS

app.UseAuthorization(); // Habilita a autorização

app.MapControllers(); // Mapeia os controladores da API

app.Run(); // Inicia o aplicativo
