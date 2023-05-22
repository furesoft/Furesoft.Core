﻿using Furesoft.Core.Rules;
using Furesoft.Core.Rules.DSL;
using Furesoft.Core.Rules.DSL.Nodes;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using RulesTest.Models;
using Xunit;

namespace RulesTest;

public class TestDsl
{
    [Fact]
    public void Not_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("not 5", "test.dsl");

        Assert.True(node.Tree is PrefixOperatorNode);

        var prefixOperator = (PrefixOperatorNode)node.Tree;

        Assert.Equal("not", prefixOperator.Operator.Name);
        Assert.True(prefixOperator.Expr is LiteralNode<int>);
        Assert.Equal(5, ((LiteralNode<int>)prefixOperator.Expr).Value);
    }
    
    [Fact]
    public void Comparison_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("5 is equal to 5", "test.dsl");

        Assert.True(node.Tree is BinaryOperatorNode);

        var binary = (BinaryOperatorNode)node.Tree;

        Assert.Equal("==", binary.Operator.Name);
        Assert.True(binary.LeftExpr is LiteralNode<int>);
        Assert.True(binary.RightExpr is LiteralNode<int>);
        Assert.Equal(5, ((LiteralNode<int>)binary.LeftExpr).Value);
        Assert.Equal(5, ((LiteralNode<int>)binary.RightExpr).Value);
    }

    [Fact]
    public void If_Should_Pass() {
        var node = Grammar.Parse<Grammar>("if 5 is equal to 5 then error 'something went wrong'", "test.dsl");

        Assert.True(node.Tree is IfNode);

        var ifNode = (IfNode)node.Tree;

        var binary = (BinaryOperatorNode)ifNode.Condition;

        // Asserts for Condition
        Assert.Equal("==", binary.Operator.Name);
        Assert.True(binary.LeftExpr is LiteralNode<int>);
        Assert.True(binary.RightExpr is LiteralNode<int>);
        Assert.Equal(5, ((LiteralNode<int>)binary.LeftExpr).Value);
        Assert.Equal(5, ((LiteralNode<int>)binary.RightExpr).Value);

        // Asserts for Body
        Assert.True(ifNode.Body is ErrorNode);

        var errorNode = (ErrorNode)ifNode.Body;

        Assert.Equal("something went wrong", errorNode.Message);
    }

    [Fact]
    public void SimpleRule_Should_Pass()
    {
        var engine = RuleEngine<Product>.GetInstance();
        
        engine.AddRule("1 + 1");

        var result = engine.Execute();
    }
}