using Microsoft.AspNetCore.Mvc;
using CalculatorMVC.Models;

namespace CalculatorMVC.Controllers;

public class CalculatorController : Controller
{
    public IActionResult Index()
    {
        return View(new CalculatorModel());
    }

    [HttpPost]
    public IActionResult Calculate(CalculatorModel model)
    {
        if (model.NumberA is null || model.NumberB is null || string.IsNullOrEmpty(model.Operation))
        {
            model.Error = "Please enter both numbers and select an operation.";
            return View("Index", model);
        }

        model.Result = model.Operation switch
        {
            "add"      => model.NumberA + model.NumberB,
            "subtract" => model.NumberA - model.NumberB,
            "multiply" => model.NumberA * model.NumberB,
            _          => null
        };

        if (model.Result is null)
            model.Error = "Unknown operation.";

        return View("Index", model);
    }
}
