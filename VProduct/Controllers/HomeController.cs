using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using VProduct.Helper;
using VProduct.Models;

namespace VProduct.Controllers
{
    [Route("api")] //Testowy kontroler
    public class HomeController : Controller
    {
        VProductAPI api = new VProductAPI();
        [Route("Test")]
        public async Task<IActionResult> Index()
        {
            TestModel tstMdl = new TestModel();
            HttpClient client = api.Inital();
            HttpResponseMessage res = await client.GetAsync("api/TestModel");
            if(res.IsSuccessStatusCode)
            {
                var result = res.Content.ReadAsStringAsync().Result;
                tstMdl = JsonConvert.DeserializeObject<TestModel>(result);
            }

            return new JsonResult(tstMdl);
        }

        [Route("TestTwo")]
        public IActionResult Random()
        {
            TestModel tstMdl = new TestModel();
            tstMdl.ID = 1;
            tstMdl.Name = "test";
            return new JsonResult(tstMdl);
        }
    }
}
