using ab_testing_dotnet_sample.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Hook ConfigCat up via the application builder
builder.UseConfigCat();

// Configure Amplitude and add a named HTTP client for accessing the Amplitude HTTP API
builder.Services.Configure<AmplitudeOptions>(builder.Configuration.GetSection("Amplitude"));
builder.Services.AddHttpClient("amplitude",
    options => options.BaseAddress = new Uri("https://api2.amplitude.com"));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
