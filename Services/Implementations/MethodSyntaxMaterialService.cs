using Microsoft.EntityFrameworkCore;
using TH4.Data;
using TH4.Exceptions;
using TH4.Models.Common;
using TH4.Models.Dtos;
using TH4.Models.Entities;
using TH4.Services.Interfaces;

namespace TH4.Services.Implementations;

public class MethodSyntaxMaterialService : IMaterialService
{
    private readonly ApplicationDbContext _dbContext;

    public MethodSyntaxMaterialService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string ImplementationName => "Method Syntax Style";

    public PagingDto<MaterialDto> GetAll(FilterMaterialDto filter)
    {
        var query = _dbContext.Materials.AsNoTracking()
            .GroupJoin(
                _dbContext.Suppliers.AsNoTracking(),
                m => m.SupplierId,
                s => s.Id,
                (m, sGroup) => new { m, sGroup })
            .SelectMany(
                x => x.sGroup.DefaultIfEmpty(),
                (x, s) => new MaterialDto
                {
                    Id = x.m.Id,
                    Name = x.m.Name,
                    Unit = x.m.Unit,
                    UnitPrice = x.m.UnitPrice,
                    SupplierName = s != null ? s.Name : "Chưa có nhà cung cấp",
                    SupplierAddress = s != null ? s.Address : null,
                    SourceImplementation = ImplementationName
                });

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(material =>
                material.Name.Contains(keyword)
                || material.Unit.Contains(keyword)
                || material.SupplierName.Contains(keyword));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(material => material.UnitPrice >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(material => material.UnitPrice <= filter.MaxPrice.Value);
        }

        var totalItems = query.Count();
        var items = query
            .Skip(filter.GetSkipCount())
            .Take(filter.GetSafePageSize())
            .ToList();

        return new PagingDto<MaterialDto>(items, totalItems, filter.GetSafePageIndex(), filter.GetSafePageSize());
    }

    public MaterialDetailDto GetById(int id)
    {
        var materialDetail = _dbContext.Materials.AsNoTracking()
            .Where(m => m.Id == id)
            .GroupJoin(
                _dbContext.Suppliers.AsNoTracking(),
                m => m.SupplierId,
                s => s.Id,
                (m, sGroup) => new { m, sGroup })
            .SelectMany(
                x => x.sGroup.DefaultIfEmpty(),
                (x, s) => new MaterialDetailDto
                {
                    Id = x.m.Id,
                    Name = x.m.Name,
                    Unit = x.m.Unit,
                    UnitPrice = x.m.UnitPrice,
                    SupplierName = s != null ? s.Name : "Chưa có nhà cung cấp",
                    SupplierAddress = s != null ? s.Address : null,
                    SupplierContactPhone = s != null ? s.ContactPhone : null,
                    SourceImplementation = ImplementationName
                })
            .FirstOrDefault();

        return materialDetail ?? throw new NotFoundException($"Material with id {id} was not found.");
    }

    public IEnumerable<MaterialDto> GetInnerJoinMaterials()
    {
        return _dbContext.Materials.AsNoTracking()
            .Join(
                _dbContext.Suppliers.AsNoTracking(),
                m => m.SupplierId,
                s => s.Id,
                (m, s) => new MaterialDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Unit = m.Unit,
                    UnitPrice = m.UnitPrice,
                    SupplierName = s.Name,
                    SupplierAddress = s.Address,
                    SourceImplementation = ImplementationName
                })
            .ToList();
    }

    public IEnumerable<MaterialDto> GetLeftJoinMaterials()
    {
        return _dbContext.Materials.AsNoTracking()
            .GroupJoin(
                _dbContext.Suppliers.AsNoTracking(),
                m => m.SupplierId,
                s => s.Id,
                (m, sGroup) => new { m, sGroup })
            .SelectMany(
                x => x.sGroup.DefaultIfEmpty(),
                (x, s) => new MaterialDto
                {
                    Id = x.m.Id,
                    Name = x.m.Name,
                    Unit = x.m.Unit,
                    UnitPrice = x.m.UnitPrice,
                    SupplierName = s != null ? s.Name : "Chưa có nhà cung cấp",
                    SupplierAddress = s != null ? s.Address : null,
                    SourceImplementation = ImplementationName
                })
            .ToList();
    }

    public MaterialDto Create(CreateMaterialDto dto)
    {
        if (_dbContext.Materials.Any(m => m.Name == dto.Name.Trim()))
        {
            throw new UserFriendlyException(ErrorCodes.DuplicateMaterialName, "Tên vật liệu đã tồn tại trong hệ thống.");
        }

        if (dto.SupplierId.HasValue && !_dbContext.Suppliers.Any(s => s.Id == dto.SupplierId.Value))
        {
            throw new UserFriendlyException(ErrorCodes.SupplierNotFound, "Nhà cung cấp không tồn tại.");
        }

        var material = new Material
        {
            Name = dto.Name.Trim(),
            Unit = dto.Unit.Trim(),
            UnitPrice = dto.UnitPrice,
            SupplierId = dto.SupplierId
        };

        _dbContext.Materials.Add(material);
        _dbContext.SaveChanges();

        string supplierName = "Chưa có nhà cung cấp";
        string? supplierAddress = null;
        if (material.SupplierId.HasValue)
        {
            var supplier = _dbContext.Suppliers.Find(material.SupplierId.Value);
            if (supplier != null)
            {
                supplierName = supplier.Name;
                supplierAddress = supplier.Address;
            }
        }

        return new MaterialDto
        {
            Id = material.Id,
            Name = material.Name,
            Unit = material.Unit,
            UnitPrice = material.UnitPrice,
            SupplierName = supplierName,
            SupplierAddress = supplierAddress,
            SourceImplementation = ImplementationName
        };
    }

    public MaterialDto Update(int id, UpdateMaterialDto dto)
    {
        var material = _dbContext.Materials.Find(id);
        if (material == null)
        {
            throw new NotFoundException($"Material with id {id} was not found.");
        }

        if (_dbContext.Materials.Any(m => m.Name == dto.Name.Trim() && m.Id != id))
        {
            throw new UserFriendlyException(ErrorCodes.DuplicateMaterialName, "Tên vật liệu đã tồn tại trong hệ thống.");
        }

        if (dto.SupplierId.HasValue && !_dbContext.Suppliers.Any(s => s.Id == dto.SupplierId.Value))
        {
            throw new UserFriendlyException(ErrorCodes.SupplierNotFound, "Nhà cung cấp không tồn tại.");
        }

        material.Name = dto.Name.Trim();
        material.Unit = dto.Unit.Trim();
        material.UnitPrice = dto.UnitPrice;
        material.SupplierId = dto.SupplierId;

        _dbContext.SaveChanges();

        string supplierName = "Chưa có nhà cung cấp";
        string? supplierAddress = null;
        if (material.SupplierId.HasValue)
        {
            var supplier = _dbContext.Suppliers.Find(material.SupplierId.Value);
            if (supplier != null)
            {
                supplierName = supplier.Name;
                supplierAddress = supplier.Address;
            }
        }

        return new MaterialDto
        {
            Id = material.Id,
            Name = material.Name,
            Unit = material.Unit,
            UnitPrice = material.UnitPrice,
            SupplierName = supplierName,
            SupplierAddress = supplierAddress,
            SourceImplementation = ImplementationName
        };
    }

    public void Delete(int id)
    {
        var material = _dbContext.Materials.Find(id);
        if (material == null)
        {
            throw new NotFoundException($"Material with id {id} not found.");
        }

        _dbContext.Materials.Remove(material);
        _dbContext.SaveChanges();
    }
}
