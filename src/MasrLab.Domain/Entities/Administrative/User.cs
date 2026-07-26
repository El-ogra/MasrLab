using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Administrative;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
}
