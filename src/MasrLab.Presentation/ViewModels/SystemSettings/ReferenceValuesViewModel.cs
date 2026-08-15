using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Commands.AddReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Commands.DeleteReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValue;
using MasrLab.Application.Features.TestsMasterData.Queries.GetReferenceValuesByTestId;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestComponentsByTestId;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Exceptions;
using MediatR;

namespace MasrLab.Presentation.ViewModels.SystemSettings;

public enum RangeForMode
{
    ForAll,
    BySexAndAge,
    BySexOnly,
    ByAgeOnly
}

public partial class ReferenceValuesViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private int _testId;

    public ReferenceValuesViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string TestName { get; set; } = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ReferenceValueDto> _referenceValues = [];

    [ObservableProperty]
    private ReferenceValueDto? _selectedReferenceValue;

    [ObservableProperty]
    private ObservableCollection<TestComponentDto> _components = [];

    [ObservableProperty]
    private TestComponentDto? _selectedComponent;

    [ObservableProperty]
    private RangeForMode _selectedRangeForMode = RangeForMode.ForAll;

    [ObservableProperty]
    private ReferenceValueGender _selectedGender = ReferenceValueGender.Both;

    [ObservableProperty]
    private int _ageFrom;

    [ObservableProperty]
    private int _ageTo;

    [ObservableProperty]
    private AgeUnit _selectedAgeUnit = AgeUnit.Years;

    [ObservableProperty]
    private string _referenceRange = string.Empty;

    [ObservableProperty]
    private decimal? _lowLimit;

    [ObservableProperty]
    private decimal? _highLimit;

    [ObservableProperty]
    private string? _lowFlag;

    [ObservableProperty]
    private string? _highFlag;

    [ObservableProperty]
    private string? _testUnit;

    [ObservableProperty]
    private bool _forPregnantOnly;

    [ObservableProperty]
    private string? _lowComment;

    [ObservableProperty]
    private string? _highComment;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public IReadOnlyList<ReferenceValueGender> Genders { get; } =
        Enum.GetValues<ReferenceValueGender>();

    public IReadOnlyList<AgeUnit> AgeUnits { get; } =
        Enum.GetValues<AgeUnit>();

    public IReadOnlyList<RangeForMode> RangeForModes { get; } =
        Enum.GetValues<RangeForMode>();

    public bool IsGenderEnabled => SelectedRangeForMode is RangeForMode.BySexOnly or RangeForMode.BySexAndAge;
    public bool IsAgeEnabled => SelectedRangeForMode is RangeForMode.ByAgeOnly or RangeForMode.BySexAndAge;
    public bool IsNumericLimitsEnabled => SelectedComponent?.ResultEntryKind != ResultEntryKind.CultureDetail;

    partial void OnSelectedRangeForModeChanged(RangeForMode value)
    {
        OnPropertyChanged(nameof(IsGenderEnabled));
        OnPropertyChanged(nameof(IsAgeEnabled));

        if (!IsGenderEnabled)
            SelectedGender = ReferenceValueGender.Both;

        if (!IsAgeEnabled)
        {
            AgeFrom = 0;
            AgeTo = 0;
            SelectedAgeUnit = AgeUnit.Years;
        }
    }

    partial void OnSelectedReferenceValueChanged(ReferenceValueDto? value)
    {
        if (value is not null)
        {
            IsEditing = true;
            SelectedGender = value.Gender;
            AgeFrom = value.AgeMin;
            AgeTo = value.AgeMax;
            SelectedAgeUnit = value.AgeUnit;
            ReferenceRange = value.NormalRange;
            LowLimit = value.LowLimit;
            HighLimit = value.HighLimit;
            LowFlag = value.LowFlag;
            HighFlag = value.HighFlag;
            TestUnit = value.TestUnit;
            ForPregnantOnly = value.ForPregnantOnly;
            LowComment = value.LowComment;
            HighComment = value.HighComment;

            SelectedComponent = Components.FirstOrDefault(c => c.Id == value.TestComponentId);

            if (value.Gender == ReferenceValueGender.Both && value.AgeMin == 0 && value.AgeMax == 0)
                SelectedRangeForMode = RangeForMode.ForAll;
            else if (value.Gender != ReferenceValueGender.Both && value.AgeMin == 0 && value.AgeMax == 0)
                SelectedRangeForMode = RangeForMode.BySexOnly;
            else if (value.Gender == ReferenceValueGender.Both && (value.AgeMin != 0 || value.AgeMax != 0))
                SelectedRangeForMode = RangeForMode.ByAgeOnly;
            else
                SelectedRangeForMode = RangeForMode.BySexAndAge;
        }
        else
        {
            IsEditing = false;
        }
    }

    partial void OnSelectedComponentChanged(TestComponentDto? value)
    {
        OnPropertyChanged(nameof(IsNumericLimitsEnabled));
    }

    [RelayCommand]
    private async Task LoadReferenceValuesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _mediator.Send(new GetReferenceValuesByTestIdQuery(_testId));
            ReferenceValues = new ObservableCollection<ReferenceValueDto>(result);

            var compResult = await _mediator.Send(new GetTestComponentsByTestIdQuery(_testId));
            Components = new ObservableCollection<TestComponentDto>(compResult);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في تحميل القيم المرجعية: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddNew()
    {
        SelectedReferenceValue = null;
        ClearForm();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var gender = IsGenderEnabled ? SelectedGender : ReferenceValueGender.Both;
            var ageMin = IsAgeEnabled ? AgeFrom : 0;
            var ageMax = IsAgeEnabled ? AgeTo : 0;
            var ageUnit = IsAgeEnabled ? SelectedAgeUnit : AgeUnit.Years;

            var command = new AddReferenceValueCommand(
                _testId, SelectedComponent?.Id, gender, ageMin, ageMax, ageUnit,
                ReferenceRange, LowLimit, HighLimit, TestUnit,
                LowFlag, HighFlag, ForPregnantOnly, HighComment, LowComment);

            await _mediator.Send(command);
            await LoadReferenceValuesAsync();
            ClearForm();
        }
        catch (BusinessRuleViolationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في حفظ القيمة المرجعية: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateAsync()
    {
        if (SelectedReferenceValue is null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var gender = IsGenderEnabled ? SelectedGender : ReferenceValueGender.Both;
            var ageMin = IsAgeEnabled ? AgeFrom : 0;
            var ageMax = IsAgeEnabled ? AgeTo : 0;
            var ageUnit = IsAgeEnabled ? SelectedAgeUnit : AgeUnit.Years;

            var command = new UpdateReferenceValueCommand(
                SelectedReferenceValue.Id, _testId, SelectedComponent?.Id,
                gender, ageMin, ageMax, ageUnit,
                ReferenceRange, LowLimit, HighLimit, TestUnit,
                LowFlag, HighFlag, ForPregnantOnly, HighComment, LowComment);

            await _mediator.Send(command);
            await LoadReferenceValuesAsync();
            ClearForm();
        }
        catch (BusinessRuleViolationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في تعديل القيمة المرجعية: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanUpdate => IsEditing && SelectedReferenceValue is not null;

    [RelayCommand]
    private void DiscardChanges()
    {
        if (IsEditing && SelectedReferenceValue is not null)
        {
            SelectedGender = SelectedReferenceValue.Gender;
            AgeFrom = SelectedReferenceValue.AgeMin;
            AgeTo = SelectedReferenceValue.AgeMax;
            SelectedAgeUnit = SelectedReferenceValue.AgeUnit;
            ReferenceRange = SelectedReferenceValue.NormalRange;
            LowLimit = SelectedReferenceValue.LowLimit;
            HighLimit = SelectedReferenceValue.HighLimit;
            LowFlag = SelectedReferenceValue.LowFlag;
            HighFlag = SelectedReferenceValue.HighFlag;
            TestUnit = SelectedReferenceValue.TestUnit;
            ForPregnantOnly = SelectedReferenceValue.ForPregnantOnly;
            LowComment = SelectedReferenceValue.LowComment;
            HighComment = SelectedReferenceValue.HighComment;
        }
        else
        {
            ClearForm();
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedReferenceValue is null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            await _mediator.Send(new DeleteReferenceValueCommand(SelectedReferenceValue.Id));
            await LoadReferenceValuesAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطأ في حذف القيمة المرجعية: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanDelete => IsEditing && SelectedReferenceValue is not null;

    [RelayCommand]
    private void Close(System.Windows.Window? window)
    {
        window?.Close();
    }

    public void Initialize(int testId, string testName)
    {
        _testId = testId;
        TestName = testName;
    }

    private void ClearForm()
    {
        SelectedReferenceValue = null;
        IsEditing = false;
        SelectedRangeForMode = RangeForMode.ForAll;
        SelectedGender = ReferenceValueGender.Both;
        AgeFrom = 0;
        AgeTo = 0;
        SelectedAgeUnit = AgeUnit.Years;
        ReferenceRange = string.Empty;
        LowLimit = null;
        HighLimit = null;
        LowFlag = null;
        HighFlag = null;
        TestUnit = null;
        ForPregnantOnly = false;
        LowComment = null;
        HighComment = null;
        SelectedComponent = null;
        ErrorMessage = string.Empty;
    }
}
