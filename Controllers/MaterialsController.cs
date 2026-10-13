using Microsoft.AspNetCore.Mvc;
using TH4.Models.Common;
using TH4.Models.Dtos;
using TH4.Services.Implementations;
using TH4.Services.Interfaces;

namespace TH4.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MaterialsController(
    IMaterialService materialService,
    MethodSyntaxMaterialService methodSyntaxService,
    QuerySyntaxMaterialService querySyntaxService) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách vật tư kèm phân trang, tìm kiếm từ khóa và lọc theo khoảng giá.
    /// Có thể chọn cú pháp LINQ bằng query 'syntax=method|query' hoặc header 'X-Syntax-Style'.
    /// </summary>
    /// <param name="filter">Các tham số phân trang (pageIndex, pageSize), từ khóa (keyword), khoảng giá (minPrice, maxPrice).</param>
    /// <returns>Danh sách vật tư đã phân trang kèm tổng số bản ghi và thông tin phân trang.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagingDto<MaterialDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<PagingDto<MaterialDto>>> GetAll([FromQuery] FilterMaterialDto filter)
    {
        var result = materialService.GetAll(filter);
        return Ok(ApiResponse<PagingDto<MaterialDto>>.SuccessResponse(result, "Retrieved materials successfully."));
    }

    /// <summary>
    /// Lấy thông tin chi tiết một vật tư theo ID bao gồm thông tin liên hệ nhà cung cấp.
    /// </summary>
    /// <param name="id">Mã ID của vật tư.</param>
    /// <returns>Chi tiết vật tư và nhà cung cấp.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MaterialDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public ActionResult<ApiResponse<MaterialDetailDto>> GetById(int id)
    {
        var result = materialService.GetById(id);
        return Ok(ApiResponse<MaterialDetailDto>.SuccessResponse(result, "Retrieved material details successfully."));
    }

    /// <summary>
    /// Lấy danh sách vật tư sử dụng phép INNER JOIN.
    /// Các vật tư chưa gán nhà cung cấp (SupplierId = null như ID 9, 10) sẽ bị loại bỏ khỏi kết quả.
    /// </summary>
    /// <returns>Danh sách vật tư có nhà cung cấp hợp lệ.</returns>
    [HttpGet("inner-join")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MaterialDto>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<IEnumerable<MaterialDto>>> GetInnerJoin()
    {
        var result = materialService.GetInnerJoinMaterials();
        return Ok(ApiResponse<IEnumerable<MaterialDto>>.SuccessResponse(result, "Retrieved inner join materials successfully."));
    }

    /// <summary>
    /// Lấy danh sách vật tư sử dụng phép LEFT OUTER JOIN.
    /// Giữ lại toàn bộ vật tư, nếu không có nhà cung cấp sẽ hiển thị "Chưa có nhà cung cấp".
    /// </summary>
    /// <returns>Toàn bộ danh sách vật tư.</returns>
    [HttpGet("left-join")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MaterialDto>>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<IEnumerable<MaterialDto>>> GetLeftJoin()
    {
        var result = materialService.GetLeftJoinMaterials();
        return Ok(ApiResponse<IEnumerable<MaterialDto>>.SuccessResponse(result, "Retrieved left join materials successfully."));
    }

    /// <summary>
    /// So sánh kết quả thực thi đồng thời giữa LINQ Method Syntax và Query Syntax.
    /// </summary>
    /// <param name="filter">Bộ lọc danh sách.</param>
    /// <returns>Kết quả từ cả 2 cách triển khai để đối chiếu tính nhất quán.</returns>
    [HttpGet("syntax-comparison")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> CompareSyntax([FromQuery] FilterMaterialDto filter)
    {
        var methodResult = methodSyntaxService.GetAll(filter);
        var queryResult = querySyntaxService.GetAll(filter);

        var comparison = new
        {
            MethodSyntax = methodResult,
            QuerySyntax = queryResult,
            IsIdenticalCount = methodResult.TotalItems == queryResult.TotalItems
        };

        return Ok(ApiResponse<object>.SuccessResponse(comparison, "Syntax comparison executed successfully."));
    }

    /// <summary>
    /// Tạo mới một vật tư.
    /// </summary>
    /// <param name="dto">Dữ liệu tạo mới vật tư.</param>
    /// <returns>Vật tư vừa được tạo kèm ID mới sinh.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MaterialDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResponse<MaterialDto>> Create([FromBody] CreateMaterialDto dto)
    {
        var result = materialService.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<MaterialDto>.SuccessResponse(result, "Material created successfully.", StatusCodes.Status201Created));
    }

    /// <summary>
    /// Cập nhật thông tin vật tư theo ID.
    /// </summary>
    /// <param name="id">Mã ID vật tư cần cập nhật.</param>
    /// <param name="dto">Dữ liệu cập nhật.</param>
    /// <returns>Dữ liệu vật tư sau khi cập nhật.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MaterialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public ActionResult<ApiResponse<MaterialDto>> Update(int id, [FromBody] UpdateMaterialDto dto)
    {
        var result = materialService.Update(id, dto);
        return Ok(ApiResponse<MaterialDto>.SuccessResponse(result, "Material updated successfully."));
    }

    /// <summary>
    /// Xóa một vật tư theo ID.
    /// </summary>
    /// <param name="id">Mã ID vật tư cần xóa.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        materialService.Delete(id);
        return NoContent();
    }
}
