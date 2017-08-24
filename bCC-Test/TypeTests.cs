﻿using System;
using System.Linq;
using bCC;
using NUnit.Framework;
using Environment = bCC.Environment;

namespace bCC_Test
{
	[TestFixture]
	public class TypeTests
	{
		[SetUp]
		public void Init() => Errors.ErrList.Clear();

		[Test]
		public void TypeTest1()
		{
			LiteralExpression example = new IntLiteralExpression(MetaData.Empty, "123456789", true, 64);
			example.PrintDumpInfo();
			Assert.AreEqual("i64", example.Type.Name);
			example = new IntLiteralExpression(MetaData.Empty, "123456789", true);
			example.PrintDumpInfo();
			Assert.AreEqual("i32", example.Type.Name);
			example = new IntLiteralExpression(MetaData.Empty, "123456789", false, 64);
			example.PrintDumpInfo();
			Assert.AreEqual("u64", example.Type.Name);
			example = new IntLiteralExpression(MetaData.Empty, "123456789", false, 64);
			example.PrintDumpInfo();
			Assert.AreEqual("u64", example.Type.Name);
			example = new StringLiteralExpression(MetaData.Empty, "\"boy \\ next \\ door\n\t\"");
			example.PrintDumpInfo();
			Assert.AreEqual("string", example.Type.Name);
		}

		/// <summary>
		/// var someVar = 123u8;
		/// someVar; // the type of this expression will be inferred as "u8".
		/// </summary>
		[Test]
		public void TypeInferenceTest1()
		{
			const string varName = "someVar";
			var example = new StatementList(MetaData.Empty,
				new VariableDeclaration(MetaData.Empty, varName,
					new IntLiteralExpression(MetaData.Empty, "123", false, 8)),
				new ExpressionStatement(MetaData.Empty, new VariableExpression(MetaData.Empty, varName)));
			example.SurroundWith(new Environment());
			example.PrintDumpInfo();
			// ReSharper disable once PossibleNullReferenceException
			Assert.AreEqual("u8", (example.Statements.Last() as ExpressionStatement).Expression.GetExpressionType().ToString());
		}

		/// <summary>
		/// var someVar = null;
		/// someVar; // nulltype
		/// </summary>
		[Test]
		public void TypeInferenceTest2()
		{
			const string varName = "someOtherVar";
			var example = new StatementList(MetaData.Empty,
				new VariableDeclaration(MetaData.Empty, varName,
					new NullExpression(MetaData.Empty)),
				new ExpressionStatement(MetaData.Empty, new VariableExpression(MetaData.Empty, varName)));
			example.SurroundWith(new Environment());
			example.PrintDumpInfo();
			// ReSharper disable once PossibleNullReferenceException
			Assert.AreEqual(NullExpression.NullType,
				(example.Statements.Last() as ExpressionStatement).Expression.GetExpressionType().ToString());
		}

		/// <summary>
		/// var otherVar: i8 = null;
		/// otherVar; // i8
		/// FEATURE #11
		/// </summary>
		[Test]
		public void TypeInferenceTest3()
		{
			const string varName = "otherVar";
			var example = new StatementList(MetaData.Empty,
				new VariableDeclaration(MetaData.Empty, varName,
					new NullExpression(MetaData.Empty),
					type: new PrimaryType(MetaData.Empty, "i8")),
				new ExpressionStatement(MetaData.Empty, new VariableExpression(MetaData.Empty, varName)));
			example.SurroundWith(new Environment());
			Console.WriteLine(string.Join("", example.Dump()));
			// ReSharper disable once PossibleNullReferenceException
			Assert.AreEqual("i8", (example.Statements.Last() as ExpressionStatement).Expression.GetExpressionType().ToString());
		}

		/// <summary>
		/// lambda type inference
		/// </summary>
		[Test]
		public void TypeInferenceTest4()
		{
			const string variable = "variable";
			var example = new VariableDeclaration(MetaData.Empty, variable,
				new LambdaExpression(MetaData.Empty,
					new StatementList(MetaData.Empty,
						new ReturnStatement(MetaData.Empty, new IntLiteralExpression(MetaData.Empty, "0", true)))));
			example.SurroundWith(new Environment());
			example.PrintDumpInfo();
			var type = (LambdaType) example.Type;
			Assert.AreEqual("i32", type.RetType.Name);
			Assert.IsEmpty(type.ArgsList);
		}
	}
}