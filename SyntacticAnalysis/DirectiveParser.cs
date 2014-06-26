﻿using AbstractSyntax;
using AbstractSyntax.Directive;
using AbstractSyntax.Expression;
using System;

namespace SyntacticAnalysis
{
    public partial class Parser
    {
        private static DirectiveList RootDirectiveList(ChainParser cp)
        {
            var result = cp.Begin<DirectiveList>()
                .Ignore(TokenType.EndExpression, TokenType.LineTerminator)
                .Loop().Readble().Do()
                .Transfer((s, e) => s.Append(e), Directive)
                .Or().Error()
                .Ignore(TokenType.EndExpression, TokenType.LineTerminator)
                .EndLoop().End();
            if(result == null)
            {
                throw new InvalidOperationException();
            }
            return result;
        }

        private static DirectiveList PureInlineDirectiveList(ChainParser cp)
        {
            return cp.Begin<DirectiveList>()
                .Transfer((s, e) => s.Append(e), Directive)
                .Self(s => s.IsInline = true)
                .End();
        }

        private static DirectiveList InlineDirectiveList(ChainParser cp)
        {
            return cp.Begin<DirectiveList>()
                .Type(TokenType.Separator).Or().Text("do").Lt()
                .Transfer((s, e) => s.Append(e), Directive)
                .Self(s => s.IsInline = true)
                .End();
        }

        private static DirectiveList IfInlineDirectiveList(ChainParser cp)
        {
            return cp.Begin<DirectiveList>()
                .Type(TokenType.Separator).Or().Text("do").Or().Text("than").Lt()
                .Transfer((s, e) => s.Append(e), Directive)
                .Self(s => s.IsInline = true)
                .End();
        }

        private static DirectiveList BlockDirectiveList(ChainParser cp)
        {
            var result = cp.Begin<DirectiveList>()
                .Type(TokenType.LeftBrace)
                .Ignore(TokenType.EndExpression, TokenType.LineTerminator)
                .Loop().Not().Type(TokenType.RightBrace).Do()
                .Transfer((s, e) => s.Append(e), Directive)
                .Or().Error()
                .Ignore(TokenType.EndExpression, TokenType.LineTerminator)
                .EndLoop().End();
            if (result == null)
            {
                result = new DirectiveList();
            }
            return result;
        }

        private static DirectiveList Block(ChainParser cp)
        {
            return CoalesceParser<DirectiveList>(cp, InlineDirectiveList, BlockDirectiveList);
        }

        private static Element Directive(ChainParser cp)
        {
            return CoalesceParser(cp, directive);
        }

        private static AttributeZoneDirective AttributeZone(ChainParser cp)
        {
            return cp.Begin<AttributeZoneDirective>()
                .Type(TokenType.Wild).Loop()
                .Text((s, e) => s.Append(TextToIdentifier(cp, e.Text)))
                .Type(TokenType.List)
                .Do().EndLoop().End();
        }

        private static EchoDirective Echo(ChainParser cp)
        {
            return cp.Begin<EchoDirective>()
                .Text("echo").Lt()
                .Opt().Transfer((s, e) => s.Exp = e, Directive)
                .End();
        }

        private static AliasDirective Alias(ChainParser cp)
        {
            return cp.Begin<AliasDirective>()
                .Text("alias").Lt()
                .Transfer((s, e) => s.From = e, IdentifierAccess)
                .Transfer((s, e) => s.To = e, IdentifierAccess)
                .End();
        }

        private static ReturnDirective Return(ChainParser cp)
        {
            return cp.Begin<ReturnDirective>()
                .Text("return").Lt()
                .Transfer((s, e) => s.Exp = e, Directive)
                .End();
        }

        private static BreakDirective Break(ChainParser cp)
        {
            return cp.Begin<BreakDirective>()
                .Text("break").Lt()
                .End();
        }

        private static ContinueDirective Continue(ChainParser cp)
        {
            return cp.Begin<ContinueDirective>()
                .Text("continue").Lt()
                .End();
        }
    }
}
