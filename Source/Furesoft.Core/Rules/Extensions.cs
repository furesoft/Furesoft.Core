using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;

namespace Furesoft.Core.Rules;

public static class Extensions
{
    public static T To<T>(this object @object)
    {
        return @object != null ? (T) @object : default;
    }

    public static T To<T>(this Task<object> @object)
    {
        return @object != null ? (T) @object.Result : default;
    }

    public static Guid GetRuleEngineId<T>(this IGeneralRule<T> rule) where T : class, new()
    {
        return rule.Configuration.To<RuleEngineConfiguration<T>>().RuleEngineId;
    }

    public static string GetRuleName<T>(this IGeneralRule<T> rule) where T : class, new()
    {
        return rule.GetType().Name;
    }

    public static IRuleResult FindRuleResult<T>(this IEnumerable<IRuleResult> ruleResults)
    {
        return ruleResults.FirstOrDefault(
            r => string.Equals(r.Name, typeof(T).Name, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<IRuleResult> FindRuleResults<T>(this IEnumerable<IRuleResult> ruleResults)
    {
        return ruleResults.Where(r => string.Equals(r.Name, typeof(T).Name, StringComparison.OrdinalIgnoreCase));
    }

    public static IRuleResult FindRuleResult(this IEnumerable<IRuleResult> ruleResults, string ruleName)
    {
        return ruleResults.FirstOrDefault(r => string.Equals(r.Name, ruleName, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<IRuleResult> FindRuleResults(this IEnumerable<IRuleResult> ruleResults, string ruleName)
    {
        return ruleResults.Where(r => string.Equals(r.Name, ruleName, StringComparison.OrdinalIgnoreCase));
    }

    public static RuleEngine<T> ApplyRules<T>(this RuleEngine<T> ruleEngineExecutor,
        params IGeneralRule<T>[] rules) where T : class, new()
    {
        ruleEngineExecutor.AddRules(rules);

        return ruleEngineExecutor;
    }

    public static RuleEngine<T> ApplyRules<T>(this RuleEngine<T> ruleEngineExecutor,
        params Type[] rules) where T : class, new()
    {
        ruleEngineExecutor.AddRules(rules);

        return ruleEngineExecutor;
    }

    public static IEnumerable<IRuleResult> GetErrors(this IEnumerable<IRuleResult> ruleResults)
    {
        return ruleResults.Where(r => r.Error != null);
    }

    public static bool AnyError(this IEnumerable<IRuleResult> ruleResults)
    {
        return ruleResults.Any(r => r.Error != null);
    }

    public static RuleType GetRuleType<T>(this IGeneralRule<T> rule) where T : class, new()
    {
        if (rule.IsProactive) return RuleType.ProActiveRule;
        if (rule.IsReactive) return RuleType.ReActiveRule;
        if (rule.IsExceptionHandler) return RuleType.ExceptionHandlerRule;

        return RuleType.None;
    }
}

public enum RuleType
{
    None,
    ProActiveRule,
    ReActiveRule,
    ExceptionHandlerRule
}