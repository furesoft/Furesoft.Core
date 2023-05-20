﻿using Furesoft.Core.Rules.DSL;
using Furesoft.Core.Rules.Exceptions;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using Furesoft.Core.Rules.Services;
using Furesoft.PrattParser;

namespace Furesoft.Core.Rules;

public sealed class RuleEngine
{
    public static RuleEngine<T> GetInstance<T>(T model)
        where T : class, new()
    {
        return RuleEngine<T>.GetInstance(model);
    }
}

/// <summary>
/// Rule Engine.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class RuleEngine<T> where T : class, new()
{
    private T _model;
    private IDependencyResolver _dependencyResolver;
    private RuleService<T> _ruleService;
    private AsyncRuleService<T> _asyncRuleService;
    private readonly List<object> _rules = new();
    private readonly Guid _ruleEngineId = Guid.NewGuid();
    private readonly RuleEngineConfiguration<T> _ruleEngineConfiguration = new(new Configuration<T>());

    /// <summary>
    /// Rule engine ctor.
    /// </summary>
    private RuleEngine() { }

    /// <summary>
    /// Set dependency resolver
    /// </summary>
    /// <param name="dependencyResolver"></param>
    public void SetDependencyResolver(IDependencyResolver dependencyResolver) => _dependencyResolver = dependencyResolver;

    /// <summary>
    /// Get a new instance of RuleEngine
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="dependencyResolver"></param>
    /// <returns></returns>
    public static RuleEngine<T> GetInstance(T instance = null, IDependencyResolver dependencyResolver = null) =>
        new()
        {
            _model = instance,
            _dependencyResolver = dependencyResolver,
        };

    /// <summary>
    /// Used to add rules to rule engine.
    /// </summary>
    /// <param name="rules">Rule(s) list.</param>
    public void AddRules(params IGeneralRule<T>[] rules) => _rules.AddRange(rules);

    /// <summary>
    /// Used to add rules to rule engine.
    /// </summary>
    /// <param name="rules">Rule(s) list.</param>
    public void AddRules(params Type[] rules)
    {
        foreach (var rule in rules)
        {
            if (!rule.IsSubclassOf(typeof(Rule<T>)) && !rule.IsSubclassOf(typeof(RuleAsync<T>)))
            {
                throw new InvalidRuleObjectException($"{rule} is invalid. Must inherit from {nameof(Rule<T>)} or {nameof(RuleAsync<T>)}");
            }
        }

        _rules.AddRange(rules);
    }

    /// <summary>
    /// Add a dynamic rule
    /// </summary>
    /// <param name="source"></param>
    public void AddRule(string source)
    {
        AddRule(new DslRule<T>(source));
    }

    /// <summary>
    /// Used to add rule to rule engine.
    /// </summary>
    /// <param name="rule">Rule(s) list.</param>
    public void AddRule(IGeneralRule<T> rule) => _rules.Add(rule);

    /// <summary>
    /// Used to add rule to rule engine.
    /// </summary>
    public void AddRule<TK>() where TK: IGeneralRule<T> => _rules.Add(typeof(TK));

    /// <summary>
    /// Used to set instance.
    /// </summary>
    /// <param name="instance">_model</param>
    public void SetInstance(T instance) => _model = instance;

    /// <summary>
    /// Used to execute async rules.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<IRuleResult>> ExecuteAsync()
    {
        if (!_rules.Any()) return Enumerable.Empty<IRuleResult>().ToArray();

        var rules = await new BootstrapService<T>(_model, _ruleEngineId, _dependencyResolver)
            .BootstrapAsync(_rules);

        _asyncRuleService = new(rules, _ruleEngineConfiguration);

        await _asyncRuleService.InvokeAsync();

        return await _asyncRuleService.GetAsyncRuleResultsAsync();
    }

    /// <summary>
    /// Used to execute rules.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IRuleResult> Execute()
    {
        if (!_rules.Any()) return Enumerable.Empty<IRuleResult>().ToArray();

        var rules = new BootstrapService<T>(_model, _ruleEngineId, _dependencyResolver)
            .Bootstrap(_rules);

        _ruleService = new(rules, _ruleEngineConfiguration);

        _ruleService.Invoke();

        return _ruleService.GetRuleResults();
    }
}