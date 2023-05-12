namespace Furesoft.Core.Rules.Interfaces;

public interface IRuleEngineConfiguration<T> : IConfiguration<T>
{
    Guid RuleEngineId { get; set; }
}