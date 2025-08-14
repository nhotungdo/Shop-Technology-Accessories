using System;
using System.Collections.Generic;

namespace ShopTechnologyAccessories.Models;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public Guid CartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
