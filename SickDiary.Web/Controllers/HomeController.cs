using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SickDiary.BL.Services;
using SickDiary.Web.Models;

namespace SickDiary.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ClientService _service;

    public HomeController(ILogger<HomeController> logger, ClientService service)
    {
        _logger = logger;
        _service = service;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _service.SignUp(model.Login, model.Password, model.FullName);
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = await _service.Login(model.Login, model.Password);
            // Тут можна додати логіку авторизації (наприклад, зберегти користувача в сесії)
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}







/*public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ClientService _service;

    public HomeController(ILogger<HomeController> logger, ClientService service)
    {
        _logger = logger;
        _service = service;
    }


    public async Task<IActionResult> Index()
    {
        try
        {
            var reg = await _service.SignUp("123", "123123", "Maxim Pushka");
            _logger.LogInformation("User registered: {Login}", reg.Login);

            var client = await _service.Login("123", "123123");
            _logger.LogInformation("User logged in: {Login}", client.Login);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Index action");
            return View("Error");
        }
    }

---------------------------------------------------------------------
    /*public async Task<IActionResult> Index()
    {
        var reg = await _service.SignUp("123", "123123", "Maxim Pushka");
        var client = await _service.Login("123", "123123");
        return View();
    }
-----------------------------------------------------------------
    
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
*/