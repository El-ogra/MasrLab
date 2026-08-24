namespace MasrLab.Domain.Services;

public static class CultureAntibioticVisibility
{
    public static bool IsVisible(
        bool pregnantFlag,
        bool childrenFlag,
        bool patientIsPregnant,
        int patientAgeYears)
    {
        if (!pregnantFlag && !childrenFlag)
            return true;

        var pregnancyVisible = pregnantFlag && patientIsPregnant;
        var childrenVisible = childrenFlag && patientAgeYears < 12;
        return pregnancyVisible || childrenVisible;
    }
}
