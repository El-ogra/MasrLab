namespace MasrLab.Domain.Common.Enums;

// Cash/Insurance/Contract feed drawer accounting; Individual/LabToLab/VIP/Free are the
// registration-time result-worklist categories (OQ-M4-10). Values are stable ints.
public enum AccountType
{
    Cash,
    Insurance,
    Contract,
    Individual,
    LabToLab,
    VIP,
    Free
}
