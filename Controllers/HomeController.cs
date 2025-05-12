using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JiraApp.ViewModels;

namespace JiraApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string message = null)
    {
        var errorModel = new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        };
        
        if (!string.IsNullOrEmpty(message))
        {
            ViewBag.ErrorMessage = message;
        }
        else if (TempData["ErrorMessage"] != null)
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
        }
        

            
        return View(errorModel);
    }
}
