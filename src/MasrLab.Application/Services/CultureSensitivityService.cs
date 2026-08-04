using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// خدمة حساسية الزرعات — تسجيل الحساسية وملخصاتها.
/// INV: Culture.RecordSensitivity() يفرض وجود عضو واحد على الأقل وحالة Recorded (Culture.cs:45-48).
/// </summary>
public class CultureSensitivityService : ICultureSensitivityService
{
    private readonly ICultureRepository _cultures;
    private readonly IUnitOfWork _unitOfWork;

    public CultureSensitivityService(ICultureRepository cultures, IUnitOfWork unitOfWork)
    {
        _cultures = cultures ?? throw new ArgumentNullException(nameof(cultures));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// يسجل حساسية لزراعة معينة.
    /// INV: يستدعي culture.RecordSensitivity() — لا يُعيّن Sensitivities مباشرة.
    /// INV:Culture.Status يجب أن يكون Recorded، ويجب وجود عضو واحد على الأقل.
    /// </summary>
    public void RecordSensitivity(int cultureId, int antibioticId, int sensitivityLevel)
    {
        var culture = _cultures.GetByIdAsync(cultureId).GetAwaiter().GetResult();
        if (culture is null)
            return;

        var level = (SensitivityLevel)sensitivityLevel;
        culture.RecordSensitivity(antibioticId, level);

        _cultures.Update(culture);
        _unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// يُرجع ملخصاً نصياً لحساسية الزراعة.
    /// INV: قراءة فقط — لا يُعدّل أي حالة.
    /// </summary>
    public string GetSensitivitySummary(int cultureId)
    {
        var culture = _cultures.GetByIdAsync(cultureId).GetAwaiter().GetResult();
        if (culture is null)
            return string.Empty;

        var organism = !string.IsNullOrEmpty(culture.OrganismA) ? culture.OrganismA
            : !string.IsNullOrEmpty(culture.OrganismB) ? culture.OrganismB
            : culture.OrganismC ?? "Unknown";

        var sensitivities = culture.Sensitivities
            .Select(s => $"{s.AntibioticId}({s.SensitivityLevel})")
            .ToList();

        return $"Organism: {organism}; Antibiotics: {string.Join(", ", sensitivities)}";
    }

    /// <summary>
    /// النسخة غير المتزامنة من RecordSensitivity.
    /// INV: يستدعي culture.RecordSensitivity() — لا يُعيّن Sensitivities مباشرة.
    /// </summary>
    public async Task RecordSensitivityAsync(int cultureId, int antibioticId, int sensitivityLevel, CancellationToken ct = default)
    {
        var culture = await _cultures.GetByIdAsync(cultureId);
        if (culture is null)
            return;

        var level = (SensitivityLevel)sensitivityLevel;
        culture.RecordSensitivity(antibioticId, level);

        _cultures.Update(culture);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
