using Microsoft.AspNetCore.Mvc;
using ProductAPI.Models;
using System.Data.SqlClient;
using ProductAPI.Data;
using Dapper;

namespace ProductAPI.Controllers
{
    [Route("api/[controller]")]
    public class DefaultController : Controller
    {
        private readonly IConfiguration configuration;

        public DefaultController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        [Route("GetTestModel")]
        public TestModel GetTestModel() 
        {
            //SqlConnection con = new SqlConnection(this.configuration.GetConnectionString(Context.NAME_OF_DATABASE));

            //var result = con.Query<object>("select * from Products").ToList();

            return new TestModel()
            {
                ID= 1,
                Name = "Test"
            };        
        }
    }
}
