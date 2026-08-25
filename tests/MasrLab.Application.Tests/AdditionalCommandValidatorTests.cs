using FluentValidation;

namespace MasrLab.Application.Tests;

public class AdditionalCommandValidatorTests
{
    public static IEnumerable<object[]> Cases()
    {
        var day = new DateTime(2026, 8, 9);
        yield return Case("CreateAccountTypeDrawer", new object?[] { day, day.AddDays(1), 0 }, new object?[] { day, day, 0 });
        yield return Case("CreateDoctorDrawer", new object?[] { day, day.AddDays(1), 1 }, new object?[] { day, day, 0 });
        yield return Case("CreatePeriodDrawer", new object?[] { day, day.AddDays(1) }, new object?[] { day, day });
        yield return Case("RecordCashTransaction", new object?[] { 0, 10m, 1, 1, day }, new object?[] { 0, 0m, 0, 0, day });
        yield return Case("RecordBreak", new object?[] { 1, null }, new object?[] { 0, null });
        yield return Case("RecordLogin", new object?[] { 1, day }, new object?[] { 0, day });
        yield return Case("RecordLogout", new object?[] { 1, 1, day }, new object?[] { 0, 0, day });
        yield return Case("AddCaseFollowUp", new object?[] { 1, "review" }, new object?[] { 0, "" });
        yield return Case("AddAntibioticToCulture", new object?[] { 1, 1, 0 }, new object?[] { 0, 0, 0 });
        yield return Case("AddNewCulture", new object?[] { 1, "blood", null, null, null, "aerobic", 0 }, new object?[] { 1, "", null, null, null, "", -1 });
        yield return Case("EnterCultureResult", new object?[] { 1, null, null, null, "aerobic", 0, null, null }, new object?[] { 0, null, null, null, "", -1, null, null });
        yield return Case("AddDoctor", new object?[] { "Dr A", null, null, 0m }, new object?[] { "", null, null, 101m });
        yield return Case("AddReferralEntity", new object?[] { "Lab", 2, null, null, null, null, null, null, null, 1 }, new object?[] { "", 2, null, null, null, null, null, null, null, 0 });
        yield return Case("AddCommentTemplate", new object?[] { 1, "note" }, new object?[] { 0, "" });
        yield return Case("UpdateCommentTemplate", new object?[] { 1, 1, "note" }, new object?[] { 0, 0, "" });
        yield return Case("DeleteCommentTemplate", new object?[] { 1, 1 }, new object?[] { 0, 0 });
        yield return Case("ApplyCommentTemplate", new object?[] { 1, 1, 1 }, new object?[] { 0, 0, 0 });
        yield return Case("MarkTestAsOutsourced", new object?[] { 1, 1, 1, 0m, 0m }, new object?[] { 0, 0, 0, -1m, -1m });
        yield return Case("SettleOutsourcedAccount", new object?[] { 1, 0 }, new object?[] { 0, 99 });
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Validator_accepts_valid_command_and_rejects_each_essential_invalid_value(
        Type validatorType, object validCommand, object invalidCommand)
    {
        var validator = (IValidator)Activator.CreateInstance(validatorType)!;

        Assert.True(validator.Validate(new ValidationContext<object>(validCommand)).IsValid);
        var invalid = validator.Validate(new ValidationContext<object>(invalidCommand));
        Assert.False(invalid.IsValid);
        Assert.NotEmpty(invalid.Errors);
    }

    private static object[] Case(string commandName, object?[] validArguments, object?[] invalidArguments)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        var commandType = assembly.GetTypes().Single(t => t.Name == $"{commandName}Command");
        var validatorType = assembly.GetTypes().Single(t => t.Name == $"{commandName}CommandValidator");
        return new[]
        {
            validatorType,
            Create(commandType, validArguments),
            Create(commandType, invalidArguments)
        };
    }

    private static object Create(Type commandType, object?[] arguments)
    {
        var parameters = commandType.GetConstructors().Single().GetParameters();
        var converted = arguments.Select((argument, index) =>
            argument is not null && parameters[index].ParameterType.IsEnum
                ? Enum.ToObject(parameters[index].ParameterType, argument)
                : argument).ToArray();
        return Activator.CreateInstance(commandType, converted)!;
    }
}
