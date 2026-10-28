using Sinistros.Application;
using Sinistros.Infrastructure;
using Sinistros.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configuração das camadas Clean Architecture
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Execução segura da carga inicial de dados (Seed Data)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Não foi possível conectar ao banco de dados durante o boot inicial. Verifique se o PostgreSQL está ativo via 'docker compose up -d'.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
