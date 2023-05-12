using Furesoft.Core.Rules.Exceptions;
using Furesoft.Core.Rules.Interfaces;

namespace Furesoft.Core.Rules;

internal static class InternalExtensions
{
    public static bool CanInvoke<T>(this IGeneralRule<T> rule) where T : class, new() =>
        !rule.Configuration.Skip && rule.Configuration.Constraint.Invoke2(rule.Model);

    public static bool Invoke2<T>(this Predicate<T> predicate, T model) =>
        predicate == null || predicate(model);


    public static void AssignRuleName(this IRuleResult ruleResult, string ruleName)
    {
        if (ruleResult != null) ruleResult.Name = ruleResult.Name ?? ruleName;
    }

    public static void Validate<T>(this T model)
    {
        if (model == null) throw new ModelInstanceNotFoundException();
    }

    public static void UpdateRuleEngineConfiguration<T>(this IGeneralRule<T> rule,
        IConfiguration<T> ruleEngineConfiguration) where T : class, new()
    {
        if (ruleEngineConfiguration.Terminate == null && rule.Configuration.Terminate == true)
        {
            ruleEngineConfiguration.Terminate = true;
        }
    }

    public static bool IsRuleEngineTerminated<T>(this IConfiguration<T> ruleEngineConfiguration) where T : class, new()
        => ruleEngineConfiguration.Terminate != null && ruleEngineConfiguration.Terminate.Value;

    public static IEnumerable<IGeneralRule<T>> GetRulesWithExecutionOrder<T>(this IEnumerable<IGeneralRule<T>> rules,
        Func<IGeneralRule<T>, bool> condition = null) where T : class, new()
    {
        condition = condition ?? (rule => true);

        return rules.Where(r => r.Configuration.ExecutionOrder.HasValue)
            .Where(condition)
            .OrderBy(r => r.Configuration.ExecutionOrder);
    }

    public static IEnumerable<IGeneralRule<T>> GetRulesWithoutExecutionOrder<T>(this IEnumerable<IGeneralRule<T>> rules,
        Func<IGeneralRule<T>, bool> condition = null) where T : class, new()
    {
        condition = condition ?? (k => true);

        return rules.Where(r => !r.Configuration.ExecutionOrder.HasValue)
            .Where(condition);
    }

    public static IGeneralRule<T> GetGlobalExceptionHandler<T>(this IEnumerable<IGeneralRule<T>> rules) where T : class, new()
    {
        var globalExceptionHandler = rules.Where(r => r.IsGlobalExceptionHandler).ToList();

        if (globalExceptionHandler.Count > 1)
        {
            throw new GlobalHandlerException("Found multiple GlobalHandlerException. Only one can be defined.");
        }

        return globalExceptionHandler.SingleOrDefault();
    }
}