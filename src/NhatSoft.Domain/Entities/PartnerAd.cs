using NhatSoft.Domain.Entities.Base;
using NhatSoft.Domain.Enums;

namespace NhatSoft.Domain.Entities
{
    public class PartnerAd: AuditableEntity
    {
        // LƯU Ý QUAN TRỌNG: 
        // Nếu trong class BaseEntity của bạn ĐÃ CÓ SẴN thuộc tính `Id` và `CreatedAt` 
        // thì bạn HÃY XÓA 2 dòng dưới đây đi để tránh bị lỗi trùng lặp (hiding member) nhé.

        // public Guid Id { get; set; } 
        // public DateTime CreatedAt { get; set; } 

        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string TargetUrl { get; set; } = string.Empty;
        public AdPosition Position { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
