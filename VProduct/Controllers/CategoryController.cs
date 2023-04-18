using Microsoft.AspNetCore.Mvc;
using VProduct.Models;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Duende.IdentityServer.Extensions;
using System.Web;
using Microsoft.Extensions.Caching.Distributed;
using Duende.IdentityServer.Services;
using Newtonsoft.Json;
using Microsoft.Identity.Client;

namespace VProduct.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IDistributedCache cache;

        private static object _lock = new object();

        public CategoryController(IConfiguration configuration, IDistributedCache cache)
        {
            this.configuration = configuration;
            this.cache = cache;
        }

        [Route("AddNewCategory")]
        [HttpPost]
        public async Task<IActionResult> AddNewCategory([FromBody] Category category)
        {
            if (category is null || string.IsNullOrEmpty(category.Name?.Trim())) { return BadRequest(); };

            var categoriesJson = await cache.GetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES);

            using (IDbConnection db = new SqlConnection(configuration.GetConnectionString(Helper.Helper.NAME_OF_DATABASE)))
            {
                category.Name = HttpUtility.HtmlEncode(category.Name);
                string queryCount = @"select count(*) from Category where Name = @Name";
                int countCategory = await db.QueryFirstAsync<int>(queryCount, category);

                if (countCategory > 0)
                {
                    throw new InvalidOperationException($"{category.Name} Category is exist !");
                }
                string query = @"INSERT INTO Category (Name) VALUES (@Name); SELECT SCOPE_IDENTITY();";
                int? categoryId = (await db.QueryAsync<int>(query, category)).SingleOrDefault();
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    var categoriesList = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
                    if (category.Id < 1)
                    {
                        category.Id = categoryId.Value;
                    }
                    categoriesList.Add(category);
                    await cache.SetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES, JsonConvert.SerializeObject(categoriesList));
                    return Ok(category);
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        [Route("GetAllCategory")]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var cacheEntry = await cache.GetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES);

            if (cacheEntry != null)
            {
                var categoriesList = JsonConvert.DeserializeObject<List<Category>>(cacheEntry);
                return new JsonResult(categoriesList);
            }
            else
            {
                using (IDbConnection db = new SqlConnection(configuration.GetConnectionString(Helper.Helper.NAME_OF_DATABASE)))
                {
                    string query = @"select * from Category";
                    List<Category> categoriesList = (await db.QueryAsync<Category>(query)).ToList();

                    if (categoriesList is null)
                        throw new ArgumentNullException(nameof(categoriesList));

                    var cacheOptions = new DistributedCacheEntryOptions()
                             .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
                    await cache.SetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES, JsonConvert.SerializeObject(categoriesList), cacheOptions);

                    return new JsonResult(categoriesList);
                }
            }
        }

        [Route("DeleteCategoryById")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategoryById([FromQuery] int categoryId)
        {
            if (categoryId < 1) { return BadRequest(); }

            var categoriesJson = await cache.GetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES);

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

                        int rowsAffected = await db.ExecuteAsync(query, new { id = categoryId }, transaction);
                        if (rowsAffected < 1)
                            throw new InvalidOperationException();

                        //Check if some product dont have string in product_in_category - Cache
                        query = @"SELECT id
                                      FROM Product
                                      WHERE id NOT IN (
                                      SELECT DISTINCT id_product
                                      FROM product_in_category)";

                        List<int> idsProductsWithoutCategory = (await db.QueryAsync<int>(query, transaction: transaction)).ToList();
                        if (idsProductsWithoutCategory?.Count > 0)
                        {
                            //Delete products without categories
                            query = @"DELETE FROM Product WHERE Id in @listIds";
                            rowsAffected = await db.ExecuteAsync(query, new { listIds = idsProductsWithoutCategory.AsList() }, transaction);
                            if (rowsAffected < 1)
                                throw new InvalidOperationException();
                        }

                        transaction.Commit();

                        if (categoriesJson != null)
                        {
                            var categoriesList = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
                            categoriesList?.Remove(categoriesList.First(category => category.Id == categoryId));
                            await cache.SetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES, JsonConvert.SerializeObject(categoriesList));
                        }

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest(ex.Message);
                    }

                }
            }
        }
    }
}
