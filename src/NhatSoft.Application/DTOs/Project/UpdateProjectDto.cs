namespace NhatSoft.Application.DTOs.Project;

public class UpdateProjectDto : CreateProjectDto
{
    // Kế thừa lại các trường của CreateProjectDto (Name, Description...)
    // Có thể thêm trường Id nếu muốn chắc chắn
    public Guid Id { get; set; }
}