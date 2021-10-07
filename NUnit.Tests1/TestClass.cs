﻿// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.ExpressionEvaluator;
using Furesoft.Core.ExpressionEvaluator.Library;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.Tests1
{
    [TestFixture]
    public class TestClass
    {
        static TestClass()
        {
            ExpressionParser.Init();
        }

        private static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData("xy = 5; xy", 5);
                yield return new TestCaseData("log10(100)", Math.Log10(100));
                yield return new TestCaseData("floor(PI)", Math.Floor(Math.PI));
                yield return new TestCaseData("-2 * 5", -10);
                yield return new TestCaseData("-2 * -5", 10);
                yield return new TestCaseData("3.14 * 2", 6.28);
                yield return new TestCaseData("-3.14 * 2", -6.28);
                yield return new TestCaseData("round(sin(2 * 4 + floor(PI)), 5)", Math.Sin(2 * 4 + Math.Floor(Math.PI)));
                yield return new TestCaseData("areaTriangle(width, height) = width * height / 2; areaTriangle(5,10);", 25);
                yield return new TestCaseData("use \"geometry.math\"; geometry.areaRectangle(5, 3);", 15);
                yield return new TestCaseData("g: x in N [5, INFINITY];g(x) = x*x; g(6);", 36);
                yield return new TestCaseData("g: x in N;g(x) = x*x; g(6);", 36);
                yield return new TestCaseData("|-4^2|;", 16);
                yield return new TestCaseData("-|4^2|;", -16);
                yield return new TestCaseData("use \"geometry.math\"; -geometry.areaRectangle(5, 3);", -15);
                yield return new TestCaseData("use geometry; round(circumference(5), 5);", 31.4159265359);
                yield return new TestCaseData("round(geometry.circumference(5), 5);", 31.4159265359);
                yield return new TestCaseData("module trignonomic; b(p) = 2 * PI / p; b(PI);", 2);
                yield return new TestCaseData("-(2+3+4)", -9);
                yield return new TestCaseData("f: x in N 2 <= x < 20; f(x) = 2*x; f(5);", 10);
                yield return new TestCaseData("f: x in N; f(x) = 2*x; f(5);", 10);
                yield return new TestCaseData("h(x) = x^2; h(x, y) = x ^ y; h(4, 2)", 16);
                yield return new TestCaseData("alias round as rnd; rnd(2.345, 1)", 2.3);
                yield return new TestCaseData("alias geometry.circumference as umfang; round(umfang(1), 5);", 2 * Math.PI);
                yield return new TestCaseData("5!", 120);
                yield return new TestCaseData("set P in N = 1 < x && x % 1 == 0 && x % x == 0;", 0);
                yield return new TestCaseData("set P in N = 1 < x && x % 1 == 0 && x % x == 0; set MP in P = x < 100;", 0);
                yield return new TestCaseData("set D = {0,1,2,3,4,5,6};", 0);
                yield return new TestCaseData("set P in N = 1 < x && x % 1 == 0 && x % x == 0;set MP in P = x < 100;f: y in MP; f(y) = y; f(2);", 2);
                yield return new TestCaseData("set P in N = {2,4,6,8}; f: y in P; f(y) = y; f(2);", 2);
                yield return new TestCaseData("set P in N = {2,4,6,8,9}; f: y in P; f(y) = y; f(2);", 2);
                yield return new TestCaseData("rename(round, rndm);rndm(3.14);", 3);
                yield return new TestCaseData("rename(round(2), rndm);rndm(3.14, 1);", 3.1);
                yield return new TestCaseData("f(x) = x * 2; f(x=2);", 4);
                yield return new TestCaseData("f(x, y) = x * y; f(x=2, y=3);", 6);
                yield return new TestCaseData("f(x, y) = x * y; f(1, y=3);", 3);
                yield return new TestCaseData("f(x, y) = x * y; f(y=3, 2);", 6);
            }
        }

        [Test, TestCaseSource(nameof(TestData))]
        public void ExtendedExpressions_Should_Pass(string input, double expected)
        {
            ExecuteTest(input, expected);
        }

        [TestCase("4*5", 20)]
        [TestCase("3+4*5", 23)]
        [TestCase("(3+4)*5", 35)]
        [TestCase("2", 2)]
        [TestCase("-2", -2)]
        [TestCase("round(PI*2, 5)", Math.PI * 2)]
        public void SimpleExpressions_Should_Pass(string input, double expected)
        {
            ExecuteTest(input, expected);
        }

        private static void ExecuteTest(string input, double expected)
        {
            var ep = new ExpressionParser();
            ep.RootScope.Import(typeof(Math));
            ep.RootScope.Import(typeof(Core));

            ep.RootScope.Macros.Add("rename", new Func<MacroContext, Expression, Expression, Expression>(RenameFunction));

            ep.Import(typeof(Geometry));

            var result = ep.Evaluate(input);
            if (result.Errors.Count == 0)
            {
                if (result.Values.Any())
                {
                    Assert.That(result.Values, Does.Contain(Math.Round(expected, 5)), "Calculation is wrong");
                }
            }
            else
            {
                Assert.Fail(result.Errors.First().Text);
            }
        }

        private static Expression RenameFunction(MacroContext mc, Expression func, Expression newName)
        {
            if (func is UnresolvedRef oldRef && oldRef.Reference is string oldName && newName is UnresolvedRef nameref && nameref.Reference is string newNameString)
            {
                return RenameInternal(mc, oldName, newNameString);
            }
            else if (func is Call c && c.ArgumentCount == 1 && c.Arguments[0] is Literal l && int.TryParse(l.Text, out var argCount))
            {
                if (c.Expression is UnresolvedRef oldRef1 && oldRef1.Reference is string oldName1 && newName is UnresolvedRef nameref1 && nameref1.Reference is string newNameString1)
                {
                    return RenameInternal(mc, oldName1, newNameString1, argCount);
                }
            }

            return func;
        }

        private static Expression RenameInternal(MacroContext mc, string oldName, string newNameString, int argumentCount = 1)
        {
            var funcRef = mc.GetImportedFunctionForName(oldName + ":" + argumentCount, out var mangledName);

            mc.Scope.ImportedFunctions.Remove(mangledName);

            mc.Scope.ImportedFunctions.Add(newNameString + ":" + mangledName.Split(':')[1], funcRef);

            return new TempExpr();
        }
    }
}