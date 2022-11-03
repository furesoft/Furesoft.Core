using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;

namespace Furesoft.Core.CodeDom.CodeDOM;

public abstract class VisitorBase<T>
{
    public virtual T Visit(CodeUnit unit)
    {
        return Visit(unit.Body);
    }

    public virtual T Visit(Block block)
    {
        foreach (var expr in block)
        {
            Visit(expr);
        }

        return default;
    }

    public abstract T Visit(Add add);
    public abstract T Visit(Divide div);
    public abstract T Visit(Multiply mul);
    public abstract T Visit(Mod mod);
    public abstract T Visit(Subtract sub);
    
    public abstract T Visit(If @if);
    public abstract T Visit(Switch @switch);
    
    public abstract T Visit(NewOperator newOperator);
    public abstract T Visit(TypeOperator typeOperator);
    
    public abstract T Visit(Call call);
    public abstract T Visit(DefaultValue defaultValue);
    public abstract T Visit(SizeOf sizeOf);
    public abstract T Visit(Index index);
    
    public abstract T Visit(BinaryOperator binaryOperator);
    public abstract T Visit(RelationalOperator relationalOperator);
    public abstract T Visit(BinaryBooleanOperator binarybooleanOperator);
    public abstract T Visit(BinaryBitwiseOperator binaryBitwiseOperator);
    public abstract T Visit(UnaryOperator unaryOperator);
    public abstract T Visit(PostUnaryOperator postUnaryOperator);
    public abstract T Visit(PreUnaryOperator preUnaryOperator);
    public abstract T Visit(BinaryShiftOperator binaryShiftOperator);
    public abstract T Visit(ArgumentsOperator argumentsOperator);
    public abstract T Visit(BaseListTypeDecl baseListTypeDecl);
    
    public abstract T Visit(BlockStatement blockStatement);
    public abstract T Visit(MethodDeclBase methodDeclBase);
    public abstract T Visit(Statement statement);
    
    public abstract T Visit(Assignment assignment);

    public abstract T Visit(Expression expression);
    public abstract T Visit(Operator expression);
    public abstract T Visit(BinaryArithmeticOperator expression);
    public abstract T Visit(CodeObject obj);
    
    

}