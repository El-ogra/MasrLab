using MasrLab.Domain.Entities.Core;

namespace MasrLab.Application.Tests;

internal static class TestVisitTestHelpers
{
    internal static VisitTest CreateVisitTest(int visitId, int testId, decimal price, bool isOutsourced = false)
        => new(visitId, testId, price, isOutsourced);
}
