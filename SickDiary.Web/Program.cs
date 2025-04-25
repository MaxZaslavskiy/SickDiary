    using MongoDB.Driver;
    using SickDiary.BL.Services;
    using SickDiary.DL.Interfaces;
    using SickDiary.DL.Repositories;

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    // Налаштування сесій
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Сесія активна 30 хвилин
        options.Cookie.HttpOnly = true; // Захист кукі
        options.Cookie.IsEssential = true; // Кукі необхідні для роботи
    });

    // Налаштування MongoDB
    builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient("mongodb+srv://Makson:AoE7SIyGQgu36SWA@sickdiary.nk16dvv.mongodb.net/?retryWrites=true&w=majority&appName=SickDiary"));
    builder.Services.AddSingleton(sp =>
        sp.GetRequiredService<IMongoClient>().GetDatabase("max"));

    // Register your repositories
    builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
    builder.Services.AddTransient<ClientService>();
    builder.Services.AddTransient<DiaryService>();
    builder.Services.AddTransient<GptService>();
    

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
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