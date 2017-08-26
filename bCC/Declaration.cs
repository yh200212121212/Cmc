﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using static System.StringComparison;

#pragma warning disable 659

namespace bCC
{
	public class Declaration : Statement
	{
		[NotNull] public readonly string Name;

		public Declaration(MetaData metaData, [NotNull] string name) : base(metaData) => Name = name;
	}

	public sealed class VariableDeclaration : Declaration
	{
		[NotNull] public readonly Expression Expression;
		public readonly bool Mutability;
		public Type Type;

		public VariableDeclaration(
			MetaData metaData,
			[NotNull] string name,
			[CanBeNull] Expression expression = null,
			bool isMutable = false,
			[CanBeNull] Type type = null) :
			base(metaData, name)
		{
			Expression = expression ?? new NullExpression(MetaData);
			Type = type;
			Mutability = isMutable;
		}

		public override void SurroundWith(Environment environment)
		{
			base.SurroundWith(environment);
			Expression.SurroundWith(Env);
			var exprType = Expression.GetExpressionType();
			// FEATURE #8
			Type = Type ?? exprType;
			// FEATURE #11
			if (!string.Equals(Type.ToString(), PrimaryType.NullType, Ordinal) && !Equals(Type, exprType))
				// FEATURE #9
				Errors.Add($"{MetaData.GetErrorHeader()}type mismatch, expected: {Type}, actual: {exprType}");
		}

		public override bool Equals(object obj) => obj is Declaration declaration && declaration.Name == Name;

		public override IEnumerable<string> Dump() => new[]
			{
				$"variable declaration [{Name}]:\n",
				"  type:\n"
			}
			.Concat(Type.Dump().Select(MapFunc2))
			.Concat(new[] {"  initialize expression:\n"})
			.Concat(Expression.Dump().Select(MapFunc2));
	}

	public abstract class TypeDeclaration : Declaration
	{
		protected TypeDeclaration(
			MetaData metaData,
			[NotNull] string name)
			: base(metaData, name)
		{
		}

		public abstract Type ToType();
	}

	public class SctructDeclaration : Declaration
	{
		public readonly IList<VariableDeclaration> FieldList;
		public readonly IList<TypeDeclaration> TypeParameters;

		public override void SurroundWith(Environment environment)
		{
			base.SurroundWith(environment);
			var internalEnv = new Environment(Env);
			foreach (var typeDeclaration in TypeParameters) internalEnv.Declarations.Add(typeDeclaration);
			foreach (var variableDeclaration in FieldList)
			{
				variableDeclaration.SurroundWith(Env);
//				if (variableDeclaration.Type)
			}
		}

		public SctructDeclaration(
			MetaData metaData,
			[NotNull] string name,
			[NotNull] IList<VariableDeclaration> fieldList,
			[NotNull] IList<TypeDeclaration> typeParameters) :
			base(metaData, name)
		{
			TypeParameters = typeParameters;
			FieldList = fieldList;
		}
	}

	/// <summary>
	///   Probably useless
	/// </summary>
	public class Macro : Declaration
	{
		[NotNull] public string Content;

		public Macro(MetaData metaData, [NotNull] string name, [NotNull] string content) : base(metaData, name) =>
			Content = content;

		public override IEnumerable<string> Dump() => new[] {"macro(this shouldn't appear)\n"};
	}
}