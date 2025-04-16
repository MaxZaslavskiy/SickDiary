using MongoDB.Driver;
using SickDiary.BL.Services;
using SickDiary.DL.Interfaces;
using SickDiary.DL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Налаштування MongoDB
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient("mongodb+srv://Makson:AoE7SIyGQgu36SWA@sickdiary.nk16dvv.mongodb.net/?retryWrites=true&w=majority&appName=SickDiary"));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("max"));

//Налаштування сесій
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Сесыя активна 30 хвилин
    options.Cookie.HttpOnly = true; // Захист кукі
    options.Cookie.IsEssential = true; // Кукі необхідні для роботи
});

// Register your repositories
builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddTransient<ClientService>();
builder.Services.AddTransient<DiaryService>();

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

// Додаємо використання сесій перед авторизацією
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();