using Microsoft.AspNetCore.Mvc;
using SickDiary.BL.Services;
using SickDiary.DL.Entities;
using SickDiary.Web.Filters;
using SickDiary.Web.Models;

namespace SickDiary.Web.Controllers;

public class DiaryController : Controller
{
    private readonly DiaryService _diaryService;
    private readonly ClientService _clientService;
    private readonly GptService _gptService;

    public DiaryController(DiaryService diaryService,
        ClientService clientService,
        GptService gptService)
    {
        _diaryService = diaryService;
        _clientService = clientService;
        _gptService = gptService;
    }

    [AuthorizeUser]
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        var diary = await _diaryService.GetDiaryByUserIdAsync(userId);
        var model = new DiaryViewModel();

        if (diary != null && diary.Records.Any())
        {
            model.Records = diary.Records.Select((r, index) => new RecordViewModel
            {
                Date = r.Date,
                BloodGlucoseLevel = r.BloodGlucoseLevel,
                InsulinDose = r.InsulinDose,
                CarbohydrateIntake = r.CarbohydrateIntake,
                WellBeingLevel = (SickDiary.Web.Models.WellBeingLevel)r.WellBeingLevel,
                PhysicalActivityLevel = (SickDiary.Web.Models.PhysicalActivityLevel)r.PhysicalActivityLevel,
                Dizziness = r.Dizziness,
                Sweating = r.Sweating,
                VisionProblems = r.VisionProblems,
                Weakness = r.Weakness,
                Result = (SickDiary.Web.Models.DiseaseState)r.Result,
                Index = index // Додаємо індекс для редагування / видалення
            }).ToList();
        }

        return View(model);

    }

    [AuthorizeUser]
    public async Task<IActionResult> Analyze()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        var diary = await _diaryService.GetDiaryByUserIdAsync(userId);
        if (diary == null || !diary.Records.Any())
        {
            TempData["Error"] = "No records available for analysis.";
            return RedirectToAction("Index");
        }

        try
        {
            string analysis = await _gptService.Analyze(diary.Records);
            TempData["Analysis"] = analysis;
        }

        catch (Exception ex)
        {
            TempData["Error"] = $"Error during analysis: {ex.Message}";
        }

        return RedirectToAction("Index");
    }


    [AuthorizeUser] // Додаємо захист
    public IActionResult CreateRecord()
    {
        return View(new RecordViewModel { Date = DateTime.Now });
    }

    [HttpPost]
    [AuthorizeUser] // Додаємо захист
    public async Task<IActionResult> CreateRecord(RecordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Отримуємо UserId з сесії
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        var record = new Record
        {
            Date = model.Date,
            BloodGlucoseLevel = model.BloodGlucoseLevel,
            InsulinDose = model.InsulinDose,
            CarbohydrateIntake = model.CarbohydrateIntake,
            WellBeingLevel = (SickDiary.DL.Entities.WellBeingLevel)model.WellBeingLevel,
            PhysicalActivityLevel = (SickDiary.DL.Entities.PhysicalActivityLevel)model.PhysicalActivityLevel,
            Dizziness = model.Dizziness,
            Sweating = model.Sweating,
            VisionProblems = model.VisionProblems,
            Weakness = model.Weakness,
            Result = (SickDiary.DL.Entities.DiseaseState)model.Result // Поки що фіксоване значення, пізніше можна додати логіку
        };

        try
        {
            await _diaryService.AddRecordAsync(userId, record);
            return RedirectToAction("Index", "Diary");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [AuthorizeUser] // Додаємо захист
    public async Task<IActionResult> EditRecord(int index)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        var diary = await _diaryService.GetDiaryByUserIdAsync(userId);
        if (diary == null || index < 0 || index >= diary.Records.Count)
        {
            return NotFound();
        }

        var record = diary.Records[index];
        var model = new RecordViewModel
        {

            Index = index,
            Date = record.Date,
            BloodGlucoseLevel = record.BloodGlucoseLevel,
            InsulinDose = record.InsulinDose,
            CarbohydrateIntake = record.CarbohydrateIntake,
            WellBeingLevel = (SickDiary.Web.Models.WellBeingLevel)record.WellBeingLevel,
            PhysicalActivityLevel = (SickDiary.Web.Models.PhysicalActivityLevel)record.PhysicalActivityLevel,
            Dizziness = record.Dizziness,
            Sweating = record.Sweating,
            VisionProblems = record.VisionProblems,
            Weakness = record.Weakness,
            Result = (SickDiary.Web.Models.DiseaseState)record.Result

        };

        return View(model);
    }

    [HttpPost]
    [AuthorizeUser] // Додаємо захист
    public async Task<IActionResult> EditRecord(int index, RecordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Index = index;
            return View(model);
        }

        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        var record = new Record
        {
            Date = model.Date,
            BloodGlucoseLevel = model.BloodGlucoseLevel,
            InsulinDose = model.InsulinDose,
            CarbohydrateIntake = model.CarbohydrateIntake,
            WellBeingLevel = (SickDiary.DL.Entities.WellBeingLevel)model.WellBeingLevel,
            PhysicalActivityLevel = (SickDiary.DL.Entities.PhysicalActivityLevel)model.PhysicalActivityLevel,
            Dizziness = model.Dizziness,
            Sweating = model.Sweating,
            VisionProblems = model.VisionProblems,
            Weakness = model.Weakness,
            Result = (SickDiary.DL.Entities.DiseaseState)model.Result
        };

        try
        {
            await _diaryService.UpdateRecordAsync(userId, index, record);
            return RedirectToAction("Index", "Diary");
        }

        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.Index = index;
            return View(model);
        }
    }


    // Додали дію DeleteRecord, яка видаляє запис за індексом.

    [AuthorizeUser] // Додаємо захист
    public async Task<IActionResult> DeleteRecord(int index)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        try
        {
            await _diaryService.DeleteRecordAsync(userId, index);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

}