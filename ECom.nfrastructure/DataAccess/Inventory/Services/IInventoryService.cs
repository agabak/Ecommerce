using Ecommerce.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECom.Infrastructure.DataAccess.Inventory.Services;

public interface IInventoryService
{
    Task<Dictionary<Guid, Guid>> UpdateInventoryAfterOrderAsync(List<Item> items, CancellationToken token);
    Task EnsureInventoryRecordAsync(Guid productId, CancellationToken token = default);
}
