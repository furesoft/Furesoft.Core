using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.ExpressionEvaluator.FunctionPlotter;
using System.ComponentModel;
using System.Threading;

namespace Furesoft.Core.ExpressionEvaluator.Macros
{
    [Description("Plot the function to an image file")]
    [ParameterDescription("function", "The function body")]
    internal class PlotMacro : Macro
    {
        public override string Name => "plot";

        public override Expression Invoke(MacroContext mc, params Expression[] arguments)
        {
            if (arguments.Length == 1 && mc.ParentCallNode is Expression e && !e.HasMessages)
            {
                var function = arguments[0];

                mc.ExpressionParser.RootScope.Variables.Add("x", 0);

                double execute(double i)
                {
                    Scope scope = mc.ExpressionParser.RootScope;
                    scope.Variables["x"]= i;

                    var result = mc.ExpressionParser.EvaluateExpression(function, scope);

                    if (result == null) return 0;

                    return result.Get<double>();
                };

                    var plotter = new Plotter(execute, -10, 10, -10, 10);
                    plotter.Draw();
                
                    OutputChannel.Send(plotter.Plot);

                // plotter.Plot.Save("plot.png", System.Drawing.Imaging.ImageFormat.Png);
            }

            return new TempExpr();
        }
    }
}
