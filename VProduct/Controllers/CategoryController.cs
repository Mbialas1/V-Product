using Microsoft.AspNetCore.Mvc;
using VProduct.Models;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace VProduct.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly IConfiguration configuration;

        private object Lock { get; set; }

        public CategoryController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [Route("AddNewCategory")]
        [HttpPost]
        public IActionResult AddNewCategory([FromBody] Category category)
        {
            if (category is null) { return BadRequest(); };

            lock (Lock)
            {
                using (IDbConnection db = new SqlConnection(configuration.GetConnectionString(Helper.Helper.NAME_OF_DATABASE)))
                {
                    string queryCount = @"select count(*) from Category where Name = @Name";
                    int countCategory = db.QueryFirst<int>(queryCount, category);

                    if (countCategory > 0)
                    {
                        throw new InvalidOperationException($"{category.Name} Category is exist !");
                    }
                    string query = @"INSERT INTO Category (Name) VALUES (@Name)";
                    int rowsAffected = db.Execute(query, category);
                    if (rowsAffected > 0)
                    {
                        return Ok(category);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
        }

        [Route("GetAllCategory")]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            using (IDbConnection db = new SqlConnection(configuration.GetConnectionString(Helper.Helper.NAME_OF_DATABASE)))
            {
                string query = @"select * from Category";
                List<Category> categoriesList = (await db.QueryAsync<Category>(query)).ToList();

                if(categoriesList is null)
                    throw new ArgumentNullException(nameof(categoriesList));

                return new JsonResult(categoriesList);
            }
        }

        [Route("DeleteCategoryById")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategoryById([FromQuery] int categoryId)
        {
            lock (Lock)
            {
                using (IDbConnection db = new SqlConnection(configuration.GetConnectionString(Helper.Helper.NAME_OF_DATABASE)))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            string query = @"DELETE FROM product_in_category
                                             WHERE id_category = @id; 
                                             DELETE FROM Category WHERE Id = @id";

                            int rowsAffected = db.Execute(query, new { id = categoryId }, transaction);
                            if(rowsAffected < 1)
                                throw new InvalidOperationException();

                            //Check if some product dont have string in product_in_category
                            query = @"SELECT id
                                      FROM Product
                                      WHERE id NOT IN (
                                      SELECT DISTINCT productId
                                      FROM product_in_category)";

                            List<int> idsProductsWithoutCategory = db.Query<int>(query, transaction).ToList();
                            if(idsProductsWithoutCategory?.Count > 0)
                            {
                                //Delete products without categories
                                query = @"DELETE FROM Product WHERE Id in (@listIds)";
                                rowsAffected = db.Execute(query, new { listIds = idsProductsWithoutCategory }, transaction);
                                if (rowsAffected < 1)
                                    throw new InvalidOperationException();
                            }

                            transaction.Commit();
                            return Ok();
                        }
                        catch(Exception ex)
                        {
                            transaction.Rollback();
                            return BadRequest(ex.Message);
                        }

                    }
                }
            }
        }
    }
}
