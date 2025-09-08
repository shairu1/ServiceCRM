using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceCRM.Data;
using System.Threading.Tasks;


namespace ServiceCRM.Controllers;

[Authorize]
public class TestDataController : Controller
{
    private readonly TestDataGenerator _dataGenerator;

    public TestDataController(TestDataGenerator dataGenerator)
    {
        _dataGenerator = dataGenerator;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int count = 100, int serviceCenterId = 1)
    {
        try
        {
            await _dataGenerator.GenerateTestOrdersAsync(count, serviceCenterId);
            TempData["Message"] = $"Успешно создано {count} тестовых заказов!";
            TempData["MessageType"] = "success";
        }
        catch (System.Exception ex)
        {
            TempData["Message"] = $"Ошибка: {ex.Message}";
            TempData["MessageType"] = "danger";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        try
        {
            await _dataGenerator.ClearTestDataAsync();
            TempData["Message"] = "Тестовые данные успешно удалены!";
            TempData["MessageType"] = "success";
        }
        catch (System.Exception ex)
        {
            TempData["Message"] = $"Ошибка: {ex.Message}";
            TempData["MessageType"] = "danger";
        }

        return RedirectToAction(nameof(Index));
    }
}