using Microsoft.EntityFrameworkCore;
using TH4.Data;
using TH4.Exceptions;
using TH4.Models.Common;
using TH4.Models.Dtos;
using TH4.Models.Entities;
using TH4.Services.Interfaces;

namespace TH4.Services.Implementations;

public class QuerySyntaxMaterialService : IMaterialService
{
    private readonly ApplicationDbContext _dbContext;

    public QuerySyntaxMaterialService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string ImplementationName => "Query Syntax Style";

    public PagingDto<MaterialDto> GetAll(FilterMaterialDto filter)
    {
        var query =
            from material in _dbContext.Materials.AsNoTracking()
            join supplier in _dbContext.Suppliers.AsNoTracking()
                on material.SupplierId equals supplier.Id into supplierGroup
            from supplier in supplierGroup.DefaultIfEmpty()
            select new MaterialDto
            {
                Id = material.Id,
                Name = material.Name,
                Unit = material.Unit,
                UnitPrice = material.UnitPrice,
                SupplierName = supplier != null ? supplier.Name : "Chưa có nhà cung cấp",
                SupplierAddress = supplier != null ? supplier.Address : null,
                SourceImplementation = ImplementationName
            };

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query =
                from m in query
                where m.Name.Contains(keyword)
                      || m.Unit.Contains(keyword)
                      || m.SupplierName.Contains(keyword)
                select m;
        }

        if (filter.MinPrice.HasValue)
        {
            query =
                from m in query
                where m.UnitPrice >= filter.MinPrice.Value
                select m;
        }

        if (filter.MaxPrice.HasValue)
        {
            query =
                from m in query
                where m.UnitPrice <= filter.MaxPrice.Value
                select m;
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
        var materialDetail =
            (from material in _dbContext.Materials.AsNoTracking()
             where material.Id == id
             join supplier in _dbContext.Suppliers.AsNoTracking()
                 on material.SupplierId equals supplier.Id into supplierGroup
             from supplier in supplierGroup.DefaultIfEmpty()
             select new MaterialDetailDto
             {
                 Id = material.Id,
                 Name = material.Name,
                 Unit = material.Unit,
                 UnitPrice = material.UnitPrice,
                 SupplierName = supplier != null ? supplier.Name : "Chưa có nhà cung cấp",
                 SupplierAddress = supplier != null ? supplier.Address : null,
                 SupplierContactPhone = supplier != null ? supplier.ContactPhone : null,
                 SourceImplementation = ImplementationName
             }).FirstOrDefault();

        return materialDetail ?? throw new NotFoundException($"Material with id {id} was not found.");
    }

    public IEnumerable<MaterialDto> GetInnerJoinMaterials()
    {
        var query =
            from material in _dbContext.Materials.AsNoTracking()
            join supplier in _dbContext.Suppliers.AsNoTracking()
                on material.SupplierId equals supplier.Id
            select new MaterialDto
            {
                Id = material.Id,
                Name = material.Name,
                Unit = material.Unit,
                UnitPrice = material.UnitPrice,
                SupplierName = supplier.Name,
                SupplierAddress = supplier.Address,
                SourceImplementation = ImplementationName
            };

        return query.ToList();
    }

    public IEnumerable<MaterialDto> GetLeftJoinMaterials()
    {
        var query =
            from material in _dbContext.Materials.AsNoTracking()
            join supplier in _dbContext.Suppliers.AsNoTracking()
                on material.SupplierId equals supplier.Id into supplierGroup
            from supplier in supplierGroup.DefaultIfEmpty()
            select new MaterialDto
            {
                Id = material.Id,
                Name = material.Name,
                Unit = material.Unit,
                UnitPrice = material.UnitPrice,
                SupplierName = supplier != null ? supplier.Name : "Chưa có nhà cung cấp",
                SupplierAddress = supplier != null ? supplier.Address : null,
                SourceImplementation = ImplementationName
            };

        return query.ToList();
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
