using TH4.Models.Entities; // Nhập các lớp entity dùng cho dữ liệu mẫu trong bộ nhớ.

namespace TH4.Data // Khai báo namespace cho các thành phần hỗ trợ dữ liệu.
{
    /* Cung cấp dữ liệu mẫu tĩnh trong bộ nhớ cho nhà cung cấp và vật liệu. */
    public static class SeedData // Định nghĩa lớp tĩnh chứa các tập dữ liệu mẫu.
    {
        /* Mỗi lần truy cập sẽ trả về một danh sách nhà cung cấp mới. */
        public static List<Supplier> Suppliers => new List<Supplier> // Tạo danh sách nhà cung cấp dùng trong các truy vấn join.
        {
            new Supplier { Id = 1, Name = "Thép Hòa Phát", Address = "Hà Nội", ContactPhone = "0901000001" }, // Khai báo nhà cung cấp 1 với tên, địa chỉ và số điện thoại.
            new Supplier { Id = 2, Name = "Xi măng Xuân Thành", Address = "Hà Nam", ContactPhone = "0901000002" }, // Khai báo nhà cung cấp 2 với tên, địa chỉ và số điện thoại.
            new Supplier { Id = 3, Name = "Gạch Viglacera", Address = "Bắc Ninh", ContactPhone = "0901000003" }, // Khai báo nhà cung cấp 3 với tên, địa chỉ và số điện thoại.
            new Supplier { Id = 4, Name = "Cát sạch Sông Đà", Address = "Hòa Bình", ContactPhone = "0901000004" } // Khai báo nhà cung cấp 4 với tên, địa chỉ và số điện thoại.
        };

        /* Mỗi lần truy cập sẽ trả về một danh sách vật liệu mới. */
        public static List<Material> Materials => new List<Material> // Tạo danh sách vật liệu dùng trong toàn bộ tầng service.
        {
            new Material { Id = 1, Name = "Xi măng PC40", Unit = "Tấn", UnitPrice = 1500000, SupplierId = 2 }, // Khai báo vật liệu 1 và liên kết với nhà cung cấp 2.
            new Material { Id = 2, Name = "Thép cuộn D6", Unit = "Tấn", UnitPrice = 16500000, SupplierId = 1 }, // Khai báo vật liệu 2 và liên kết với nhà cung cấp 1.
            new Material { Id = 3, Name = "Thép thanh D18", Unit = "Tấn", UnitPrice = 17200000, SupplierId = 1 }, // Khai báo vật liệu 3 và liên kết với nhà cung cấp 1.
            new Material { Id = 4, Name = "Gạch đỏ đặc", Unit = "Viên", UnitPrice = 1200, SupplierId = 3 }, // Khai báo vật liệu 4 và liên kết với nhà cung cấp 3.
            new Material { Id = 5, Name = "Cát vàng", Unit = "Khối", UnitPrice = 350000, SupplierId = 4 }, // Khai báo vật liệu 5 và liên kết với nhà cung cấp 4.
            new Material { Id = 6, Name = "Đá 1x2", Unit = "Khối", UnitPrice = 420000, SupplierId = 4 }, // Khai báo vật liệu 6 và liên kết với nhà cung cấp 4.
            new Material { Id = 7, Name = "Gạch block", Unit = "Viên", UnitPrice = 9500, SupplierId = 3 }, // Khai báo vật liệu 7 và liên kết với nhà cung cấp 3.
            new Material { Id = 8, Name = "Xi măng PCB30", Unit = "Tấn", UnitPrice = 1400000, SupplierId = 2 }, // Khai báo vật liệu 8 và liên kết với nhà cung cấp 2.
            new Material { Id = 9, Name = "Ống nhựa PVC", Unit = "Mét", UnitPrice = 67000, SupplierId = null }, // Khai báo vật liệu 9 chưa có liên kết nhà cung cấp.
            new Material { Id = 10, Name = "Sơn chống thấm", Unit = "Thùng", UnitPrice = 890000, SupplierId = null } // Khai báo vật liệu 10 chưa có liên kết nhà cung cấp.
        };
    }
}
