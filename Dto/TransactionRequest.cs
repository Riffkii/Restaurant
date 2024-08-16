namespace Restaurant.Dto
{
    public class TransactionRequest
    {
        public int CustomerId { get; set; }

        public int FoodId { get; set; }

        public int Qty { get; set; }
    }
}
