﻿using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace TestApp
{
    //f: x is N {2,10}
    //f: x is N x > 10

    public class FunctionArgumentConditionDefinition : Annotation
    {
        public FunctionArgumentConditionDefinition(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public Expression Condition { get; set; }

        public string Function { get; set; }

        public string NumberRoom { get; set; }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint(":", Parse);
        }

        public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var name = parser.RemoveLastUnusedToken();

            var result = new FunctionArgumentConditionDefinition(parser, parent);
            result.Function = name.Text;

            parser.NextToken();

            var parameter = parser.GetIdentifierText();

            if (!result.ParseExpectedToken(parser, "in"))
            {
                return null;
            }

            result.NumberRoom = parser.GetIdentifierText();
            result.Condition = Expression.Parse(parser, result, false, ";");

            return result;
        }
    }
}