using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Common.Models.Users;

public class UserAddress
{
    public Guid AddressId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty ;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

}
