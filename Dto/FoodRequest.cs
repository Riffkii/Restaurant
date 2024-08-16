namespace Restaurant.Dto
{
    public class FoodRequest
    {
        public string Name { get; set; } = null!;

        public int Price { get; set; }

        public int Stock { get; set; }
    }
}
