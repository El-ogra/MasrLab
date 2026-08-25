using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.Constants;

// Named capabilities mapped onto the existing Screen/Operation permission model
// (no new permission infrastructure — plan Risk #6).
public static class PermissionNames
{
    // OQ-M2-8: full financial administration over the patient billing screens.
    public const string BillingAdmin = "BillingAdmin";

    // OQ-M4-15: editing result values after a report has been printed (Slice 6).
    public const string ResultEdit = "ResultEdit";

    public static readonly IReadOnlyList<(ScreenType ScreenId, PermissionOperation OperationId)> BillingAdminOperations =
        new (ScreenType, PermissionOperation)[]
        {
            (ScreenType.Receipts, PermissionOperation.Edit),
            (ScreenType.Receipts, PermissionOperation.Delete),
            (ScreenType.Accounts, PermissionOperation.Edit),
            (ScreenType.Accounts, PermissionOperation.Delete)
        };

    public static readonly IReadOnlyList<(ScreenType ScreenId, PermissionOperation OperationId)> ResultEditOperations =
        new (ScreenType, PermissionOperation)[]
        {
            (ScreenType.Results, PermissionOperation.EditPrinted)
        };
}
