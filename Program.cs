using System.Globalization;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Infraestrutura;
using GestaoFinanceiraMEI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cultura padrão pt-BR ---------------------------------------------------
// Define pt-BR como cultura padrão da aplicação (independentemente da
// cultura do sistema operacional onde ela rodar), garantindo formatação
// de moeda/data consistente. A interpretação de números digitados pelo
// usuário nos formulários, porém, é tratada à parte pelo DecimalModelBinder
// abaixo, que aceita tanto vírgula quanto ponto como separador decimal.
var culturaPadrao = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaPadrao;
CultureInfo.DefaultThreadCurrentUICulture = culturaPadrao;

// Banco de dados (SQL Server / MS SQL Express) ---------------------------
// Em desenvolvimento local, aponta por padrão para o LocalDB que acompanha
// o Visual Studio (appsettings.json). Em produção (hospedagem no Somee.com),
// o valor real vem de appsettings.Production.json, com a connection string
// do banco MS SQL Express fornecido pelo provedor.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// MVC --------------------------------------------------------------------
// O DecimalModelBinderProvider é registrado antes dos binders padrão para
// que campos decimal/decimal? dos formulários aceitem tanto "3,45" quanto
// "3.45" como entrada, sem depender da cultura corrente do servidor.
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

// Serviços de domínio ------------------------------------------------
builder.Services.AddScoped<IFluxoCaixaService, FluxoCaixaService>();

// Autenticação por cookie ---------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Conta/Login";
        options.LogoutPath = "/Conta/Logout";
        options.AccessDeniedPath = "/Conta/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Cria o banco de dados (se ainda não existir) a partir do modelo -----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Erro");
    app.UseHsts();
}

// Força pt-BR em toda a aplicação (exibição de datas/moeda), independente
// da cultura instalada na máquina onde o servidor roda.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaPadrao),
    SupportedCultures = new[] { culturaPadrao },
    SupportedUICultures = new[] { culturaPadrao }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
