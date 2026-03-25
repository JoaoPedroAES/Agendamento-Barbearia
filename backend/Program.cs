using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Nome da política de CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// --- 1. CONFIGURAÇÃO DE CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000",
                                            "https://agendamento-barbearia-lilac.vercel.app")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// --- 2. BANCO DE DATA (PostgreSQL/Supabase) ---
var connectionString = builder.Configuration.GetConnectionString("AppDbConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();

// --- 3. SWAGGER (Documentação) ---
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Autenticação JWT (Bearer Token). Insira 'Bearer' [espaço] e o token.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// --- 4. IDENTITY & AUTHENTICATION ---
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();

// --- 5. INJEÇÃO DE DEPENDÊNCIA (Serviços) ---
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBarberService, BarberService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServicesService, ServicesService>();
builder.Services.AddScoped<IWorkScheduleService, WorkScheduleService>();

var app = builder.Build();

// --- 6. INICIALIZAÇÃO DO BANCO (MIGRATE + SEED) ---
// Este bloco garante que o banco esteja pronto antes da API começar a receber requisições
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        Console.WriteLine("--> [DATABASE] Verificando migrações pendentes...");
        // MigrateAsync cria as tabelas no Supabase se elas não existirem
        await context.Database.MigrateAsync();
        Console.WriteLine("--> [DATABASE] Migrações aplicadas/verificadas com sucesso!");

        Console.WriteLine("--> [SEEDER] Iniciando DataSeeder...");
        // SeedRolesAndAdminAsync cria as Roles e o Admin padrão
        await DataSeeder.SeedRolesAndAdminAsync(app);
        Console.WriteLine("--> [SEEDER] Dados iniciais populados com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> [ERRO] Falha crítica na inicialização do banco: {ex.Message}");
    }
}

// --- 7. MIDDLEWARES & ROTAS ---

// Habilita Swagger tanto em Dev quanto em Prod no Render para facilitar testes
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

app.MapIdentityApi<ApplicationUser>();

app.UseHttpsRedirection();

app.UseAuthentication(); // Importante: deve vir antes da Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();