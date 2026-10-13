namespace TH4.Models.Dtos; // Khai báo namespace cho các DTO dùng chung.

/* Biểu diễn kết quả danh sách đã phân trang kèm đầy đủ metadata. */
public class PagingDto<T> // DTO generic dùng lại cho nhiều loại dữ liệu.
{
    public IEnumerable<T> Items { get; set; } = new List<T>(); // Lưu các bản ghi của trang hiện tại.

    public int TotalItems { get; set; } // Lưu tổng số bản ghi sau khi áp dụng filter, trước khi phân trang.

    public int PageIndex { get; set; } = 1; // Lưu trang hiện tại.

    public int PageSize { get; set; } = 10; // Lưu số bản ghi mỗi trang.

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0; // Tính tổng số trang.

    public bool HasPreviousPage => PageIndex > 1; // Kiểm tra có trang trước đó không.

    public bool HasNextPage => PageIndex < TotalPages; // Kiểm tra có trang kế tiếp không.

    public PagingDto() { }

    public PagingDto(IEnumerable<T> items, int totalItems, int pageIndex, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}
