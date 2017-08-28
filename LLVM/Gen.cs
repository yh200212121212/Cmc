﻿using System.IO;
using System.Text;
using bCC;
using bCC.Core;
using bCC.Expression;
using bCC.Statement;
using Tools;

namespace LLVM
{
	public static class Gen
	{
		public static ulong GlobalVarCount = 0;

		public static void Generate(
			string outputFile,
			params Declaration[] declarations)
		{
			var core = new Core();
			var builder = new StringBuilder();
			foreach (var analyzedDeclaration in core.Analyze(declarations))
				GenAst(builder, analyzedDeclaration, true);
			File.WriteAllText(outputFile, builder.ToString());
			CommandLine.RunCommand($"llc-4.0 {outputFile}.ll -filetype=obj");
			CommandLine.RunCommand($"gcc {outputFile}.ll -o {outputFile}");
		}

		/// <summary>
		///  C# cannot have a tenary operator in the string templete
		///  So I made this
		/// </summary>
		/// <param name="isGlobal">is it a global declaration</param>
		/// <returns>the prefix of the declaration</returns>
		public static char DetermineDeclarationPrefix(bool isGlobal) => isGlobal ? '@' : '%';

		/// <summary>
		///  generate llvm ir by the given ast
		/// </summary>
		/// <param name="builder">the string builder used to append ir</param>
		/// <param name="element">the ast element waiting to be generated</param>
		/// <param name="isGlobal">is it a global declaration?</param>
		public static void GenAst(
			StringBuilder builder,
			Ast element,
			bool isGlobal = false)
		{
			if (element is TypeDeclaration typeDeclaration)
				builder.AppendLine($"; type alias: <{typeDeclaration.Name}> -> <{typeDeclaration.Type}>");
			else if (element is VariableDeclaration variable)
			{
				var varName = $"{DetermineDeclarationPrefix(isGlobal)}{GlobalVarCount++}";
				var expr = variable.Expression;
				if (expr is StringLiteralExpression str)
				{
					builder.AppendLine(
						$"{varName}=unnamed_addr constant [{str.Value.Length} x i8] c\"{str.Value}\"");
				}
				else if (expr is IntLiteralExpression integer)
				{
//					builder.AppendLine(
//						$"{varName}={}");
				}
			}
			else if(element is ReturnStatement returnStatement)
			{
				var expr = returnStatement.Expression;
				GenAst(builder, expr);
				// TODO assign the value
			}
		}

		public static void Main(string[] args)
		{
			Generate(
				"out"
			);
		}
	}
}