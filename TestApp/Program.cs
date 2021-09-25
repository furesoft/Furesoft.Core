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
            var src = "var k = 12; sizeof(int); loop: \n\tmov 0x12, [A + 4];\ngoto loop;mov [hello + 4], B;mov B, A;";

            Parser.AddParsePoint("[", ParseSquared);

            Add.AddParsePoints();
            Multiply.AddParsePoints();
            Divide.AddParsePoints();
            Subtract.AddParsePoints();

            Parser.AddMultipleParsePoints(new[] { "mov", "load", "add", "sub", "inc" }, (parser, parent, flags) =>
            {
                return new Instruction(parser, parent);
            });

            Label.AddParsePoints();
            Goto.AddParsePoints();
            TypeRef.AddParsePoints();
            SizeOf.AddParsePoints();
            LocalDecl.AddParsePoints();

            // CodeUnit.LoadDefaultParsePoints();

            var expr = Expression.Parse("sizeof(int) * 4 + 2", out var root);
            //Parser.Clear();

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
                if (binary.Left is UnresolvedRef ul)
                {
                    if (_registers.ContainsKey(ul.Reference.ToString()))
                    {
                        binary.Left = new RegisterRef(_registers[ul.Reference.ToString()]);
                    }
                }
                else if (binary.Right is UnresolvedRef ur)
                {
                    if (_registers.ContainsKey(ur.Reference.ToString()))
                    {
                        binary.Right = new RegisterRef(_registers[ur.Reference.ToString()]);
                    }
                }
                else
                {
                    Bind(binary.Left);
                    Bind(binary.Right);
                }
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

        private static int Evaluate(Expression expr)
        {
            if (expr is Add add)
            {
                return Evaluate(add.Left) + Evaluate(add.Right);
            }
            else if (expr is Multiply mul)
            {
                return Evaluate(mul.Left) * Evaluate(mul.Right);
            }
            else if (expr is Subtract sub)
            {
                return Evaluate(sub.Left) - Evaluate(sub.Right);
            }
            else if (expr is Divide div)
            {
                return Evaluate(div.Left) / Evaluate(div.Right);
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