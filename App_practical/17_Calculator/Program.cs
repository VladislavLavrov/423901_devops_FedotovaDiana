using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Настройка OpenTelemetry для сбора метрик

// Настройка OpenTelemetry для метрик
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        // Настройка ресурса
        metrics.SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: "CalculatorApp", serviceVersion: "1.0.0")
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector()
        );

        // Добавление метрик для ASP.NET Core
        metrics.AddMeter("Microsoft.AspNetCore.Hosting");
        metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
        metrics.AddMeter("Microsoft.AspNetCore.Http.Connections");
        metrics.AddMeter("System.Runtime");

        // Добавление инструментации для HTTP запросов
        metrics.AddHttpClientInstrumentation();
        metrics.AddAspNetCoreInstrumentation();

        // Экспорт метрик в Prometheus
        metrics.AddPrometheusExporter();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");

app.MapPrometheusScrapingEndpoint();
app.Run();
