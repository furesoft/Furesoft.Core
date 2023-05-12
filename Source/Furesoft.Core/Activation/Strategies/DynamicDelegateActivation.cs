using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Furesoft.Core.Activation.Strategies;

public class DynamicDelegateActivation : IActivationStrategy
{
    private delegate object ObjectActivator(object[] args);

    public object Activate(Type type, object[] args)
    {
        var argTypes = args?.Select(_ => _.GetType()).ToArray();
        var ctor = type.GetConstructor(argTypes!);

        if (ctor != null)
        {
            var a = GetActivator(ctor);

            return a(args);
        }

        return null;
    }

    private static ObjectActivator GetActivator(ConstructorInfo ctor)
    {
        var paramsInfo = ctor.GetParameters();

        //create a single param of type object[]
        var param = Expression.Parameter(typeof(object[]), "args");

        var argsExp = new Expression[paramsInfo.Length];

        //pick each arg from the params array
        //and create a typed expression of them
        for (var i = 0; i < paramsInfo.Length; i++)
        {
            var index = Expression.Constant(i);
            var paramType = paramsInfo[i].ParameterType;

            var paramAccessorExp =
                Expression.ArrayIndex(param, index);

            argsExp[i] = Expression.Convert(paramAccessorExp, paramType);
        }

        //make a NewExpression that calls the
        //ctor with the args we just created
        var newExp = Expression.New(ctor, argsExp);

        //create a lambda with the New
        //Expression as body and our param object[] as arg
        var lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);

        //compile it
        return (ObjectActivator)lambda.Compile();
    }
}