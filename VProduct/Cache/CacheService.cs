using Duende.IdentityServer.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using VProduct.Models;

namespace VProduct.Cache
{
    public class CacheService
    {
        private readonly IDistributedCache cache;
        private readonly IConfiguration _configuration;

        public CacheService(IDistributedCache cache, IConfiguration configuration)
        {
            this.cache = cache;
            _configuration = configuration;
        }

        public async Task<List<Category>> GetCategories()
        {
            List<Category> categoriesList;
            var cacheEntry = await cache.GetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES);
            if (cacheEntry is null)
            {
                return new List<Category>();
            }
            else
            {
                categoriesList = JsonConvert.DeserializeObject<List<Category>>(cacheEntry);
                return categoriesList;
            }
        }

        public async Task LoadCategories(List<Category> categoriesList)
        {
            var cacheOptions = new DistributedCacheEntryOptions()
                             .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
            await cache.SetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES, JsonConvert.SerializeObject(categoriesList), cacheOptions);
        }

        public async Task AddCategory(Category category) // Po przez interface dodawać.
        {
            var categoriesJson = await cache.GetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES);
            var categoriesList = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
            categoriesList.Add(category);
            await cache.SetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES, JsonConvert.SerializeObject(categoriesList));
        }

        public async Task DeleteCategory(int categoryId)
        {
            var categoriesJson = await cache.GetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES);
            var categoriesList = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
            categoriesList?.Remove(categoriesList.First(category => category.Id == categoryId));
            await cache.SetStringAsync(Helper.Helper.KEY_OF_CACHE_CATEGORIES, JsonConvert.SerializeObject(categoriesList));
        }
    }
}
