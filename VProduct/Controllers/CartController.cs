using Microsoft.AspNetCore.Mvc;

namespace VProduct.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
