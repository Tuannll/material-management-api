using Microsoft.EntityFrameworkCore;
using TH4.Models.Entities;

namespace TH4.Data;

/* Khởi tạo dữ liệu mẫu một lần khi ứng dụng khởi động */
public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        if (context.Database.IsRelational())
        {
            var strategy = context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = context.Database.BeginTransaction();
                if (!context.Suppliers.Any())
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Suppliers ON;");
                    context.Suppliers.AddRange(SeedData.Suppliers);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Suppliers OFF;");
                }

                if (!context.Materials.Any())
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Materials ON;");
                    context.Materials.AddRange(SeedData.Materials);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Materials OFF;");
                }
                transaction.Commit();
            });
        }
        else
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
}
