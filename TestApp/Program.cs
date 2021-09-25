using Furesoft.Core.CLI;
using Furesoft.Core.CodeDom.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.GotoTargets;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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
        public static Dictionary<string, int> _registers = new()
        {
            ["A"] = 0,
            ["B"] = 4,
            ["C"] = 8,
            ["D"] = 12,
            ["E"] = 16,
            ["F"] = 20,
        };

        private readonly string Register;

        public RegisterRef(Parser parser, CodeObject parent, string register) : base(register)
        {
            this.Parent = parent;
            Register = register;

            SetField(ref this._reference, _registers[register], false);
        }

        public override bool IsConst => true;
        public override string Name => Register;

        public static new void AddParsePoints()
        {
            Parser.AddMultipleParsePoints(_registers.Keys.ToArray(), Parse);
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var result = new RegisterRef(parser, parent, parser.TokenText);

            parser.NextToken();

            return result;
        }
    }

    internal static class Program
    {
        private static Dictionary<string, Label> _labels = new();

        private static byte[] ram = new byte[50];

        public static void Evaluate(CodeObject obj)
        {
            if (obj is Block blk)
            {
                foreach (var cn in blk)
                {
                    Evaluate(cn);
                }
            }
            else if (obj is Instruction instr)
            {
                var first = instr.Arguments[0];
                var second = instr.Arguments[1];
                var third = instr.Arguments[2];

                var firstValue = EvaluateExpression(first);
                var secondValue = EvaluateExpression(second);
                var thirdValue = EvaluateExpression(third);

                if (instr.Mnemnonic == "mov")
                {
                    if (first is Literal val && second is SquaredExpression mem)
                    {
                        //mov val in to ram
                        ram[secondValue] = (byte)firstValue;
                    }
                    else if (first is Literal val1 && second is RegisterRef r0)
                    {
                        //mov lit in to ram
                        ram[secondValue] = (byte)firstValue;
                    }
                    else
                    {
                        ram[secondValue] = ram[firstValue];
                    }
                }
                else if (instr.Mnemnonic == "add")
                {
                    ram[firstValue] = (byte)(ram[secondValue] + ram[thirdValue]);
                }
                else if (instr.Mnemnonic == "clear")
                {
                    ram[firstValue] = 0;
                }
            }
        }

        public static int Main(string[] args)
        {
            var src = "mov 12, B;mov B, D;add A,B,D;mov 42, [B + D];mov 1, [(A+C+D+E)*8];";

            Parser.AddParsePoint("[", ParseSquared);

            Add.AddParsePoints();
            Multiply.AddParsePoints();
            Divide.AddParsePoints();
            Subtract.AddParsePoints();

            Parser.AddMultipleParsePoints(new[] { "mov", "add", "sub", "inc", "clear" }, (parser, parent, flags) =>
            {
                return new Instruction(parser, parent);
            });

            Label.AddParsePoints();
            Goto.AddParsePoints();
            TypeRef.AddParsePoints();
            SizeOf.AddParsePoints();
            LocalDecl.AddParsePoints();
            RegisterRef.AddParsePoints();
            Expression.AddParsePoints();

            // CodeUnit.LoadDefaultParsePoints();

            var expr = Expression.Parse("sizeof(int) * 4 + 2", out var root);
            //Parser.Clear();

            var result = EvaluateExpression(expr);

            Block body = CodeUnit.LoadFragment(src, "d").Body;

            Bind(body);
            Evaluate(body);

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

                    if (arg is SquaredExpression squared)
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
            else if (obj is LocalDecl ld && ld.Initialization is Literal i && ld.Type is UnresolvedRef ur && ur.Name == "var")
            {
                if (int.TryParse(i.Text, out var r))
                {
                    ld.Type = TypeRef.IntRef;
                }
            }
        }

        private static int EvaluateExpression(Expression expr)
        {
            if (expr is Add add)
            {
                if (add.Left is RegisterRef rl)
                {
                    add.Left = new Literal(ram[(int)rl.Reference]);
                }
                else
                {
                    add.Left = EvaluateExpression(add.Left);
                }

                if (add.Right is RegisterRef rr)
                {
                    add.Right = new Literal(ram[(int)rr.Reference]);
                }
                else
                {
                    add.Right = EvaluateExpression(add.Right);
                }

                return EvaluateExpression(add.Left) + EvaluateExpression(add.Right);
            }
            else if (expr is Multiply mul)
            {
                return EvaluateExpression(mul.Left) * EvaluateExpression(mul.Right);
            }
            else if (expr is Subtract sub)
            {
                return EvaluateExpression(sub.Left) - EvaluateExpression(sub.Right);
            }
            else if (expr is Divide div)
            {
                return EvaluateExpression(div.Left) / EvaluateExpression(div.Right);
            }
            else if (expr is Literal lit)
            {
                return int.Parse(lit.Text);
            }
            else if (expr is SizeOf so && so.Expression is TypeRef tr)
            {
                var typeSize = Marshal.SizeOf((Type)tr.Reference);

                return typeSize;
            }
            else if (expr is RegisterRef regRef)
            {
                return (int)regRef.Reference;
            }
            else if (expr is SquaredExpression squared)
            {
                return EvaluateExpression(squared.Body);
            }
            else
            {
                return 1;
            }
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