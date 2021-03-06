﻿using System.Text;
using Cmc;
using Cmc.Core;
using Cmc.Decl;
using Cmc.Expr;
using Cmc.Stmt;
using JetBrains.Annotations;
using static LLVM.GenDeclaration;
using static LLVM.GenExpression;
using static LLVM.GenStatement;

namespace LLVM
{
	public static class GenAstHolder
	{
		/// <summary>
		///  generate llvm ir by the given ast
		/// </summary>
		/// <param name="builder">the string builder used to append ir</param>
		/// <param name="element">the ast element waiting to be generated</param>
		/// <param name="varName">local variable counter</param>
		public static void GenAst(
			[NotNull] StringBuilder builder,
			[NotNull] Ast element,
			ref ulong varName)
		{
			if (element is EmptyStatement) return;
			// convertion
			while (element is Statement statement && statement.ConvertedStatementList != null)
				element = statement.ConvertedStatementList;
			// optimization
			while (element.OptimizedStatementList != null && !Pragma.KeepAll)
				element = element.OptimizedStatementList;
			switch (element)
			{
				case Expression expression:
					GenAstExpression(builder, expression, ref varName);
					break;
				case Declaration declaration:
					GenAstDeclaration(builder, declaration, ref varName);
					break;
				case Statement statement:
					GenAstStatement(builder, statement, ref varName);
					break;
			}
		}
	}
}