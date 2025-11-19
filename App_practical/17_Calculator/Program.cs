using Calculator.Data;
using Microsoft.EntityFrameworkCore;
using Confluent.Kafka;
using Calculator.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------
// ÀŒ√»–Œ¬¿Õ»≈
// ---------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ---------------------
// ¡¿«¿ ƒ¿ÕÕ€’
// ---------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


// ---------------------
// MVC + HTTP CLIENT
// ---------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();


// ---------------------
// KAFKA
// ---------------------
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddSingleton<KafkaProducerHandler>();
builder.Services.AddSingleton<KafkaProducerService<Null, string>>(sp =>
    new KafkaProducerService<Null, string>(
        sp.GetRequiredService<KafkaProducerHandler>().Producer));



var app = builder.Build();

// ---------------------
// PIPELINE
// ---------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Calculator/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=Index}/{id?}");

app.Run();
