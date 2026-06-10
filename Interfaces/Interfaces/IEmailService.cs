using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(
            string toEmail,
            string toName,
            string orderCode,
            decimal total,
            List<(string Name, int Qty, decimal Price)> items,
            string shippingAddress);
    }
}
