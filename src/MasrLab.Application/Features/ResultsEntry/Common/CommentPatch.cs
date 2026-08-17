namespace MasrLab.Application.Features.ResultsEntry.Common;

public record CommentPatch
{
    public bool UpdateComment { get; init; }
    public string? NewComment { get; init; }
}
