using Furesoft.Core.CodeDom.Compiler.Analysis;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using System;
using System.Linq;
using static Furesoft.Core.CodeDom.Compiler.Core.ExceptionSpecification;
using static Furesoft.Core.CodeDom.Compiler.Instructions.ArrayIntrinsics;

namespace Furesoft.Core.CodeDom.Backends.CLR.Analysis
{
    /// <summary>
    /// Exception specifications for prototypes that rely on
    /// CIL's semantics.
    /// </summary>
    public static class CilPrototypeExceptionSpecs
    {
        /// <summary>
        /// Creates CIL prototype exception specification rules.
        /// </summary>
        /// <param name="corlibTypeResolver">
        /// A type resolver for the core library (corlib.dll) that
        /// defines well-known exception types.
        /// </param>
        /// <returns>Prototype exception specification rules.</returns>
        public static RuleBasedPrototypeExceptionSpecs Create(
            ReadOnlyTypeResolver corlibTypeResolver)
        {
            var result = new RuleBasedPrototypeExceptionSpecs(
                RuleBasedPrototypeExceptionSpecs.Default);

            // We know the type of exception thrown by null checks:
            // it's System.NullReferenceException.
            var nullRefException = corlibTypeResolver.ResolveTypes(
                new SimpleName(nameof(NullReferenceException))
                .Qualify(nameof(System)))
                .Single();

            // Resolve other well-known exception types.
            var outOfRangeException = corlibTypeResolver.ResolveTypes(
                new SimpleName(nameof(IndexOutOfRangeException))
                .Qualify(nameof(System)))
                .Single();

            var arrayTypeMismatchException = corlibTypeResolver.ResolveTypes(
                new SimpleName(nameof(ArrayTypeMismatchException))
                .Qualify(nameof(System)))
                .Single();

            // Refine null check exception types.
            result.Register<GetFieldPointerPrototype>(
                new NullCheckExceptionSpecification(0, nullRefException));
            result.Register<NewDelegatePrototype>(
                proto => proto.Lookup == MethodLookup.Virtual
                    ? new NullCheckExceptionSpecification(0, nullRefException)
                    : ExceptionSpecification.NoThrow);
            result.Register<UnboxPrototype>(
                new NullCheckExceptionSpecification(0, nullRefException));

            // Array intrinsics are worth refining, too.
            result.Register(
                ArrayIntrinsics.Namespace.GetIntrinsicName(Operators.GetElementPointer),
Union.Create(
                    new NullCheckExceptionSpecification(0, nullRefException),
Exact.Create(outOfRangeException),
Exact.Create(arrayTypeMismatchException)));
            result.Register(
                ArrayIntrinsics.Namespace.GetIntrinsicName(Operators.LoadElement),
Union.Create(
                    new NullCheckExceptionSpecification(0, nullRefException),
Exact.Create(outOfRangeException)));
            result.Register(
                ArrayIntrinsics.Namespace.GetIntrinsicName(Operators.StoreElement),
Union.Create(
                    new NullCheckExceptionSpecification(0, nullRefException),
Exact.Create(outOfRangeException),
Exact.Create(arrayTypeMismatchException)));
            result.Register(
                ArrayIntrinsics.Namespace.GetIntrinsicName(Operators.GetLength),
                new NullCheckExceptionSpecification(0, nullRefException));

            return result;
        }
    }
}