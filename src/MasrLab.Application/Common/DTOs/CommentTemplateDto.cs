namespace MasrLab.Application.Common.DTOs;

public record CommentTemplateDto
{
    public int Id { get; init; }
    public int TestId { get; init; }
    public string Text { get; init; } = string.Empty;
}
