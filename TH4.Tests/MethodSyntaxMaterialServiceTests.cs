using Microsoft.EntityFrameworkCore;
using TH4.Data;
using TH4.Exceptions;
using TH4.Models.Dtos;
using TH4.Models.Entities;
using TH4.Services.Implementations;
using Xunit;

namespace TH4.Tests;

public class MethodSyntaxMaterialServiceTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void Create_ValidMaterial_ReturnsDtoAndPersists()
    {
        using var context = CreateInMemoryDbContext();
        var service = new MethodSyntaxMaterialService(context);
        var dto = new CreateMaterialDto
        {
            Name = "Thep Hoa Phat",
            Unit = "Tan",
            UnitPrice = 15000000,
            SupplierId = null
        };

        var result = service.Create(dto);

        Assert.NotNull(result);
        Assert.Equal("Thep Hoa Phat", result.Name);
        Assert.Single(context.Materials);
    }

    [Fact]
    public void Create_DuplicateName_ThrowsUserFriendlyException()
    {
        using var context = CreateInMemoryDbContext();
        context.Materials.Add(new Material { Name = "Xi Mang Ha Tien", Unit = "Bao", UnitPrice = 90000 });
        context.SaveChanges();

        var service = new MethodSyntaxMaterialService(context);
        var dto = new CreateMaterialDto
        {
            Name = "Xi Mang Ha Tien",
            Unit = "Bao",
            UnitPrice = 95000
        };

        Assert.Throws<UserFriendlyException>(() => service.Create(dto));
    }

    [Fact]
    public void GetById_NonExistingId_ThrowsNotFoundException()
    {
        using var context = CreateInMemoryDbContext();
        var service = new MethodSyntaxMaterialService(context);

        Assert.Throws<NotFoundException>(() => service.GetById(999));
    }

    [Fact]
    public void GetAll_FilterKeyword_ReturnsMatchingItems()
    {
        using var context = CreateInMemoryDbContext();
        context.Materials.AddRange(
            new Material { Name = "Cat vang", Unit = "m3", UnitPrice = 300000 },
            new Material { Name = "Da xanh", Unit = "m3", UnitPrice = 400000 }
        );
        context.SaveChanges();

        var service = new MethodSyntaxMaterialService(context);
        var filter = new FilterMaterialDto { Keyword = "Cat" };

        var result = service.GetAll(filter);

        Assert.Single(result.Items);
        Assert.Equal("Cat vang", result.Items.First().Name);
    }

    [Fact]
    public void GetInnerJoin_ExcludesMaterialsWithoutSupplier()
    {
        using var context = CreateInMemoryDbContext();
        var supplier = new Supplier { Id = 1, Name = "Nha Cung Cap A", Address = "HN", ContactPhone = "0123" };
        context.Suppliers.Add(supplier);
        context.Materials.AddRange(
            new Material { Name = "Gach", Unit = "Vien", UnitPrice = 2000, SupplierId = 1 },
            new Material { Name = "Son", Unit = "Thung", UnitPrice = 500000, SupplierId = null }
        );
        context.SaveChanges();

        var service = new MethodSyntaxMaterialService(context);
        var result = service.GetInnerJoinMaterials().ToList();

        Assert.Single(result);
        Assert.Equal("Gach", result[0].Name);
        Assert.Equal("Nha Cung Cap A", result[0].SupplierName);
    }
}
