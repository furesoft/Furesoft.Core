// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using Furesoft.Core.ExpressionEvaluator;
using NUnit.Framework;
using System;
using System.Collections.Generic;

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
                yield return new TestCaseData("log10(100)", Math.Log10(100));
                yield return new TestCaseData("floor(PI)", Math.Floor(Math.PI));
                yield return new TestCaseData("sin(2 * 4 + floor(PI))", Math.Sin(2 * 4 + Math.Floor(Math.PI)));
                yield return new TestCaseData("areaTriangle(width, height) = width * height / 2; areaTriangle(5,10);", 25);
                yield return new TestCaseData("use \"geometry.math\"; areaRectangle(5, 3);", 15);
                yield return new TestCaseData("g: x in N [5, INFINITY];g(x) = x*x; g(6);", 36);
                yield return new TestCaseData("|-4**2|;", 16);
                yield return new TestCaseData("-|4**2|;", -16);
                yield return new TestCaseData("use \"geometry.math\"; -areaRectangle(5, 3);", -15);
            }
        }

        [Test, TestCaseSource(nameof(TestData))]
        public void FunctionCall_Should_Pass(string input, double expected)
        {
            ExecuteTest(input, expected);
        }

        [TestCase("4*5", 20)]
        [TestCase("3+4*5", 23)]
        [TestCase("(3+4)*5", 35)]
        [TestCase("PI*2", Math.PI * 2)]
        public void SimpleExpressions_Should_Pass(string input, double expected)
        {
            ExecuteTest(input, expected);
        }

        private void ExecuteTest(string input, double expected)
        {
            var ep = new ExpressionParser();
            ep.RootScope.Import(typeof(Math));

            var result = ep.Evaluate(input);

            Assert.That(result.Values, Does.Contain(expected), "Calculation is wrong");
        }
    }
}