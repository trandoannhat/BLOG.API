using Xunit;
using NhatSoft.Domain.Entities; 
using System;

namespace NhatSoft.UnitTests
{
    public class ProjectDomainTests
    {
        [Fact]
        public void CreateProject_Should_Initialize_Correctly()
        {
            // 1. Arrange (Chuẩn bị dữ liệu)
            var projectName = "Website NhatSoft";

            // 2. Act (Thực thi hành động)
            var project = new Project
            {
                Name = projectName,
                ClientName = "NhatSoft Internal"
            };

            // 3. Assert (Kiểm chứng kết quả)

            // Kiểm tra ID có được tự động sinh ra không (Khác Empty)
            Assert.NotEqual(Guid.Empty, project.Id);

            // Kiểm tra ngày tạo có hợp lý không (gần với giờ hiện tại)
            Assert.True(project.CreatedAt > DateTime.MinValue);

            // Kiểm tra các giá trị mặc định
            Assert.False(project.IsDeleted); // Mặc định chưa xóa
            Assert.Equal("[]", project.TechStackJson); // TechStack mặc định là mảng rỗng

            // Quan trọng: Kiểm tra List Images đã được new() chưa (tránh NullReferenceException sau này)
            Assert.NotNull(project.Images);
            Assert.Empty(project.Images);
        }
    }
}