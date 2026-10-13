using TH4.Models.Entities;

namespace TH4.Data;

/* Khởi tạo dữ liệu mẫu một lần khi ứng dụng khởi động */
public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        if (!context.Suppliers.Any())
        {
            context.Suppliers.AddRange(SeedData.Suppliers);
            context.SaveChanges();
        }

        if (!context.Materials.Any())
        {
            context.Materials.AddRange(SeedData.Materials);
            context.SaveChanges();
        }
    }
}
