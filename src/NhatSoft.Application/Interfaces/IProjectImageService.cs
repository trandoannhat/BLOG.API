namespace NhatSoft.Application.Interfaces
{
    public interface IProjectImageService
    {
        Task DeleteImageAsync(Guid id);
        Task SetThumbnailAsync(Guid id);
    }
}
