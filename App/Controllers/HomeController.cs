using ConfigCat.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;

namespace AB_test_app.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfigCatClient configCatClient;

        public HomeController(IConfigCatClient configCatClient)
        {
            this.configCatClient = configCatClient;
        }

        // creating a user
        readonly User userObject = new User("UID-132dadss4-12-55");
        public IActionResult Index()
        {
            // assigning the flag value depending on the ConfigCat response
            bool isFlagOn = this.configCatClient.GetValue("addtocartflag", false, userObject);

            ViewBag.Style = GetStyle(isFlagOn);
            //ViewBag.flag = isFlagOn;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCartForm()
        {
            // assigning the flag value depending on the ConfigCat response
            bool isFlagOn = this.configCatClient.GetValue("addtocartflag", false, userObject);

            await LogEventToAmplitude(isFlagOn, userObject);

            // Redirect back to the same page
            return RedirectToAction("Index");
        }

        private string GetStyle(bool isFlagOn
        
            if (isFlagOn)
            {
                // setting the new style of the add-to-chart submit form
                return "border: 1px solid green; color:green";
            }
            else
            {
                return "";
            }
        }

        private async Task LogEventToAmplitude(bool isFlagOn, User userObject)
        {
            // HTTP POST request body:
            var data = new
            {
                api_key = "deddaa2d8d60d0417b33e9a3d99fa0e3",
                events = new[]
                {
                    new
                    {
                        user_id = userObject.Identifier,
                        event_type = isFlagOn ? "Variant Cart Button" : "Original Cart Button",
                    }
                }
            };

            // Create an HttpClient
            var client = new HttpClient();

            // Send POST request to Amplitude
            var response = await client.PostAsync(
                "https://api.amplitude.com/2/httpapi",
                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Response failed while logging to amplitude");
            }

            Console.WriteLine("Event logged successfully");
        }
    } 
}
