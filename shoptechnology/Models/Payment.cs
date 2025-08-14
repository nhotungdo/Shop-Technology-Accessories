using System;
using System.Collections.Generic;

namespace ShopTechnologyAccessories.Models;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public string Method { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
