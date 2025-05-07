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

    public DiaryController(DiaryService diaryService, ClientService clientService, GptService gptService)
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
            model.Records = diary.Records
                .OrderByDescending(r => r.Date) // Сортування за датою і часом у порядку спадання
                .Select((r, index) => new RecordViewModel
                {
                    Date = r.Date.ToLocalTime(), // Конвертуємо UTC у локальний час для відображення
                    BloodGlucoseLevel = r.BloodGlucoseLevel,
                    InsulinDose = r.InsulinDose,
                    CarbohydrateIntake = r.CarbohydrateIntake,
                    WellBeingLevel = (SickDiary.Web.Models.WellBeingLevel)r.WellBeingLevel,
                    PhysicalActivityLevel = (SickDiary.Web.Models.PhysicalActivityLevel)r.PhysicalActivityLevel,
                    Dizziness = r.Dizziness,
                    Sweating = r.Sweating,
                    VisionProblems = r.VisionProblems,
                    Weakness = r.Weakness,
                    MeasurementState = (SickDiary.Web.Models.MeasurementState)r.MeasurementState,
                    Result = (SickDiary.Web.Models.DiseaseState)r.Result,
                    Index = index
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
            // Передаємо записи як є, вони вже в UTC
            string analysis = await _gptService.Analyze(diary.Records);
            TempData["Analysis"] = analysis;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Analysis failed: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    [AuthorizeUser]
    public IActionResult CreateRecord()
    {
        return View(new RecordViewModel { Date = DateTime.Now });
    }

    [HttpPost]
    [AuthorizeUser]
    public async Task<IActionResult> CreateRecord(RecordViewModel model, string Time)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        // Об'єднуємо дату і час, інтерпретуємо як локальний час
        if (TimeSpan.TryParse(Time, out var timeOfDay))
        {
            model.Date = model.Date.Date + timeOfDay;
        }

        // Конвертуємо локальний час у UTC перед збереженням
        var utcDate = model.Date.ToUniversalTime();

        var record = new Record
        {
            Date = utcDate, // Зберігаємо в UTC
            BloodGlucoseLevel = model.BloodGlucoseLevel,
            InsulinDose = model.InsulinDose,
            CarbohydrateIntake = model.CarbohydrateIntake,
            WellBeingLevel = (SickDiary.DL.Entities.WellBeingLevel)model.WellBeingLevel,
            PhysicalActivityLevel = (SickDiary.DL.Entities.PhysicalActivityLevel)model.PhysicalActivityLevel,
            Dizziness = model.Dizziness,
            Sweating = model.Sweating,
            VisionProblems = model.VisionProblems,
            Weakness = model.Weakness,
            MeasurementState = (SickDiary.DL.Entities.MeasurementState)model.MeasurementState,
            Result = SickDiary.DL.Entities.DiseaseState.Normal // Початкове значення, буде перераховано у DiaryService
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

    [AuthorizeUser]
    public async Task<IActionResult> EditRecord(int index)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        var diary = await _diaryService.GetDiaryByUserIdAsync(userId);
        if (diary == null)
        {
            return NotFound();
        }

        // Сортуємо записи так само, як у Index
        var sortedRecords = diary.Records.OrderByDescending(r => r.Date).ToList();
        if (index < 0 || index >= sortedRecords.Count)
        {
            return NotFound();
        }

        var record = sortedRecords[index];
        var model = new RecordViewModel
        {
            Index = index,
            Date = record.Date.ToLocalTime(), // Конвертуємо UTC у локальний час для редагування
            BloodGlucoseLevel = record.BloodGlucoseLevel,
            InsulinDose = record.InsulinDose,
            CarbohydrateIntake = record.CarbohydrateIntake,
            WellBeingLevel = (SickDiary.Web.Models.WellBeingLevel)record.WellBeingLevel,
            PhysicalActivityLevel = (SickDiary.Web.Models.PhysicalActivityLevel)record.PhysicalActivityLevel,
            Dizziness = record.Dizziness,
            Sweating = record.Sweating,
            VisionProblems = record.VisionProblems,
            Weakness = record.Weakness,
            MeasurementState = (SickDiary.Web.Models.MeasurementState)record.MeasurementState,
            Result = (SickDiary.Web.Models.DiseaseState)record.Result
        };

        return View(model);
    }

    [HttpPost]
    [AuthorizeUser]
    public async Task<IActionResult> EditRecord(int index, RecordViewModel model, string Time)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        // Об'єднуємо дату і час, інтерпретуємо як локальний час
        if (TimeSpan.TryParse(Time, out var timeOfDay))
        {
            model.Date = model.Date.Date + timeOfDay;
        }

        // Конвертуємо локальний час у UTC перед збереженням
        var utcDate = model.Date.ToUniversalTime();

        var record = new Record
        {
            Date = utcDate, // Зберігаємо в UTC
            BloodGlucoseLevel = model.BloodGlucoseLevel,
            InsulinDose = model.InsulinDose,
            CarbohydrateIntake = model.CarbohydrateIntake,
            WellBeingLevel = (SickDiary.DL.Entities.WellBeingLevel)model.WellBeingLevel,
            PhysicalActivityLevel = (SickDiary.DL.Entities.PhysicalActivityLevel)model.PhysicalActivityLevel,
            Dizziness = model.Dizziness,
            Sweating = model.Sweating,
            VisionProblems = model.VisionProblems,
            Weakness = model.Weakness,
            MeasurementState = (SickDiary.DL.Entities.MeasurementState)model.MeasurementState,
            Result = SickDiary.DL.Entities.DiseaseState.Normal // Початкове значення, буде перераховано у DiaryService
        };

        try
        {
            await _diaryService.UpdateRecordAsync(userId, index, record);
            return RedirectToAction("Index", "Diary");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [AuthorizeUser]
    public async Task<IActionResult> DeleteRecord(int index)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        try
        {
            var diary = await _diaryService.GetDiaryByUserIdAsync(userId);
            if (diary == null)
            {
                throw new ApplicationException("Diary not found.");
            }

            // Сортуємо записи так само, як у Index
            var sortedRecords = diary.Records.OrderByDescending(r => r.Date).ToList();
            if (index < 0 || index >= sortedRecords.Count)
            {
                throw new ApplicationException("Record not found.");
            }

            // Знаходимо індекс запису у несортованому списку
            var originalIndex = diary.Records.IndexOf(sortedRecords[index]);
            if (originalIndex == -1)
            {
                throw new ApplicationException("Record not found in original list.");
            }

            await _diaryService.DeleteRecordAsync(userId, originalIndex);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    // Цей метод отримує записи щоденника, сортує їх за датою і передає дані в модель.
    [AuthorizeUser]
    [AuthorizeUser]
    public async Task<IActionResult> Charts()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Home");
        }

        var diary = await _diaryService.GetDiaryByUserIdAsync(userId);
        var model = new ChartDataViewModel();

        if (diary != null && diary.Records.Any())
        {
            var sortedRecords = diary.Records.OrderBy(r => r.Date).ToList(); // Зміна на OrderBy
            model.Labels = sortedRecords.Select(r => r.Date.ToLocalTime()).ToList();
            model.BloodGlucoseLevels = sortedRecords.Select(r => r.BloodGlucoseLevel).ToList();
            model.InsulinDoses = sortedRecords.Select(r => r.InsulinDose).ToList();
            Console.WriteLine($"Records found: {sortedRecords.Count}");
        }
        else
        {
            Console.WriteLine("No records found for user.");
        }

        return View(model);
    }

}