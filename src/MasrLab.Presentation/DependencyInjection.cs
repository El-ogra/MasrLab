using MasrLab.Presentation.Printing;
using MasrLab.Presentation.ViewModels;
using MasrLab.Presentation.ViewModels.Accounting;
using MasrLab.Presentation.ViewModels.AttendanceAndAudit;
using MasrLab.Presentation.ViewModels.CasesFollowUp;
using MasrLab.Presentation.ViewModels.Cultures;
using MasrLab.Presentation.ViewModels.DoctorsAndReferrals;
using MasrLab.Presentation.ViewModels.FixedComments;
using MasrLab.Presentation.ViewModels.OutsourcedSamples;
using MasrLab.Presentation.ViewModels.PatientHistory;
using MasrLab.Presentation.ViewModels.PatientManagement;
using MasrLab.Presentation.ViewModels.PatientSearch;
using MasrLab.Presentation.ViewModels.PriceLists;
using MasrLab.Presentation.ViewModels.ResultsEntry;
using MasrLab.Presentation.ViewModels.SampleCollection;
using MasrLab.Presentation.ViewModels.Statistics;
using MasrLab.Presentation.ViewModels.SystemSettings;
using MasrLab.Presentation.ViewModels.TestGroups;
using MasrLab.Presentation.ViewModels.TestsMasterData;
using MasrLab.Presentation.ViewModels.UsersAndPermissions;
using MasrLab.Presentation.ViewModels.WorkSheets;
using MasrLab.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<EnvelopePrinter>();

        services.AddTransient<LoginWindow>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<FirstRunSetupWindow>();
        services.AddTransient<FirstRunSetupViewModel>();
        services.AddTransient<AccountTypeDrawerViewModel>();
        services.AddTransient<DoctorReferralDrawerViewModel>();
        services.AddTransient<PeriodDrawerViewModel>();
        services.AddTransient<AttendanceAuditViewModel>();
        services.AddTransient<CasesFollowUpViewModel>();
        services.AddTransient<AddAntibioticViewModel>();
        services.AddTransient<AddCultureViewModel>();
        services.AddTransient<CultureResultViewModel>();
        services.AddTransient<DoctorsReferralsViewModel>();
        services.AddTransient<FixedCommentsViewModel>();
        services.AddTransient<OutsourcedSamplesViewModel>();
        services.AddTransient<PatientHistoryViewModel>();
        services.AddTransient<DeliverResultsViewModel>();
        services.AddTransient<RegisterPatientViewModel>();
        services.AddTransient<UpdatePatientAccountViewModel>();
        services.AddTransient<UpdatePatientDataViewModel>();
        services.AddTransient<SearchPatientsViewModel>();
        services.AddTransient<VisitHistoryViewModel>();
        services.AddTransient<PriceListsViewModel>();
        services.AddTransient<BlankReportViewModel>();
        services.AddTransient<CombinedReportViewModel>();
        services.AddTransient<EnterResultsViewModel>();
        services.AddTransient<SampleCollectionViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<SystemSettingsViewModel>();
        services.AddTransient<TestGroupsViewModel>();
        services.AddTransient<TestsMasterDataViewModel>();
        services.AddTransient<UsersPermissionsViewModel>();
        services.AddTransient<WorkSheetsViewModel>();

        return services;
    }
}
