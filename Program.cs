using System.Globalization;
using System.Threading.RateLimiting;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Infraestrutura;
using GestaoFinanceiraMEI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
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

        // Reforço de segurança do cookie de autenticação.
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        // IMPORTANTE: só trocar para CookieSecurePolicy.Always depois que o
        // HTTPS estiver ativo e confirmado no domínio de produção — com
        // "Always" antes disso, o navegador descarta o cookie em conexões
        // HTTP e ninguém consegue permanecer logado.
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddHttpContextAccessor();

// Limitação de taxa de requisições (proteção básica contra força bruta e
// varreduras automatizadas) ------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Limite global por IP: não afeta o uso normal de uma pessoa navegando,
    // mas barra scripts/scanners que disparam muitas requisições seguidas.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 0
        });
    });

    // Limite mais restrito específico para login/cadastro, contra
    // tentativas de força bruta de senha ou spam de contas.
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

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

// Cabeçalhos HTTP de segurança básicos, aplicados a toda resposta.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.Append("X-Content-Type-Options", "nosniff");
    headers.Append("X-Frame-Options", "DENY");
    headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' https://cdn.jsdelivr.net 'unsafe-inline'; " +
        "style-src 'self' https://cdn.jsdelivr.net 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self' https://cdn.jsdelivr.net;");
    await next();
});

app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
