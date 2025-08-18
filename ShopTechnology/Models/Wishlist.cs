using System;
using System.Collections.Generic;

namespace ShopTechnology.Models;

public partial class Wishlist
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
