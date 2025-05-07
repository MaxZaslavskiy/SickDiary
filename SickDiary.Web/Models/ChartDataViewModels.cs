using System.Globalization;
namespace SickDiary.Web.Models;

public class ChartDataViewModel
{
    public List<DateTime> Labels { get; set; } = new List<DateTime>();
    public List<double> BloodGlucoseLevels { get; set; } = new List<double>();
    public List<double> InsulinDoses { get; set; } = new List<double>();
}