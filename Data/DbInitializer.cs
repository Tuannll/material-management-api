using Microsoft.EntityFrameworkCore;
using TH4.Models.Entities;

namespace TH4.Data;

/* Khởi tạo dữ liệu mẫu một lần khi ứng dụng khởi động */
public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        var isRelational = context.Database.IsRelational();

        if (!context.Suppliers.Any())
        {
            if (isRelational)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [Suppliers] ON;");
                    context.Suppliers.AddRange(SeedData.Suppliers);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [Suppliers] OFF;");
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            else
            {
                context.Suppliers.AddRange(SeedData.Suppliers);
                context.SaveChanges();
            }
        }

        if (!context.Materials.Any())
        {
            if (isRelational)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [Materials] ON;");
                    context.Materials.AddRange(SeedData.Materials);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [Materials] OFF;");
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            else
            {
                context.Materials.AddRange(SeedData.Materials);
                context.SaveChanges();
            }
        }
    }
}
