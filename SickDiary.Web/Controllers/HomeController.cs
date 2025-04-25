using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SickDiary.BL.Services;
using SickDiary.Web.Models;
using Microsoft.AspNetCore.Http;

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
            var client = await _service.Login(model.Login, model.Password, HttpContext.Session);
            _logger.LogInformation("User logged in: {Login}", client.Login);
            return RedirectToAction("Index", "Diary"); // Перенаправляємо на My Diary
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
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
