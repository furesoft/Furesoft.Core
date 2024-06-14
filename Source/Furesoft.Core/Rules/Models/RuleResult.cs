using Furesoft.Core.Rules.Interfaces;

namespace Furesoft.Core.Rules.Models;

public class RuleResult : IRuleResult
{
    public RuleResult()
    {
        Data = [];
    }

    public string Name { get; set; }

    public object Result { get; set; }

    public Dictionary<string, object> Data { get; set; }

    public IError Error { get; set; }

    public static Task<IRuleResult> Nil()
    {
        return Task.FromResult<IRuleResult>(null);
    }

    public static async Task<IRuleResult> CreateAsync(RuleResult ruleResult)
    {
        return await Task.FromResult(ruleResult);
    }
}