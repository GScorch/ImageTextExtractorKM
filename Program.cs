using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<OcrService>();
builder.Services.AddSingleton<FtpHelper>();
builder.Services.AddSingleton<FileHelper>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages(); // Add Razor Pages if needed
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable static file serving from wwwroot
app.UseStaticFiles();

// Configure routing
app.UseRouting();

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS

// Authorization middleware (if required)
app.UseAuthorization();

// Map controllers and Razor pages
app.MapControllers();
app.MapRazorPages(); // Only if you are using Razor Pages

app.Run();
