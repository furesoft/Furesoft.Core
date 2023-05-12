﻿using Furesoft.Core.Rules.Interfaces;

namespace Furesoft.Core.Rules.Models;

public class ParallelConfiguration<T> : IParallelConfiguration<T> where T : class, new()
{
    public TaskCreationOptions TaskCreationOptions { get; set; } = TaskCreationOptions.None;

    public CancellationTokenSource CancellationTokenSource { get; set; }

    public TaskScheduler TaskScheduler { get; set; } = TaskScheduler.Default;

    public bool NestedParallelRulesInherit { get; set; }
}