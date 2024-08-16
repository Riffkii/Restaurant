using System;
using System.Collections.Generic;

namespace Restaurant.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int FoodId { get; set; }

    public int Qty { get; set; }

    public int TotalPrice { get; set; }

    public DateOnly CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Food Food { get; set; } = null!;
}
