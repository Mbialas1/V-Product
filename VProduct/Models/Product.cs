namespace VProduct.Models
{
    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Calories { get; set; }
        public List<int> CategoriesIds { get; set; }
    }
}
