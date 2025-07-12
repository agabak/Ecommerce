using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Common.Models;

public class Cart
{
    public User User { get; set; } = new User();
    public List<Item> Items { get; set; } = new List<Item>();
}
