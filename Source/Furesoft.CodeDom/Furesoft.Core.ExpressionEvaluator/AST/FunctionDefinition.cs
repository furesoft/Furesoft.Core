﻿

namespace Furesoft.Core.ExpressionEvaluator.AST;

public class FunctionDefinition : BlockStatement, IParameters, IEvaluatableStatement
{
    private ChildList<ParameterDecl> _parameters;

    public FunctionDefinition(string name)
    {
        Name = name;
        _parameters = new ChildList<ParameterDecl>();
    }

    public bool HasParameters => ParameterCount > 0;
    public string Name { get; }
    public int ParameterCount => Parameters.Count;

    public ChildList<ParameterDecl> Parameters => _parameters;

    public void Evaluate(ExpressionParser ep)
    {
        string mangledName = Name + ":" + ParameterCount;

        ep.RootScope.Functions.Add(mangledName, this);
    }
}