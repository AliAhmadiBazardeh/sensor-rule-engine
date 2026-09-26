using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public class RuleOperatorResolverTests
{
    private readonly RuleOperatorResolver _sut;

    public RuleOperatorResolverTests()
    {
        var operators = new IRuleOperator[]
        {
            new GreaterThanOperator(),
            new GreaterThanOrEqualOperator(),
            new LessThanOperator(),
            new LessThanOrEqualOperator(),
            new EqualOperator(),
            new BetweenOperator()
        };

        _sut = new RuleOperatorResolver(operators);
    }

    [Theory]
    [InlineData(
        RuleOperatorType.GreaterThan,
        typeof(GreaterThanOperator))]

    [InlineData(
        RuleOperatorType.GreaterThanOrEqual,
        typeof(GreaterThanOrEqualOperator))]

    [InlineData(
        RuleOperatorType.LessThan,
        typeof(LessThanOperator))]

    [InlineData(
        RuleOperatorType.LessThanOrEqual,
        typeof(LessThanOrEqualOperator))]

    [InlineData(
        RuleOperatorType.Equal,
        typeof(EqualOperator))]

    [InlineData(
        RuleOperatorType.Between,
        typeof(BetweenOperator))]
    public void Should_resolve_correct_operator(
        RuleOperatorType operatorType,
        Type expectedType)
    {
        var result = _sut.Resolve(operatorType);

        Assert.IsType(expectedType, result);
    }

    [Fact]
    public void Should_throw_when_operator_is_not_registered()
    {
        var operators = new IRuleOperator[]
        {
            new GreaterThanOperator()
        };

        var sut = new RuleOperatorResolver(operators);

        Assert.Throws<InvalidOperationException>(
            () => sut.Resolve(RuleOperatorType.Between));
    }
}