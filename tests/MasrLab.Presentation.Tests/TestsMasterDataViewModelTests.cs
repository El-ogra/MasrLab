using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using MasrLab.Presentation.ViewModels.SystemSettings;
using MediatR;
using Moq;

namespace MasrLab.Presentation.Tests;

public class TestsMasterDataViewModelTests
{
    [Fact]
    public async Task SaveTest_sends_entered_TurnaroundTime_and_Unit()
    {
        var captured = new CapturedCommands();
        var mediator = CreateMediator(captured);
        var vm = CreateViewModel(mediator);

        vm.TestName = "CBC";
        vm.ReportName = "CBC Report";
        vm.EillName = "CBC Receipt";
        vm.GroupName = "Hematology";
        vm.PatientPrice = 120m;
        vm.TurnaroundTime = "24h";
        vm.Unit = "mg/dL";

        await ExecuteAsync(vm.SaveTestCommand);

        Assert.NotNull(captured.Add);
        Assert.Equal("24h", captured.Add!.TurnaroundTime);
        Assert.Equal("mg/dL", captured.Add.Unit);
        Assert.NotEqual(string.Empty, captured.Add.TurnaroundTime);
        Assert.NotEqual(string.Empty, captured.Add.Unit);
    }

    [Fact]
    public async Task UpdateTest_sends_entered_TurnaroundTime_and_Unit()
    {
        var captured = new CapturedCommands();
        var mediator = CreateMediator(captured);
        var vm = CreateViewModel(mediator);

        vm.SelectedTest = new TestDto
        {
            Id = 7,
            Name = "CBC",
            ReportName = "CBC Report",
            ReceiptName = "CBC Receipt",
            Group = "Hematology",
            Price = 120m,
            TurnaroundTime = "24h",
            Unit = "mg/dL"
        };
        vm.TurnaroundTime = "48h";
        vm.Unit = "mg/L";

        await ExecuteAsync(vm.UpdateTestCommand);

        Assert.NotNull(captured.Update);
        Assert.Equal(7, captured.Update!.Id);
        Assert.Equal("48h", captured.Update.TurnaroundTime);
        Assert.Equal("mg/L", captured.Update.Unit);
        Assert.NotEqual(string.Empty, captured.Update.TurnaroundTime);
        Assert.NotEqual(string.Empty, captured.Update.Unit);
    }

    [Fact]
    public async Task UpdateTest_preserves_existing_TurnaroundTime_and_Unit_when_not_changed()
    {
        var captured = new CapturedCommands();
        var mediator = CreateMediator(captured);
        var vm = CreateViewModel(mediator);

        vm.SelectedTest = new TestDto
        {
            Id = 9,
            Name = "CRP",
            ReportName = "CRP Report",
            ReceiptName = "CRP Receipt",
            Group = "Chemistry",
            Price = 220m,
            TurnaroundTime = "48h",
            Unit = "mg/L"
        };

        await ExecuteAsync(vm.UpdateTestCommand);

        Assert.NotNull(captured.Update);
        Assert.Equal("48h", captured.Update!.TurnaroundTime);
        Assert.Equal("mg/L", captured.Update.Unit);
    }

    [Fact]
    public async Task SaveTest_sends_supplied_CostPrice()
    {
        var captured = new CapturedCommands();
        var mediator = CreateMediator(captured);
        var vm = CreateViewModel(mediator);

        vm.TestName = "CBC";
        vm.ReportName = "CBC Report";
        vm.EillName = "CBC Receipt";
        vm.GroupName = "Hematology";
        vm.PatientPrice = 120m;
        vm.TurnaroundTime = "24h";
        vm.Unit = "mg/dL";
        vm.CostPrice = 30.50m;

        await ExecuteAsync(vm.SaveTestCommand);

        Assert.NotNull(captured.Add);
        Assert.Equal(30.50m, captured.Add!.CostPrice);
    }

    [Fact]
    public async Task SaveTest_sends_null_CostPrice_when_not_set()
    {
        var captured = new CapturedCommands();
        var mediator = CreateMediator(captured);
        var vm = CreateViewModel(mediator);

        vm.TestName = "CBC";
        vm.ReportName = "CBC Report";
        vm.EillName = "CBC Receipt";
        vm.GroupName = "Hematology";
        vm.PatientPrice = 120m;
        vm.TurnaroundTime = "24h";
        vm.Unit = "mg/dL";

        await ExecuteAsync(vm.SaveTestCommand);

        Assert.NotNull(captured.Add);
        Assert.Null(captured.Add!.CostPrice);
    }

    [Fact]
    public async Task UpdateTest_sends_supplied_CostPrice()
    {
        var captured = new CapturedCommands();
        var mediator = CreateMediator(captured);
        var vm = CreateViewModel(mediator);

        vm.SelectedTest = new TestDto
        {
            Id = 7,
            Name = "CBC",
            ReportName = "CBC Report",
            ReceiptName = "CBC Receipt",
            Group = "Hematology",
            Price = 120m,
            TurnaroundTime = "24h",
            Unit = "mg/dL"
        };
        vm.CostPrice = 45m;

        await ExecuteAsync(vm.UpdateTestCommand);

        Assert.NotNull(captured.Update);
        Assert.Equal(45m, captured.Update!.CostPrice);
    }

    [Fact]
    public void Selection_preserves_null_CostPrice()
    {
        var captured = new CapturedCommands();
        var mediator = CreateMediator(captured);
        var vm = CreateViewModel(mediator);

        vm.SelectedTest = new TestDto
        {
            Id = 11,
            Name = "CBC",
            ReportName = "CBC Report",
            ReceiptName = "CBC Receipt",
            Group = "Hematology",
            Price = 100m,
            TurnaroundTime = "24h",
            Unit = "mg/dL",
            CostPrice = null
        };

        Assert.Null(vm.CostPrice);
    }

    private sealed class CapturedCommands
    {
        public AddTestCommand? Add { get; set; }
        public UpdateTestCommand? Update { get; set; }
    }

    private static TestsMasterDataViewModel CreateViewModel(Mock<IMediator> mediator)
    {
        var externalLabs = new Mock<IRepository<ExternalLab>>();
        externalLabs.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ExternalLab>());

        return new TestsMasterDataViewModel(mediator.Object, externalLabs.Object, new Mock<IServiceProvider>().Object);
    }

    private static Mock<IMediator> CreateMediator(CapturedCommands captured)
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<AddTestCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Unit>, CancellationToken>((request, _) => captured.Add = (AddTestCommand)request)
            .ReturnsAsync(Unit.Value);
        mediator.Setup(x => x.Send(It.IsAny<UpdateTestCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Unit>, CancellationToken>((request, _) => captured.Update = (UpdateTestCommand)request)
            .ReturnsAsync(Unit.Value);
        mediator.Setup(x => x.Send(It.IsAny<GetTestsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestDto>());

        return mediator;
    }

    private static async Task ExecuteAsync(IAsyncRelayCommand command)
    {
        command.Execute(null);
        await command.ExecutionTask!;
    }
}
