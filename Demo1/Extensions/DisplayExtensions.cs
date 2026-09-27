using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Demo1.Models;

namespace Demo1.Extensions;

public static class DisplayExtensions
{
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

    public static string ToVnd(this decimal value) => $"{value.ToString("N0", VietnameseCulture)} ₫";

    public static string ToVietnamese(this OrderStatus status)
        => typeof(OrderStatus).GetMember(status.ToString()).FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?.Name ?? status.ToString();
}
