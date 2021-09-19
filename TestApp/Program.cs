using Furesoft.Core.CLI;
using Nova.CodeDOM;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace TestApp
{
    public class Instruction : CodeObject
    {
        public Instruction(Parser parser, CodeObject parent) : base(parser, parent)
        {
            Mnemnonic = parser.GetIdentifierText();

            Arguments = Expression.ParseList(parser, parent, ",");
        }

        public ChildList<Expression> Arguments { get; set; }
        public string Mnemnonic { get; set; }
    }

    public class RegisterRef : SymbolicRef
    {
        public RegisterRef(int addr) : base(addr)
        {
        }

        public override bool IsConst => true;
    }

    internal static class Program
    {
        private static Dictionary<string, Label> _labels = new();

        private static Dictionary<string, int> _registers = new()
        {
            ["A"] = 0,
            ["B"] = 4,
            ["C"] = 8,
            ["D"] = 12,
            ["E"] = 16,
            ["F"] = 20,
        };

        public static int Main(string[] args)
        {
            var src = "loop: \n\tmov 0x12, [hello + 4];\ngoto loop;mov [hello + 4], B;mov B, A;";
            CodeObject.AutoDetectTabs = true;

            Parser.AddOperatorParsePoint("+", 2, true, false, parse);
            Parser.AddOperatorParsePoint("*", 1, true, false, parse2);
            Parser.AddParsePoint("[", ParseSquared);

            Parser.AddMultipleParsePoints(new[] { "mov", "load", "add", "sub", "inc" }, ParseMov);

            Label.AddParsePoints();
            Goto.AddParsePoints();

            //CodeUnit.LoadDefaultParsePoints();

            var expr = Expression.Parse(src, out var root);

            var result = Evaluate(expr);

            Block body = CodeUnit.LoadFragment(src, "d").Body;

            Bind(body);

            var instr = body.First();

            var children = body.GetChildren<Instruction>();

            return App.Current.Run();
        }

        private static void Bind(CodeObject obj)
        {
            if (obj is Block blk)
            {
                foreach (var child in blk)
                {
                    Bind(child);
                }
            }
            else if (obj is Label lbl)
            {
                _labels.Add(lbl.Name, lbl);
            }
            else if (obj is Instruction instr)
            {
                for (int i = 0; i < instr.Arguments.Count; i++)
                {
                    Expression arg = instr.Arguments[i];
                    if (arg is UnresolvedRef uref)
                    {
                        if (_registers.ContainsKey(uref.Reference.ToString()))
                        {
                            instr.Arguments[i] = new RegisterRef(_registers[uref.Reference.ToString()]);
                        }
                        else
                        {
                            instr.AttachMessage($"Reference '{uref.Reference}' cannot be bind", MessageSeverity.Error, MessageSource.Resolve);
                        }
                    }
                    else if (arg is SquaredExpression squared)
                    {
                        Bind(squared.Body);
                    }
                }
            }
            else if (obj is BinaryOperator binary)
            {
                Bind(binary.Left);
                Bind(binary.Right);
            }
            else if (obj is Goto gt)
            {
                if (_labels.ContainsKey(gt.Target.Name))
                {
                    gt.Target = new LabelRef(_labels[gt.Target.Name]);
                }
            }
            else if (obj is UnresolvedRef uref)
            {
                uref.Parent.AttachMessage($"Reference '{uref.Reference}' cannot be bind", MessageSeverity.Error, MessageSource.Resolve);
            }
        }

        private static int Evaluate(Expression expr)
        {
            if (expr is AddOp add)
            {
                return Evaluate(add.Left) + Evaluate(add.Right);
            }
            else if (expr is MulOp mul)
            {
                return Evaluate(mul.Left) * Evaluate(mul.Right);
            }
            else if (expr is Literal lit)
            {
                return int.Parse(lit.Text);
            }
            else
            {
                return 1;
            }
        }

        private static CodeObject parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new AddOp(parser, parent);
        }

        private static CodeObject parse2(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new MulOp(parser, parent);
        }

        private static CodeObject ParseMov(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Instruction(parser, parent);
        }

        private static CodeObject ParseSquared(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var obj = new SquaredExpression();

            parser.NextToken();

            obj.Body = Expression.Parse(parser, obj);

            parser.NextToken();

            return obj;
        }
    }

    internal class AddOp : BinaryArithmeticOperator
    {
        public AddOp(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public AddOp(Expression left, Expression right) : base(left, right)
        {
        }

        public override string Symbol => "+";

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            Left.AsTextExpression(writer, flags);
            AsTextOperator(writer, flags);
            Right.AsTextExpression(writer, flags);
        }

        public override int GetPrecedence()
        {
            return 2;
        }
    }

    internal class MulOp : BinaryArithmeticOperator
    {
        public MulOp(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public MulOp(Expression left, Expression right) : base(left, right)
        {
        }

        public override string Symbol => "*";

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
        }

        public override int GetPrecedence()
        {
            return 1;
        }
    }

    internal class SquaredExpression : Expression
    {
        public Expression Body { get; set; }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            writer.Write("[");
            Body.AsTextExpression(writer, flags);

            writer.Write("]");
        }
    }
}