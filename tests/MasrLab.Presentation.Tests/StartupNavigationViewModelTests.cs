using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Models;
using MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;
using MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using MasrLab.Application.Features.UsersAndPermissions.Queries.GetRegisteredUsernames;
using MasrLab.Presentation.ViewModels;
using MasrLab.Presentation.ViewModels.PatientManagement;
using MasrLab.Presentation.ViewModels.PatientSearch;
using MasrLab.Presentation.ViewModels.ResultsEntry;
using MasrLab.Presentation.ViewModels.SystemSettings;
using MasrLab.Presentation.Views.PatientManagement;
using MasrLab.Presentation.Views.PatientSearch;
using MasrLab.Presentation.Views.ResultsEntry;
using MasrLab.Presentation.Views.SystemSettings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MasrLab.Presentation.Tests;

public class StartupNavigationViewModelTests
{
    [Fact]
    public void LoginViewModel_Constructor_LoadsRegisteredUsernames()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetRegisteredUsernamesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "admin", "mona" });

        var vm = new LoginViewModel(new Mock<IAuthenticationService>().Object, mediator.Object);

        Assert.Equal(2, vm.Usernames.Count);
        Assert.Contains("admin", vm.Usernames);
        Assert.Contains("mona", vm.Usernames);
    }

    [Fact]
    public async Task LoginViewModel_Login_WithEmptyFields_ShowsValidationError()
    {
        var vm = new LoginViewModel(new Mock<IAuthenticationService>().Object, CreateMediator().Object);

        vm.Username = "";
        vm.Password = "";
        await ExecuteAsync(vm.LoginCommand);

        Assert.Equal("يرجى إدخال اسم المستخدم وكلمة المرور", vm.ErrorMessage);
    }

    [Fact]
    public async Task LoginViewModel_Login_WithWrongCredentials_ShowsFailureReason()
    {
        var authService = new Mock<IAuthenticationService>();
        authService.Setup(x => x.LoginAsync("admin", "wrong", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.Failure("اسم المستخدم أو كلمة المرور غير صحيحة"));
        var vm = new LoginViewModel(authService.Object, CreateMediator().Object);

        vm.Username = "admin";
        vm.Password = "wrong";
        await ExecuteAsync(vm.LoginCommand);

        Assert.Equal("اسم المستخدم أو كلمة المرور غير صحيحة", vm.ErrorMessage);
    }

    [Fact]
    public async Task LoginViewModel_Login_WithValidCredentials_LogsInAndRecordsAttendance()
    {
        var authService = new Mock<IAuthenticationService>();
        authService.Setup(x => x.LoginAsync("admin", "pw", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.Success(1, "admin", new List<string> { "Admin" }));
        var mediator = CreateMediator();
        mediator.Setup(x => x.Send(It.IsAny<RecordLoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        var vm = new LoginViewModel(authService.Object, mediator.Object);
        vm.Username = "admin";
        vm.Password = "pw";
        await ExecuteAsync(vm.LoginCommand);

        authService.Verify(x => x.LoginAsync("admin", "pw", It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(x => x.Send(It.IsAny<RecordLoginCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    [Fact]
    public async Task FirstRunSetupViewModel_Save_WithEmptyFields_ShowsValidationError()
    {
        var vm = new FirstRunSetupViewModel(new Mock<IMediator>().Object, new Mock<IAuthenticationService>().Object);

        await ExecuteAsync(vm.SaveCommand);

        Assert.Equal("يرجى إدخال اسم المستخدم وكلمة المرور", vm.ErrorMessage);
    }

    [Fact]
    public async Task FirstRunSetupViewModel_Save_WithMismatchedPasswords_ShowsError()
    {
        var vm = new FirstRunSetupViewModel(new Mock<IMediator>().Object, new Mock<IAuthenticationService>().Object);
        vm.AdminUsername = "admin";
        vm.AdminPassword = "pw1";
        vm.AdminPasswordConfirm = "pw2";

        await ExecuteAsync(vm.SaveCommand);

        Assert.Equal("كلمتا المرور غير متطابقتين", vm.ErrorMessage);
    }

    [Fact]
    public async Task FirstRunSetupViewModel_Save_CreatesAdminUser_LogsInAndRecordsAttendance()
    {
        var authService = new Mock<IAuthenticationService>();
        authService.Setup(x => x.LoginAsync("admin", "pw", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult.Success(1, "admin", new List<string> { "Admin" }));
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        mediator.Setup(x => x.Send(It.IsAny<RecordLoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        var vm = new FirstRunSetupViewModel(mediator.Object, authService.Object);
        vm.AdminUsername = "admin";
        vm.AdminPassword = "pw";
        vm.AdminPasswordConfirm = "pw";

        await ExecuteAsync(vm.SaveCommand);

        mediator.Verify(x => x.Send(
            It.Is<CreateUserCommand>(c => c.Username == "admin" && c.IsAdmin),
            It.IsAny<CancellationToken>()), Times.Once);
        authService.Verify(x => x.LoginAsync("admin", "pw", It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(x => x.Send(It.IsAny<RecordLoginCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    [Fact]
    public void MainViewModel_SelectPatients_ShowsPatientsMenu()
    {
        var (vm, _) = CreateMainViewModel();
        var raised = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsPatientsSelected))
                raised = true;
        };

        vm.SelectPatientsCommand.Execute(null);

        Assert.True(vm.IsPatientsSelected);
        Assert.True(raised);
    }

    [Fact]
    public void MainViewModel_ShowRegisterPatient_OpensRegisterPatientWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowRegisterPatientCommand.Execute(null);

        Assert.Equal(typeof(RegisterPatientView), testVm.LastRequestedWindow);
        Assert.False(vm.IsPatientsSelected);
    }

    [Fact]
    public void MainViewModel_ShowEnterResults_OpensEnterResultsWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowEnterResultsCommand.Execute(null);

        Assert.Equal(typeof(EnterResultsView), testVm.LastRequestedWindow);
        Assert.False(vm.IsPatientsSelected);
    }

    [Fact]
    public void MainViewModel_ShowSearchPatients_OpensSearchPatientsWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowSearchPatientsCommand.Execute(null);

        Assert.Equal(typeof(SearchPatientsView), testVm.LastRequestedWindow);
        Assert.False(vm.IsPatientsSelected);
    }

    [Fact]
    public void MainViewModel_ShowDeliverResults_OpensDeliverResultsWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowDeliverResultsCommand.Execute(null);

        Assert.Equal(typeof(DeliverResultsView), testVm.LastRequestedWindow);
        Assert.False(vm.IsPatientsSelected);
    }

    [Fact]
    public void MainViewModel_SelectSystem_ShowsSystemMenu()
    {
        var (vm, _) = CreateMainViewModel();
        var raised = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsSystemSelected))
                raised = true;
        };

        vm.SelectSystemCommand.Execute(null);

        Assert.True(vm.IsSystemSelected);
        Assert.False(vm.IsPatientsSelected);
        Assert.True(raised);
    }

    [Fact]
    public void MainViewModel_SelectSystem_HidesPatientsMenu()
    {
        var (vm, _) = CreateMainViewModel();
        vm.SelectPatientsCommand.Execute(null);
        Assert.True(vm.IsPatientsSelected);

        vm.SelectSystemCommand.Execute(null);

        Assert.True(vm.IsSystemSelected);
        Assert.False(vm.IsPatientsSelected);
    }

    [Fact]
    public void MainViewModel_ShowTestsMasterData_OpensTestsMasterDataWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowTestsMasterDataCommand.Execute(null);

        Assert.Equal(typeof(TestsMasterDataWindow), testVm.LastRequestedWindow);
    }

    [Fact]
    public void MainViewModel_ShowBarcodeTypes_OpensBarcodeTypesWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowBarcodeTypesCommand.Execute(null);

        Assert.Equal(typeof(BarcodeTypesWindow), testVm.LastRequestedWindow);
    }

    [Fact]
    public void MainViewModel_ShowCultureAntibiotics_OpensCultureAntibioticsWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowCultureAntibioticsCommand.Execute(null);

        Assert.Equal(typeof(CultureAntibioticsWindow), testVm.LastRequestedWindow);
    }

    [Fact]
    public void MainViewModel_ShowExternalLabs_OpensExternalLabsWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowExternalLabsCommand.Execute(null);

        Assert.Equal(typeof(ExternalLabsWindow), testVm.LastRequestedWindow);
    }

    [Fact]
    public void MainViewModel_ShowTestGroups_OpensTestGroupsWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowTestGroupsCommand.Execute(null);

        Assert.Equal(typeof(TestGroupsWindow), testVm.LastRequestedWindow);
    }

    [Fact]
    public void MainViewModel_ShowTestUnits_OpensTestUnitsWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowTestUnitsCommand.Execute(null);

        Assert.Equal(typeof(TestUnitsWindow), testVm.LastRequestedWindow);
    }

    [Fact]
    public void MainViewModel_ShowTestComments_OpensTestCommentsWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowTestCommentsCommand.Execute(null);

        Assert.Equal(typeof(TestCommentsWindow), testVm.LastRequestedWindow);
    }

    [Fact]
    public void MainViewModel_ShowPatientTitles_OpensPatientTitlesWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowPatientTitlesCommand.Execute(null);

        Assert.Equal(typeof(PatientTitlesWindow), testVm.LastRequestedWindow);
    }

    [Fact]
    public void MainViewModel_ShowPriceListPrint_OpensPriceListPrintWindow()
    {
        var (vm, _) = CreateMainViewModel();
        var testVm = (TestMainViewModel)vm;

        vm.ShowPriceListPrintCommand.Execute(null);

        Assert.Equal(typeof(PriceListPrintWindow), testVm.LastRequestedWindow);
    }

    private static Mock<IMediator> CreateMediator()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetRegisteredUsernamesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());
        return mediator;
    }

    private static async Task ExecuteAsync(IAsyncRelayCommand command)
    {
        command.Execute(null);
        await command.ExecutionTask!;
    }

    private static (MainViewModel, ServiceProvider) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddTransient<RegisterPatientViewModel>();
        services.AddTransient<EnterResultsViewModel>();
        services.AddTransient<SearchPatientsViewModel>();
        services.AddTransient<DeliverResultsViewModel>();
        services.AddTransient<TestsMasterDataViewModel>();
        services.AddTransient<BarcodeTypesViewModel>();
        services.AddTransient<CultureAntibioticsViewModel>();
        services.AddTransient<ExternalLabsViewModel>();
        services.AddTransient<TestGroupsViewModel>();
        services.AddTransient<TestUnitsViewModel>();
        services.AddTransient<TestCommentsViewModel>();
        services.AddTransient<PatientTitlesViewModel>();
        services.AddTransient<PriceListPrintViewModel>();
        var provider = services.BuildServiceProvider();
        return (new TestMainViewModel(provider), provider);
    }

    private class TestMainViewModel : MainViewModel
    {
        public TestMainViewModel(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Type? LastRequestedWindow { get; private set; }

        protected override void OpenWindow<TWindow, TViewModel>()
        {
            LastRequestedWindow = typeof(TWindow);
        }
    }
}
