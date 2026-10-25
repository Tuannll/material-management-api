using Microsoft.EntityFrameworkCore;
using TH4.Data;
using TH4.Exceptions;
using TH4.Models.Common;
using TH4.Models.Dtos;
using TH4.Models.Entities;
using TH4.Services.Implementations;
using Xunit;

namespace TH4.Tests;

/// <summary>
/// Unit tests cho MethodSyntaxMaterialService sử dụng EF Core InMemoryDatabase.
///
/// =========================================================================================
/// HƯỚNG DẪN DEMO PIPELINE FAIL (Minh chứng khi test fail thì pipeline CI/CD fail):
/// Trong mỗi test case bên dưới đều có ghi chú vị trí [DEMO FAIL].
/// Để tạo tình huống test fail cho bài thuyết trình / demo pipeline:
///   1. Mở một trong các test case bất kỳ (ví dụ: GetAll hoặc Create).
///   2. Đổi giá trị tại dòng được đánh dấu [DEMO FAIL] (ví dụ: đổi kết quả mong đợi từ 2 thành 999).
///   3. Commit và push nhánh lên GitHub.
///   4. GitHub Actions sẽ chạy 'dotnet test', phát hiện lỗi và đánh dấu Pipeline màu ĐỎ (Failed).
/// =========================================================================================
/// </summary>
public class MaterialServiceTests
{
    /// <summary>
    /// Khởi tạo DbContext với InMemory Database riêng biệt cho từng test case
    /// nhằm đảm bảo tính cô lập (test isolation), tránh việc các test can thiệp dữ liệu của nhau.
    /// </summary>
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Test case a: GetAll_ShouldReturnPagingDto_WhenDataExists
    /// Kiểm tra khi database đã có dữ liệu vật liệu và nhà cung cấp,
    /// phương thức GetAll trả về đúng PagingDto với số lượng bản ghi và thông tin mapper.
    /// </summary>
    [Fact]
    public void GetAll_ShouldReturnPagingDto_WhenDataExists()
    {
        // 1. Arrange (Chuẩn bị dữ liệu mẫu trong DbContext)
        using var context = CreateInMemoryDbContext();

        var supplier = new Supplier
        {
            Id = 1,
            Name = "Công ty Cổ phần Thép Hòa Phát",
            Address = "KCN Phố Nối A, Hưng Yên",
            ContactPhone = "02439876543"
        };
        context.Suppliers.Add(supplier);

        context.Materials.AddRange(
            new Material
            {
                Id = 1,
                Name = "Thép cuộn CB240-T phi 8",
                Unit = "Tấn",
                UnitPrice = 15200000m,
                SupplierId = 1
            },
            new Material
            {
                Id = 2,
                Name = "Xi măng Vicem Hoàng Thạch PCB40",
                Unit = "Bao",
                UnitPrice = 92000m,
                SupplierId = null // Không có nhà cung cấp liên kết
            }
        );
        context.SaveChanges();

        var service = new MethodSyntaxMaterialService(context);
        var filter = new FilterMaterialDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        // 2. Act (Thực thi phương thức nghiệp vụ)
        var result = service.GetAll(filter);

        // 3. Assert (Kiểm tra kết quả kỳ vọng)
        Assert.NotNull(result);

        // [DEMO FAIL]: Sửa số 2 thành số khác (ví dụ: 999) để test FAIL, minh chứng pipeline CI fail
        Assert.Equal(2, result.TotalItems);

        var itemsList = result.Items.ToList();
        Assert.Equal(2, itemsList.Count);

        // Kiểm tra vật liệu có nhà cung cấp liên kết
        var materialWithSupplier = itemsList.FirstOrDefault(m => m.Id == 1);
        Assert.NotNull(materialWithSupplier);
        Assert.Equal("Công ty Cổ phần Thép Hòa Phát", materialWithSupplier.SupplierName);
        Assert.Equal("KCN Phố Nối A, Hưng Yên", materialWithSupplier.SupplierAddress);

        // Kiểm tra vật liệu không có nhà cung cấp liên kết
        var materialWithoutSupplier = itemsList.FirstOrDefault(m => m.Id == 2);
        Assert.NotNull(materialWithoutSupplier);
        Assert.Equal("Chưa có nhà cung cấp", materialWithoutSupplier.SupplierName);
    }

    /// <summary>
    /// Test case b: GetById_ShouldReturnCorrectMaterial_WhenIdExists
    /// Kiểm tra khi Id tồn tại, trả về đúng thông tin chi tiết vật liệu kèm thông tin liên hệ NCC.
    /// </summary>
    [Fact]
    public void GetById_ShouldReturnCorrectMaterial_WhenIdExists()
    {
        // 1. Arrange
        using var context = CreateInMemoryDbContext();

        var supplier = new Supplier
        {
            Id = 10,
            Name = "Công ty Xi măng Nghi Sơn",
            Address = "Thanh Hóa",
            ContactPhone = "02373861234"
        };
        context.Suppliers.Add(supplier);

        var material = new Material
        {
            Id = 100,
            Name = "Xi măng Nghi Sơn PCB40",
            Unit = "Tấn",
            UnitPrice = 1450000m,
            SupplierId = 10
        };
        context.Materials.Add(material);
        context.SaveChanges();

        var service = new MethodSyntaxMaterialService(context);

        // 2. Act
        var result = service.GetById(100);

        // 3. Assert
        Assert.NotNull(result);

        // [DEMO FAIL]: Sửa 100 thành 999 hoặc sửa tên mong đợi để test FAIL
        Assert.Equal(100, result.Id);
        Assert.Equal("Xi măng Nghi Sơn PCB40", result.Name);
        Assert.Equal("Tấn", result.Unit);
        Assert.Equal(1450000m, result.UnitPrice);
        Assert.Equal("Công ty Xi măng Nghi Sơn", result.SupplierName);
        Assert.Equal("Thanh Hóa", result.SupplierAddress);
        Assert.Equal("02373861234", result.SupplierContactPhone);
    }

    /// <summary>
    /// Test case c: GetById_ShouldThrowNotFoundException_WhenIdDoesNotExist
    /// Kiểm tra khi tìm vật liệu với Id không tồn tại trong hệ thống,
    /// phương thức phải ném ra NotFoundException.
    /// </summary>
    [Fact]
    public void GetById_ShouldThrowNotFoundException_WhenIdDoesNotExist()
    {
        // 1. Arrange
        using var context = CreateInMemoryDbContext();
        var service = new MethodSyntaxMaterialService(context);
        const int nonExistentId = 99999;

        // 2. Act & Assert
        // [DEMO FAIL]: Đổi NotFoundException thành ArgumentException để test FAIL
        var exception = Assert.Throws<NotFoundException>(() => service.GetById(nonExistentId));
        Assert.Contains(nonExistentId.ToString(), exception.Message);
    }

    /// <summary>
    /// Test case d: Create_ShouldThrowUserFriendlyException_WhenMaterialNameIsDuplicate
    /// Kiểm tra khi tạo mới vật liệu có tên đã tồn tại (trùng lặp),
    /// phương thức phải chặn lại và ném UserFriendlyException với mã lỗi DuplicateMaterialName.
    /// </summary>
    [Fact]
    public void Create_ShouldThrowUserFriendlyException_WhenMaterialNameIsDuplicate()
    {
        // 1. Arrange
        using var context = CreateInMemoryDbContext();

        var existingMaterial = new Material
        {
            Id = 1,
            Name = "Cát vàng sông Lô",
            Unit = "M3",
            UnitPrice = 320000m
        };
        context.Materials.Add(existingMaterial);
        context.SaveChanges();

        var service = new MethodSyntaxMaterialService(context);

        var duplicateDto = new CreateMaterialDto
        {
            Name = "  Cát vàng sông Lô  ", // Kiểm tra cả trường hợp khoảng trắng thừa
            Unit = "Khối",
            UnitPrice = 350000m
        };

        // 2. Act & Assert
        var exception = Assert.Throws<UserFriendlyException>(() => service.Create(duplicateDto));

        // [DEMO FAIL]: Sửa ErrorCodes.DuplicateMaterialName thành ErrorCodes.InvalidInput để test FAIL
        Assert.Equal(ErrorCodes.DuplicateMaterialName, exception.ErrorCode);
    }

    /// <summary>
    /// Test case e: Create_ShouldAddNewMaterial_WhenValidDataProvided
    /// Kiểm tra khi cung cấp dữ liệu hợp lệ và nhà cung cấp tồn tại,
    /// phương thức thêm thành công vật liệu vào cơ sở dữ liệu và trả về DTO tương ứng.
    /// </summary>
    [Fact]
    public void Create_ShouldAddNewMaterial_WhenValidDataProvided()
    {
        // 1. Arrange
        using var context = CreateInMemoryDbContext();

        var supplier = new Supplier
        {
            Id = 5,
            Name = "Nhà máy Gạch Viglacera",
            Address = "Từ Sơn, Bắc Ninh",
            ContactPhone = "02223841555"
        };
        context.Suppliers.Add(supplier);
        context.SaveChanges();

        var service = new MethodSyntaxMaterialService(context);

        var createDto = new CreateMaterialDto
        {
            Name = "Gạch đặc đất sét nung A1",
            Unit = "Viên",
            UnitPrice = 1250m,
            SupplierId = 5
        };

        // 2. Act
        var result = service.Create(createDto);

        // 3. Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0, "Id sinh ra phải lớn hơn 0");

        // [DEMO FAIL]: Sửa tên kỳ vọng thành tên khác để test FAIL
        Assert.Equal("Gạch đặc đất sét nung A1", result.Name);
        Assert.Equal("Viên", result.Unit);
        Assert.Equal(1250m, result.UnitPrice);
        Assert.Equal("Nhà máy Gạch Viglacera", result.SupplierName);
        Assert.Equal("Từ Sơn, Bắc Ninh", result.SupplierAddress);

        // Kiểm tra trực tiếp trong DbContext xem bản ghi đã được lưu xuống chưa
        var savedEntity = context.Materials.FirstOrDefault(m => m.Id == result.Id);
        Assert.NotNull(savedEntity);
        Assert.Equal("Gạch đặc đất sét nung A1", savedEntity.Name);
        Assert.Equal(5, savedEntity.SupplierId);
    }
}
