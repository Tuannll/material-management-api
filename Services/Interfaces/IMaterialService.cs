using TH4.Models.Dtos;
using TH4.Models.Entities;

namespace TH4.Services.Interfaces;

public interface IMaterialService
{
    string ImplementationName { get; }
    PagingDto<MaterialDto> GetAll(FilterMaterialDto filter);
    MaterialDetailDto GetById(int id);
    IEnumerable<MaterialDto> GetInnerJoinMaterials();
    IEnumerable<MaterialDto> GetLeftJoinMaterials();
    MaterialDto Create(CreateMaterialDto dto);
    MaterialDto Update(int id, UpdateMaterialDto dto);
    void Delete(int id);
}
