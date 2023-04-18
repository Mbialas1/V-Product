﻿using Humanizer;

namespace VProduct.Helper
{
    public class Helper
    {
        public static readonly string NAME_OF_DATABASE = "ConnetionDBProduct";
        public static readonly string KEY_OF_CACHE_CATEGORIES = "categoriesCache";
    }

    public class VProductAPI
    {
        private const string UriToAPI = "https://localhost:7098/"; // ToDo Przenieść do configa.
        public HttpClient Inital()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(UriToAPI);
            return client;
        }
    }
}
