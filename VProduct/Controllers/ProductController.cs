using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using VProduct.Models;
using Dapper;
using Humanizer;
using System.Web;

namespace VProduct.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IConfiguration configuration;

        public ProductController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [Route("AddNewProduct")]
        [HttpPost]
        public async Task<IActionResult> AddNewProduct([FromBody] Product product)
        {
            if (product is null || string.IsNullOrEmpty(product.Name.Trim()) || product.CategoriesIds is null || product.CategoriesIds.Count < 1) 
            { return BadRequest(); };

            using (IDbConnection db = new SqlConnection(configuration.GetConnectionString(Helper.Helper.NAME_OF_DATABASE)))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        product.Name = HttpUtility.HtmlEncode(product.Name);

                        string queryCount = @"SELECT COUNT(*) FROM product_in_category
                                              WHERE id_category in @CategoriesIds AND id_product = 
                                             (SELECT Id FROM Product WHERE Name = @Name)";

                        int? countProduct = await db.QueryFirstOrDefaultAsync<int>(queryCount, product, transaction);

                        if (!countProduct.HasValue || countProduct > 0)
                        {
                            throw new InvalidOperationException($"{product.Name} is exist in category {product.CategoriesIds} !");
                        }

                        string query = @"INSERT INTO Product (Name,Price,Calories) VALUES (@Name,@Price,@Calories);SELECT SCOPE_IDENTITY();";
                        int? productId = (await db.QueryAsync<int>(query, product, transaction)).SingleOrDefault();
                        if (productId != null && productId > 0)
                        {
                            foreach (int idCategory in product.CategoriesIds)
                            {
                                query = @"INSERT INTO product_in_category (id_product,id_category) VALUES (@id_product,@IdCategory)";
                                int rowsAffected = await db.ExecuteAsync(query, new { id_product = productId, IdCategory = idCategory }, transaction);
                                if (rowsAffected < 1)
                                    throw new InvalidOperationException();
                            }

                            transaction.Commit();
                            return Ok();
                        }
                        else
                            throw new ArgumentNullException(nameof(product));
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [Route("GetProductsByCategory")]
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory([FromQuery] int categoryId)
        {
            List<Product> products = new List<Product>();
            using (IDbConnection db = new SqlConnection(configuration.GetConnectionString(Helper.Helper.NAME_OF_DATABASE)))
            {
                string query = "select p.* from Product p inner join product_in_category pc on p.id = pc.id_product where pc.id_category = @idCategory";
                products = (await db.QueryAsync<Product>(query,new { idCategory = categoryId})).ToList();
            }

            return new JsonResult(products);
        }

        [Route("DeleteProduct")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct([FromQuery] int productId)
        {
            using (IDbConnection db = new SqlConnection(configuration.GetConnectionString(Helper.Helper.NAME_OF_DATABASE)))
            {
                string query = "DELETE FROM product_in_category WHERE id_product = @idProd; DELETE FROM Product WHERE id = @idProd";
                int rowsAffected = await db.ExecuteAsync(query, new { idProd = productId });
                if (rowsAffected < 1)
                    throw new InvalidOperationException();
            }

            return Ok();
        }
    }
}
