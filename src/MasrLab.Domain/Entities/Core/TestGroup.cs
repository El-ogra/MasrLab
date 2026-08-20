using System.ComponentModel.DataAnnotations.Schema;
using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class TestGroup : BaseEntity
{
    public string GroupName { get; set; } = string.Empty;

    [NotMapped]
    public decimal TotalGroupPrice =>
        TestGroupItems.Where(i => !i.IsDeleted).Sum(i => i.Price);

    public ICollection<TestGroupItem> TestGroupItems { get; set; } = new List<TestGroupItem>();
}
