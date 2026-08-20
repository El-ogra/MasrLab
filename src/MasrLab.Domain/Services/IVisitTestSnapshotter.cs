using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Services;

public interface IVisitTestSnapshotter
{
    (VisitTest VisitTest, IReadOnlyList<VisitTestResultItem> ResultItems) CreateVisitTestSnapshot(
        Test test, int visitId, decimal price, bool isOutsourced);

    (VisitTest VisitTest, IReadOnlyList<VisitTestResultItem> ResultItems) CreateVisitTestSnapshot(
        Test test, int visitId, decimal price, bool isOutsourced,
        int? sourceTestGroupId, string? testGroupNameSnapshot);
}
