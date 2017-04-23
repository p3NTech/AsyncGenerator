﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AsyncGenerator.Extensions
{
	static partial class SyntaxNodeExtensions
	{
		internal static ExpressionSyntax AddAwait(this ExpressionSyntax expression, SyntaxNode parent, ExpressionSyntax configureAwaitArgument)
		{
			var awaitNode = ConvertToAwaitExpression(expression);
			if (configureAwaitArgument != null)
			{
				awaitNode = InvocationExpression(
						MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, awaitNode, IdentifierName("ConfigureAwait")))
					.WithArgumentList(
						ArgumentList(
							SingletonSeparatedList(
								Argument(configureAwaitArgument))
						));
			}

			var nextToken = (parent ?? expression.Parent).ChildNodesAndTokens().FirstOrDefault(o => o.SpanStart >= expression.Span.End); // token can be in a new line
			if (nextToken.IsKind(SyntaxKind.DotToken) || nextToken.IsKind(SyntaxKind.BracketedArgumentList))
			{
				awaitNode = ParenthesizedExpression(awaitNode);
			}

			return awaitNode;
		}

		private static ExpressionSyntax Parenthesize(this ExpressionSyntax expression, bool includeElasticTrivia = true)
		{
			if (includeElasticTrivia)
			{
				return ParenthesizedExpression(expression.WithoutTrivia())
					.WithTriviaFrom(expression)
					.WithAdditionalAnnotations(Simplifier.Annotation);
			}
			return ParenthesizedExpression
				(
					Token(SyntaxTriviaList.Empty, SyntaxKind.OpenParenToken, SyntaxTriviaList.Empty),
					expression.WithoutTrivia(),
					Token(SyntaxTriviaList.Empty, SyntaxKind.CloseParenToken, SyntaxTriviaList.Empty)
				)
				.WithTriviaFrom(expression)
				.WithAdditionalAnnotations(Simplifier.Annotation);
		}

		private static ExpressionSyntax ConvertToAwaitExpression(ExpressionSyntax expression)
		{
			if ((expression is BinaryExpressionSyntax || expression is ConditionalExpressionSyntax) && expression.HasTrailingTrivia)
			{
				var expWithTrailing = expression.WithoutLeadingTrivia();
				var span = expWithTrailing.GetLocation().GetLineSpan().Span;
				if (span.Start.Line == span.End.Line && !expWithTrailing.DescendantTrivia().Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)))
				{
					return AwaitExpression(Token(TriviaList(), SyntaxKind.AwaitKeyword, TriviaList(Space)), ParenthesizedExpression(expWithTrailing))
						.WithLeadingTrivia(expression.GetLeadingTrivia())
						.WithAdditionalAnnotations(Formatter.Annotation);
				}
			}

			return AwaitExpression(Token(TriviaList(), SyntaxKind.AwaitKeyword, TriviaList(Space)), expression.WithoutTrivia().Parenthesize())
				.WithTriviaFrom(expression)
				.WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation);
		}
	}
}