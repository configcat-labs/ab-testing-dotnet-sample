using ConfigCat.Client; // Import types from the ConfigCat SDK's namespace
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ab_testing_dotnet_sample.Models;

namespace ab_testing_dotnet_sample.Controllers;

// Inject the ConfigCat client via a constructor parameter
// so we can read the feature flag's value and pass it to the Views/Home/Index.cshtml file to control the "Add to Cart" button
public class HomeController(IConfigCatClient configCatClient) : Controller
{
    public async Task<IActionResult> IndexAsync()
    {
        // A unique user id is required when creating a ConfigCat User Object
        var configCatUser = new User("user-id-123")
        {
            Email = "john@example.com",
            Country = "United Kingdom",
        };

        // Get the flag's latest value
        var isMyFeatureFlagEnabled = await configCatClient.GetValueAsync("myFeatureFlag", false, configCatUser);

        // Return its value to the view
        return View(new IndexViewModel { IsMyFeatureFlagEnabled = isMyFeatureFlagEnabled });
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
