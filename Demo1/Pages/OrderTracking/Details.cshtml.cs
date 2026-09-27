using Demo1.Data;
using Demo1.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Pages.OrderTracking;

public class DetailsModel(ApplicationDbContext db) : PageModel
{
    public Order? Order { get; private set; }
    public async Task OnGetAsync(string? orderCode, string? phoneNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderCode) || string.IsNullOrWhiteSpace(phoneNumber)) return;
        var code = orderCode.Trim().ToUpperInvariant();
        var phone = phoneNumber.Trim();
        Order = await db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.OrderCode == code && x.PhoneNumber == phone, cancellationToken);
    }
}
