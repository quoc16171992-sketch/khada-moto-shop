using System.Security.Cryptography;
using Demo1.Data;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Services;

public class OrderCodeGenerator(ApplicationDbContext db) : IOrderCodeGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var suffix = Convert.ToHexString(RandomNumberGenerator.GetBytes(3));
            var code = $"KS-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
            if (!await db.Orders.AnyAsync(x => x.OrderCode == code, cancellationToken))
            {
                return code;
            }
        }

        throw new InvalidOperationException("Không thể tạo mã đơn hàng duy nhất. Vui lòng thử lại.");
    }
}
