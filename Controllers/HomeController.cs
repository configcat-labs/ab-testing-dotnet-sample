using System.Diagnostics;
using System.Text.Json.Nodes;
using ab_testing_dotnet_sample.Configuration;
using ab_testing_dotnet_sample.Models;
using ConfigCat.Client; // Import types from the ConfigCat SDK's namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ab_testing_dotnet_sample.Controllers;

// Inject the ConfigCat client via a constructor parameter
// so we can read the feature flag's value and pass it to the Views/Home/Index.cshtml file to control the "Add to Cart" button
public class HomeController(
    IConfigCatClient configCatClient,
    IHttpClientFactory httpClientFactory,
    IOptions<AmplitudeOptions> amplitudeOptions,
    ILogger<HomeController> logger)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        var configCatUser = CreateConfigCatUser();

        // Get the flag's latest value
        var value = await configCatClient.GetValueAsync("addToCartButtonAbTest", "dark", configCatUser);

        var addToCartButtonVariation = value;

        // Return its value to the view
        return View(new IndexViewModel { AddToCartButtonVariation = addToCartButtonVariation });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCartForm([FromForm] string addToCartButtonVariation)
    {
        var configCatUser = CreateConfigCatUser();

        await LogEventToAmplitude(addToCartButtonVariation, configCatUser);

        // Redirect back to the same page
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

    /* Helper methods */

    private User CreateConfigCatUser()
    {
        // A unique user id is required when creating a ConfigCat User Object
        // (we use a hard-coded user id here but you usually obtain it from HttpContext.User)
        return new User("user-id-123")
        {
            Email = "john@example.com",
            Country = "United Kingdom",
        };
    }

    private async Task LogEventToAmplitude(string addToCartButtonVariation, User configCatUser)
    {
        // HTTP POST request body:
        var data = new JsonObject
        {
            ["api_key"] = amplitudeOptions.Value.ApiKey,
            ["events"] = new JsonArray
            {
                new JsonObject()
                {
                    ["user_id"] = configCatUser.Identifier,
                    ["event_type"] = "Add to Cart",
                    ["event_properties"] = new JsonObject
                    {
                        ["buttonColor"] = addToCartButtonVariation
                    }
                }
            }
        };

        // Create an HttpClient
        var client = httpClientFactory.CreateClient("amplitude");

        // Send POST request to Amplitude
        try
        {
            var response = await client.PostAsync("2/httpapi", JsonContent.Create(data));
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send event to Amplitude.");
            return;
        }

        logger.LogInformation("Event successfully sent to Amplitude.");
    }
}
