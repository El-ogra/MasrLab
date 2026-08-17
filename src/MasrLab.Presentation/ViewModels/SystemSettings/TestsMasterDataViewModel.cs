using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Application.Features.TestsMasterData.Commands.AddTestComponent;
using MasrLab.Application.Features.TestsMasterData.Commands.DeleteTest;
using MasrLab.Application.Features.TestsMasterData.Commands.DeleteTestComponent;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTestComponent;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestComponentsByTestId;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Presentation.ViewModels.SystemSettings;

public partial class TestsMasterDataViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly IRepository<ExternalLab> _externalLabRepository;
    private readonly IServiceProvider _serviceProvider;

    public TestsMasterDataViewModel(IMediator mediator, IRepository<ExternalLab> externalLabRepository, IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _externalLabRepository = externalLabRepository;
        _serviceProvider = serviceProvider;
    }

    [ObservableProperty]
    private ObservableCollection<TestDto> _tests = [];

    [ObservableProperty]
    private TestDto? _selectedTest;

    [ObservableProperty]
    private int _totalTestsCount;

    [ObservableProperty]
    private string? _searchByName;

    [ObservableProperty]
    private string? _searchByGroup;

    [ObservableProperty]
    private int? _searchById;

    [ObservableProperty]
    private string _testName = string.Empty;

    [ObservableProperty]
    private string _testCode = string.Empty;

    [ObservableProperty]
    private string _reportName = string.Empty;

    [ObservableProperty]
    private string _eillName = string.Empty;

    [ObservableProperty]
    private string _historyName = string.Empty;

    [ObservableProperty]
    private string _arabicName = string.Empty;

    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private string _branch = string.Empty;

    [ObservableProperty]
    private string _logGroup = string.Empty;

    [ObservableProperty]
    private string _collection = string.Empty;

    [ObservableProperty]
    private bool _seeReport;

    [ObservableProperty]
    private bool _printWithOther;

    [ObservableProperty]
    private bool _addToGroup;

    [ObservableProperty]
    private bool _isMainTest;

    [ObservableProperty]
    private int _testTimeDays;

    [ObservableProperty]
    private int _arrangeNo;

    [ObservableProperty]
    private ReferenceType _referenceType;

    [ObservableProperty]
    private decimal _patientPrice;

    [ObservableProperty]
    private decimal _labToLabPrice;

    [ObservableProperty]
    private string _barcodeName = string.Empty;

    [ObservableProperty]
    private string _tube1 = string.Empty;

    [ObservableProperty]
    private string _tube2 = string.Empty;

    [ObservableProperty]
    private string _tube3 = string.Empty;

    [ObservableProperty]
    private bool _sentOutsideLab;

    [ObservableProperty]
    private string _outsourcedLabName = string.Empty;

    [ObservableProperty]
    private decimal _outsourcedCostPrice;

    [ObservableProperty]
    private decimal? _costPrice;

    [ObservableProperty]
    private string _patientQuestion = string.Empty;

    [ObservableProperty]
    private string _turnaroundTime = string.Empty;

    [ObservableProperty]
    private string _unit = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TestComponentDto> _components = [];

    [ObservableProperty]
    private TestComponentDto? _selectedComponent;

    [ObservableProperty]
    private string _componentName = string.Empty;

    [ObservableProperty]
    private string _componentUnit = string.Empty;

    [ObservableProperty]
    private int _componentDisplayOrder = 1;

    [ObservableProperty]
    private ResultEntryKind _componentResultEntryKind = ResultEntryKind.Ordinary;

    [ObservableProperty]
    private bool _isEditingComponent;

    public ObservableCollection<string> GroupNames { get; } = [];

    public ObservableCollection<string> ExternalLabNames { get; } = [];

    public IReadOnlyList<ReferenceType> ReferenceTypes { get; } =
        Enum.GetValues<ReferenceType>();

    public IReadOnlyList<ResultEntryKind> ResultEntryKinds { get; } =
        Enum.GetValues<ResultEntryKind>();

    public bool IsEditing => SelectedTest is not null;

    partial void OnSelectedTestChanged(TestDto? value)
    {
        if (value is not null)
        {
            TestName = value.Name;
            TestCode = value.TestCode ?? string.Empty;
            ReportName = value.ReportName;
            EillName = value.ReceiptName;
            HistoryName = value.HistoryName ?? string.Empty;
            ArabicName = value.ArabicName ?? string.Empty;
            GroupName = value.Group;
            Branch = value.Branch ?? string.Empty;
            LogGroup = value.LogGroup ?? string.Empty;
            Collection = value.SampleType ?? string.Empty;
            SeeReport = value.SeeReport;
            PrintWithOther = value.PrintWithOther;
            AddToGroup = value.AddWithGroup;
            IsMainTest = value.IsMainTest;
            TestTimeDays = value.TestTimeDays;
            ArrangeNo = value.ArrangeNo;
            ReferenceType = value.ReferenceType;
            PatientPrice = value.Price;
            LabToLabPrice = value.LabToLabPrice ?? 0;
            BarcodeName = value.BarcodeName ?? string.Empty;
            Tube1 = value.Tube1 ?? string.Empty;
            Tube2 = value.Tube2 ?? string.Empty;
            Tube3 = value.Tube3 ?? string.Empty;
            SentOutsideLab = value.SentOutsideLab;
            OutsourcedLabName = value.OutsourcedLabName ?? string.Empty;
            OutsourcedCostPrice = value.OutsourcedCostPrice ?? 0;
            CostPrice = value.CostPrice;
            PatientQuestion = value.PatientQuestion ?? string.Empty;
            TurnaroundTime = value.TurnaroundTime;
            Unit = value.Unit;
            Components = new ObservableCollection<TestComponentDto>(value.Components);
        }
        else
        {
            Components = [];
        }
        OnPropertyChanged(nameof(IsEditing));
    }

    partial void OnSelectedComponentChanged(TestComponentDto? value)
    {
        if (value is not null)
        {
            IsEditingComponent = true;
            ComponentName = value.Name;
            ComponentUnit = value.Unit;
            ComponentDisplayOrder = value.DisplayOrder;
            ComponentResultEntryKind = value.ResultEntryKind;
        }
        else
        {
            IsEditingComponent = false;
        }
    }

    [RelayCommand]
    private async Task LoadTestsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _mediator.Send(new GetTestsListQuery(SearchByName, SearchByGroup, SearchById));
            Tests = new ObservableCollection<TestDto>(result);
            TotalTestsCount = result.Count;

            GroupNames.Clear();
            foreach (var group in result.Select(t => t.Group).Distinct().OrderBy(g => g))
                GroupNames.Add(group);

            await LoadExternalLabNamesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في تحميل البيانات: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadTestsAsync();
    }

    [RelayCommand]
    private void AddNewTest()
    {
        SelectedTest = null;
        ClearForm();
    }

    [RelayCommand]
    private async Task SaveTestAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var command = new AddTestCommand(
                TestName, ReportName, EillName, GroupName,
                string.IsNullOrWhiteSpace(TestCode) ? null : TestCode,
                PatientPrice, TurnaroundTime, false, Unit,
                string.IsNullOrWhiteSpace(TestCode) ? null : TestCode,
                string.IsNullOrWhiteSpace(HistoryName) ? null : HistoryName,
                string.IsNullOrWhiteSpace(ArabicName) ? null : ArabicName,
                string.IsNullOrWhiteSpace(Branch) ? null : Branch,
                string.IsNullOrWhiteSpace(LogGroup) ? null : LogGroup,
                string.IsNullOrWhiteSpace(Collection) ? null : Collection,
                SeeReport, PrintWithOther, AddToGroup, IsMainTest,
                TestTimeDays, ArrangeNo, ReferenceType,
                LabToLabPrice > 0 ? LabToLabPrice : null,
                string.IsNullOrWhiteSpace(BarcodeName) ? null : BarcodeName,
                string.IsNullOrWhiteSpace(Tube1) ? null : Tube1,
                string.IsNullOrWhiteSpace(Tube2) ? null : Tube2,
                string.IsNullOrWhiteSpace(Tube3) ? null : Tube3,
                SentOutsideLab,
                SentOutsideLab && !string.IsNullOrWhiteSpace(OutsourcedLabName) ? OutsourcedLabName : null,
                SentOutsideLab && OutsourcedCostPrice > 0 ? OutsourcedCostPrice : null,
                string.IsNullOrWhiteSpace(PatientQuestion) ? null : PatientQuestion,
                CostPrice
            );

            await _mediator.Send(command);
            await LoadTestsAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في حفظ التحليل: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateTestAsync()
    {
        if (SelectedTest is null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var command = new UpdateTestCommand(
                SelectedTest.Id,
                TestName, ReportName, EillName, GroupName,
                string.IsNullOrWhiteSpace(TestCode) ? null : TestCode,
                PatientPrice, TurnaroundTime, false, Unit,
                string.IsNullOrWhiteSpace(TestCode) ? null : TestCode,
                string.IsNullOrWhiteSpace(HistoryName) ? null : HistoryName,
                string.IsNullOrWhiteSpace(ArabicName) ? null : ArabicName,
                string.IsNullOrWhiteSpace(Branch) ? null : Branch,
                string.IsNullOrWhiteSpace(LogGroup) ? null : LogGroup,
                string.IsNullOrWhiteSpace(Collection) ? null : Collection,
                SeeReport, PrintWithOther, AddToGroup, IsMainTest,
                TestTimeDays, ArrangeNo, ReferenceType,
                LabToLabPrice > 0 ? LabToLabPrice : null,
                string.IsNullOrWhiteSpace(BarcodeName) ? null : BarcodeName,
                string.IsNullOrWhiteSpace(Tube1) ? null : Tube1,
                string.IsNullOrWhiteSpace(Tube2) ? null : Tube2,
                string.IsNullOrWhiteSpace(Tube3) ? null : Tube3,
                SentOutsideLab,
                SentOutsideLab && !string.IsNullOrWhiteSpace(OutsourcedLabName) ? OutsourcedLabName : null,
                SentOutsideLab && OutsourcedCostPrice > 0 ? OutsourcedCostPrice : null,
                string.IsNullOrWhiteSpace(PatientQuestion) ? null : PatientQuestion,
                CostPrice
            );

            await _mediator.Send(command);
            await LoadTestsAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في تعديل التحليل: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteTestAsync()
    {
        if (SelectedTest is null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            await _mediator.Send(new DeleteTestCommand(SelectedTest.Id));
            await LoadTestsAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في حذف التحليل: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanDelete => SelectedTest is not null;

    [RelayCommand]
    private async Task TransferAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var grouped = Tests
                .GroupBy(t => t.Group)
                .OrderBy(g => g.Key);

            int counter = 1;
            foreach (var group in grouped)
            {
                var ordered = group.OrderBy(t => t.ArrangeNo).ToList();
                foreach (var test in ordered)
                {
                    if (test.ArrangeNo != counter)
                    {
                        var command = new UpdateTestCommand(
                            test.Id, test.Name, test.ReportName, test.ReceiptName, test.Group,
                            test.Barcode, test.Price, test.TurnaroundTime, test.LabToLabFlag, test.Unit,
                            test.TestCode, test.HistoryName, test.ArabicName, test.Branch, test.LogGroup,
                            test.SampleType, test.SeeReport, test.PrintWithOther, test.AddWithGroup,
                            test.IsMainTest, test.TestTimeDays, counter, test.ReferenceType,
                            test.LabToLabPrice, test.BarcodeName, test.Tube1, test.Tube2, test.Tube3,
                            test.SentOutsideLab, test.OutsourcedLabName, test.OutsourcedCostPrice,
                            test.PatientQuestion, test.CostPrice);
                        await _mediator.Send(command);
                    }
                    counter++;
                }
            }

            await LoadTestsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في ترحيل البيانات: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenReferenceValues()
    {
        if (SelectedTest is null) return;

        var viewModel = _serviceProvider.GetRequiredService<ReferenceValuesViewModel>();
        viewModel.Initialize(SelectedTest.Id, SelectedTest.Name);

        var window = new Views.SystemSettings.ReferenceValuesWindow
        {
            DataContext = viewModel
        };

        window.Loaded += async (_, _) => await viewModel.LoadReferenceValuesCommand.ExecuteAsync(null);
        window.ShowDialog();
    }

    [RelayCommand]
    private async Task AddComponentAsync()
    {
        if (SelectedTest is null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            await _mediator.Send(new AddTestComponentCommand(
                SelectedTest.Id, ComponentName, ComponentUnit,
                ComponentDisplayOrder, ComponentResultEntryKind));

            await LoadComponentsAsync(SelectedTest.Id);
            ClearComponentForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في إضافة المكون: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsEditingComponent))]
    private async Task UpdateComponentAsync()
    {
        if (SelectedTest is null || SelectedComponent is null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            await _mediator.Send(new UpdateTestComponentCommand(
                SelectedComponent.Id, SelectedTest.Id, ComponentName, ComponentUnit,
                ComponentDisplayOrder, ComponentResultEntryKind));

            await LoadComponentsAsync(SelectedTest.Id);
            ClearComponentForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في تعديل المكون: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsEditingComponent))]
    private async Task DeleteComponentAsync()
    {
        if (SelectedTest is null || SelectedComponent is null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            await _mediator.Send(new DeleteTestComponentCommand(SelectedComponent.Id, SelectedTest.Id));

            await LoadComponentsAsync(SelectedTest.Id);
            ClearComponentForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في حذف المكون: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearComponentForm()
    {
        SelectedComponent = null;
        ComponentName = string.Empty;
        ComponentUnit = string.Empty;
        ComponentDisplayOrder = Components.Count + 1;
        ComponentResultEntryKind = ResultEntryKind.Ordinary;
        IsEditingComponent = false;
    }

    private async Task LoadComponentsAsync(int testId)
    {
        var result = await _mediator.Send(new GetTestComponentsByTestIdQuery(testId));
        Components = new ObservableCollection<TestComponentDto>(result);
        ComponentDisplayOrder = Components.Count + 1;
    }

    [RelayCommand]
    private void Close(System.Windows.Window? window)
    {
        window?.Close();
    }

    private void ClearForm()
    {
        TestName = string.Empty;
        TestCode = string.Empty;
        ReportName = string.Empty;
        EillName = string.Empty;
        HistoryName = string.Empty;
        ArabicName = string.Empty;
        GroupName = string.Empty;
        Branch = string.Empty;
        LogGroup = string.Empty;
        Collection = string.Empty;
        SeeReport = false;
        PrintWithOther = false;
        AddToGroup = false;
        IsMainTest = false;
        TestTimeDays = 0;
        ArrangeNo = 0;
        ReferenceType = ReferenceType.General;
        PatientPrice = 0;
        LabToLabPrice = 0;
        BarcodeName = string.Empty;
        Tube1 = string.Empty;
        Tube2 = string.Empty;
        Tube3 = string.Empty;
        SentOutsideLab = false;
        OutsourcedLabName = string.Empty;
        OutsourcedCostPrice = 0;
        CostPrice = null;
        PatientQuestion = string.Empty;
        TurnaroundTime = string.Empty;
        Unit = string.Empty;
        Components = [];
        ClearComponentForm();
    }

    private async Task LoadExternalLabNamesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var labs = await _externalLabRepository.GetAllAsync(cancellationToken);
            ExternalLabNames.Clear();
            foreach (var lab in labs.Select(l => l.Name).OrderBy(n => n))
                ExternalLabNames.Add(lab);
        }
        catch
        {
            ExternalLabNames.Clear();
        }
    }
}
