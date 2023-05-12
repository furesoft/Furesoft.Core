﻿using System.Collections.Concurrent;
using Furesoft.Core.Rules.Interfaces;

namespace Furesoft.Core.Rules.Services;

internal sealed class AsyncRuleService<T> where T : class, new()
{
    private readonly IEnumerable<IRuleAsync<T>> _rules;
    private readonly IRuleEngineConfiguration<T> _ruleEngineConfiguration;
    private readonly RxRuleService<IRuleAsync<T>, T> _rxRuleService;
    private readonly ConcurrentBag<IRuleResult> _asyncRuleResults = new();
    private readonly ConcurrentBag<Task<IRuleResult>> _parallelRuleResults = new();

    public AsyncRuleService(IEnumerable<IRuleAsync<T>> rules,
        IRuleEngineConfiguration<T> ruleEngineTerminated)
    {
        _rules = rules;
        _rxRuleService = new(_rules);
        _ruleEngineConfiguration = ruleEngineTerminated;
    }

    public async Task InvokeAsync()
    {
        await ExecuteAsyncRules(_rxRuleService.FilterRxRules(_rules));
    }

    public async Task<IEnumerable<IRuleResult>> GetAsyncRuleResultsAsync()
    {
        await Task.WhenAll(_parallelRuleResults);

        Parallel.ForEach(_parallelRuleResults, rule =>
        {
            rule.Result.AssignRuleName(rule.GetType().Name);
            AddToAsyncRuleResults(rule.Result);
        });

        return _asyncRuleResults;
    }

    private async Task ExecuteAsyncRules(IEnumerable<IRuleAsync<T>> rules)
    {
        await ExecuteParallelRules(rules);

        foreach (var rule in OrderByExecutionOrder(rules))
        {
            await InvokeNestedRulesAsync(rule.Configuration.InvokeNestedRulesFirst, rule);

            if (rule.CanInvoke() && !_ruleEngineConfiguration.IsRuleEngineTerminated())
            {
                try
                {
                    await InvokeProactiveRulesAsync(rule);

                    AddToAsyncRuleResults(await ExecuteRuleAsync(rule));
                    rule.UpdateRuleEngineConfiguration(_ruleEngineConfiguration);

                    await InvokeReactiveRulesAsync(rule);
                }
                catch (Exception exception)
                {
                    rule.UnhandledException = exception;

                    if (_rxRuleService.GetExceptionRules().ContainsKey(rule.GetType()))
                    {
                        await InvokeExceptionRulesAsync(rule);
                    }
                    else
                    {
                        var globalExceptionHandler = _rules.GetGlobalExceptionHandler();

                        if (globalExceptionHandler is IRuleAsync<T> ruleAsync)
                        {
                            globalExceptionHandler.UnhandledException = exception;
                            await ExecuteAsyncRules(new List<IRuleAsync<T>> { ruleAsync });
                        }
                        else
                        {
                            throw;
                        }                            
                    }
                }

            }

            await InvokeNestedRulesAsync(!rule.Configuration.InvokeNestedRulesFirst, rule);
        }
    }

    private async Task ExecuteParallelRules(IEnumerable<IRuleAsync<T>> rules)
    {
        foreach (var rule in GetParallelRules(rules))
        {
            await InvokeNestedRulesAsync(rule.Configuration.InvokeNestedRulesFirst, rule);

            if (rule.CanInvoke() && !_ruleEngineConfiguration.IsRuleEngineTerminated())
            {
                await InvokeProactiveRulesAsync(rule);

                _parallelRuleResults.Add(await Task.Factory.StartNew(async () =>
                    {
                        IRuleResult ruleResult = null;

                        try
                        {
                            ruleResult = await ExecuteRuleAsync(rule);
                        }
                        catch (Exception exception)
                        {
                            rule.UnhandledException = exception;
                            if (_rxRuleService.GetExceptionRules().ContainsKey(rule.GetType()))
                            {
                                await InvokeExceptionRulesAsync(rule);
                            }
                            else
                            {
                                var globalExceptionHandler = _rules.GetGlobalExceptionHandler();

                                if (globalExceptionHandler is IRuleAsync<T> ruleAsync)
                                {
                                    globalExceptionHandler.UnhandledException = exception;
                                    await ExecuteAsyncRules(new List<IRuleAsync<T>> { ruleAsync });
                                }
                                else
                                {
                                    throw;
                                }                                
                            }
                        }

                        return ruleResult;

                    }, rule.ParallelConfiguration.CancellationTokenSource?.Token ?? CancellationToken.None,
                    rule.ParallelConfiguration.TaskCreationOptions,
                    rule.ParallelConfiguration.TaskScheduler));

                await InvokeReactiveRulesAsync(rule);
            }

            await InvokeNestedRulesAsync(!rule.Configuration.InvokeNestedRulesFirst, rule);
        }
    }

    private static async Task<IRuleResult> ExecuteRuleAsync(IRuleAsync<T> rule)
    {
        await rule.BeforeInvokeAsync();

        if (rule.IsParallel && rule.ParallelConfiguration.CancellationTokenSource != null &&
            rule.ParallelConfiguration.CancellationTokenSource.Token.IsCancellationRequested)
        {
            return null;
        }

        var ruleResult = await rule.InvokeAsync();

        await rule.AfterInvokeAsync();

        ruleResult.AssignRuleName(rule.GetType().Name);

        return ruleResult;
    }

    private async Task InvokeReactiveRulesAsync(IRuleAsync<T> asyncRule)
    {
        if (_rxRuleService.GetReactiveRules().ContainsKey(asyncRule.GetType()))
        {
            await ExecuteAsyncRules(_rxRuleService.GetReactiveRules()[asyncRule.GetType()]);
        }
    }

    private async Task InvokeProactiveRulesAsync(IRuleAsync<T> asyncRule)
    {
        if (_rxRuleService.GetProactiveRules().ContainsKey(asyncRule.GetType()))
        {
            await ExecuteAsyncRules(_rxRuleService.GetProactiveRules()[asyncRule.GetType()]);
        }
    }

    private async Task InvokeExceptionRulesAsync(IRuleAsync<T> asyncRule)
    {
        var exceptionRules =
            _rxRuleService.GetExceptionRules()[asyncRule.GetType()]
                .Select(rule =>
                {
                    rule.UnhandledException = asyncRule.UnhandledException;
                    return rule;
                }).ToList();

        await ExecuteAsyncRules(exceptionRules);
    }

    private async Task InvokeNestedRulesAsync(bool invokeNestedRules, IRuleAsync<T> rule)
    {
        if (invokeNestedRules && rule.IsNested)
        {
            await ExecuteAsyncRules(_rxRuleService.FilterRxRules(rule.GetRules().OfType<IRuleAsync<T>>().ToList()));
        }
    }

    private void AddToAsyncRuleResults(IRuleResult ruleResult)
    {
        if (ruleResult != null) _asyncRuleResults.Add(ruleResult);
    }

    private static IEnumerable<IRuleAsync<T>> OrderByExecutionOrder(IEnumerable<IRuleAsync<T>> rules)
    {
        return rules.GetRulesWithExecutionOrder().OfType<IRuleAsync<T>>()
            .Concat(rules.GetRulesWithoutExecutionOrder(rule => !((IRuleAsync<T>)rule).IsParallel).OfType<IRuleAsync<T>>());
    }

    private static IEnumerable<IRuleAsync<T>> GetParallelRules(IEnumerable<IRuleAsync<T>> rules)
    {
        return rules.Where(r => r.IsParallel && !r.Configuration.ExecutionOrder.HasValue)
            .AsParallel();
    }
}