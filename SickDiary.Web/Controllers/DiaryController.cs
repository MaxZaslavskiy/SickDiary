using Microsoft.AspNetCore.Mvc;
using SickDiary.BL.Services;
using SickDiary.DL.Entities;
using SickDiary.Web.Models;

namespace SickDiary.Web.Controllers;

public class DiaryController : Controller
{
    private readonly DiaryService _diaryService;
    private readonly ClientService _clientService;

    public DiaryController(DiaryService diaryService, ClientService clientService)
    {
        _diaryService = diaryService;
        _clientService = clientService;
    }

    public IActionResult CreateRecord()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecord(RecordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Для простоти припускаємо, що користувач із Id=1 увійшов
        // У реальному додатку ти маєш отримати Id із сесії після логіну
        int userId = 1;

        var record = new Record
        {
            Date = model.Date,
            BloodGlucoseLevel = model.BloodGlucoseLevel,
            InsulinDose = model.InsulinDose,
            CarbohydrateIntake = model.CarbohydrateIntake,
            WellBeingLevel = (DL.Entities.WellBeingLevel)model.WellBeingLevel,
            PhysicalActivityLevel = (DL.Entities.PhysicalActivityLevel)model.PhysicalActivityLevel,
            Dizziness = model.Dizziness,
            Sweating = model.Sweating,
            VisionProblems = model.VisionProblems,
            Weakness = model.Weakness,
            Result = DiseaseState.Normal // Поки що фіксоване значення, пізніше можна додати логіку
        };

        try
        {
            await _diaryService.AddRecordAsync(userId, record);
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}