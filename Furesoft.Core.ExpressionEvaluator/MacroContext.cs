﻿using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.ExpressionEvaluator.AST;
using System;
using System.Text;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class MacroContext
    {
        private const string Alphabet = "abcdefghiklmopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private Random _random = new();

        public MacroContext(Scope scope, CodeObject parentCallNode)
        {
            Scope = scope;
            ParentCallNode = parentCallNode;
        }

        public CodeObject ParentCallNode { get; }
        public Scope Scope { get; }

        public UnresolvedRef GenerateSymbol()
        {
            //$h4g358c7
            var sb = new StringBuilder();
            sb.Append("$");

            for (int i = 0; i < 8; i++)
            {
                sb.Append(Alphabet[_random.Next(0, Alphabet.Length)]);
            }

            return new UnresolvedRef(sb.ToString());
        }

        public FunctionDefinition GetFunctionForName(string name, out string mangledName)
        {
            foreach (var iF in Scope.Functions)
            {
                if (iF.Key.StartsWith(name))
                {
                    mangledName = iF.Key;

                    return iF.Value;
                }
            }

            mangledName = null;

            return null;
        }

        public Func<double[], double> GetImportedFunctionForName(string name, out string mangledName)
        {
            foreach (var iF in Scope.ImportedFunctions)
            {
                if (iF.Key.StartsWith(name))
                {
                    mangledName = iF.Key;

                    return iF.Value;
                }
            }

            mangledName = null;

            return null;
        }
    }
}