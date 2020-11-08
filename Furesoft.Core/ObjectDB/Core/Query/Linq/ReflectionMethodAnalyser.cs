using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Furesoft.Core.ObjectDB.Exceptions;
using Furesoft.Core.ObjectDB.Reflection;
using Furesoft.Core.ObjectDB.Tool;

namespace Furesoft.Core.ObjectDB.Core.Query.Linq
{
	internal sealed class ReflectionMethodAnalyser
	{
		private static readonly Dictionary<MethodInfo, FieldInfo> fieldCache =
			new Dictionary<MethodInfo, FieldInfo>();

		private static ILPattern BackingField()
		{
			return new BackingFieldPattern();
		}

		private sealed class BackingFieldPattern : ILPattern
		{
			public static readonly object BackingFieldKey = new object();

			private static readonly ILPattern pattern = Sequence(Optional(OpCodes.Nop),
																 OpCode(OpCodes.Ldarg_0),
																 OpCode(OpCodes.Ldfld));

			internal override void Match(MatchContext context)
			{
				pattern.Match(context);
				if (!context.IsMatch)
					return;

				var match = GetLastMatchingInstruction(context);
				var field = (FieldInfo)match.Operand;
				context.AddData(BackingFieldKey, field);
			}
		}

		private static readonly ILPattern getterPattern =
			ILPattern.Sequence(
				BackingField(),
				ILPattern.Optional(
					OpCodes.Stloc_0,
					OpCodes.Br_S,
					OpCodes.Ldloc_0),
				ILPattern.OpCode(OpCodes.Ret));

		private readonly MethodInfo _method;

		public ReflectionMethodAnalyser(MethodInfo method)
		{
			_method = method;
		}

		private static MatchContext MatchGetter(MethodInfo method)
		{
			return ILPattern.Match(method, getterPattern);
		}

		public void Run(QueryBuilderRecorder recorder)
		{
			RecordField(recorder, GetBackingField(_method));
		}

		private static void RecordField(QueryBuilderRecorder recorder, FieldInfo field)
		{
			recorder.Add(ctx =>
							 {
								 ctx.Descend(field.Name);
								 ctx.PushDescendigFieldEnumType(field.FieldType.IsEnum ? field.FieldType : null);
							 });
		}

		private static FieldInfo GetBackingField(MethodInfo method)
		{
			return fieldCache.GetOrAdd(method, ResolveBackingField);
		}

		private static FieldInfo ResolveBackingField(MethodInfo method)
		{
			var context = MatchGetter(method);
			if (!context.IsMatch)
				throw new LinqQueryException("Analysed method is not a simple getter");

			return GetFieldFromContext(context);
		}

		private static FieldInfo GetFieldFromContext(MatchContext context)
		{
			if (!context.TryGetData(BackingFieldPattern.BackingFieldKey, out var data))
				throw new NotSupportedException();

			return (FieldInfo)data;
		}
	}
}