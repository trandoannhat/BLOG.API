using Microsoft.EntityFrameworkCore;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions;
using NhatSoft.Domain.Interfaces;

namespace NhatSoft.Application.Services
{
    public class ProjectImageService(IUnitOfWork unitOfWork) : IProjectImageService
    {
        public async Task DeleteImageAsync(Guid id)
        {
            var img = await unitOfWork.ProjectImages.GetByIdAsync(id);
            if (img == null) throw new NotFoundException("Ảnh không tồn tại");

            // Logic xóa file vật lý trên ổ cứng nếu cần (Optional)
            // FileHelper.DeleteFile(img.ImageUrl);

            unitOfWork.ProjectImages.Delete(img);
            await unitOfWork.CompleteAsync();
        }

        public async Task SetThumbnailAsync(Guid id)
        {
            var img = await unitOfWork.ProjectImages.GetByIdAsync(id);
            if (img == null) throw new NotFoundException("Ảnh không tồn tại");

            // 1. Lấy tất cả ảnh của Project này
            var allImages = await unitOfWork.ProjectImages.GetAllQueryable()
                .Where(x => x.ProjectId == img.ProjectId)
                .ToListAsync();

            // 2. Reset tất cả IsThumbnail = false
            foreach (var item in allImages)
            {
                item.IsThumbnail = false;
            }

            // 3. Set ảnh hiện tại = true
            img.IsThumbnail = true;

            // EF Core tự track changes, chỉ cần Save
            unitOfWork.ProjectImages.Update(img);
            await unitOfWork.CompleteAsync();
        }
    }
}
