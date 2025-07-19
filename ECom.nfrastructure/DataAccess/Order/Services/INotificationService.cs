using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECom.Infrastructure.DataAccess.Order.Services
{
    public interface INotificationService
    {
        Task AddNotificationAsync(Guid userId, Guid sourceId, string type, string source,string message, CancellationToken cancellationToken = default);
    }
}
