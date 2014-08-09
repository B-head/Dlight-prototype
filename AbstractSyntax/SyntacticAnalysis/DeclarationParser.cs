﻿using AbstractSyntax;
using AbstractSyntax.Declaration;
using AbstractSyntax.Expression;
using System.Collections.Generic;

namespace AbstractSyntax.SyntacticAnalysis
{
    public partial class Parser
    {
        private static string[] attribute = 
        { 
            "final",
            "static",
            "public",
            "internal",
            "protected",
            "private",
        };

        private static AliasDeclaration AliasDeclaration(SlimChainParser cp)
        {
            Identifier from = null;
            Identifier to = null;
            return cp.Begin
                .Text("alias").Lt()
                .Opt.Transfer(e => from = e, Identifier)
                .Opt.Transfer(e => to = e, Identifier)
                .End(tp => new AliasDeclaration(tp, from, to));
        }

        private static VariantDeclaration VariantDeclaration(SlimChainParser cp)
        {
            TupleList attr = null;
            var isLet = false;
            Identifier ident = null;
            Identifier expl = null;
            return cp.Begin
                .Transfer(e => attr = e, AttributeList)
                .Any(
                    icp => icp.Text(e => isLet = false, "var"),
                    icp => icp.Text(e => isLet = true, "let")
                ).Lt()
                .Transfer(e => ident = e, Identifier)
                .If(icp => icp.Type(TokenType.Peir).Lt())
                .Than(icp => icp.Transfer(e => expl = e, Identifier))
                .End(tp => new VariantDeclaration(tp, attr, ident, expl, isLet));
        }

        private static RoutineDeclaration RoutineDeclaration(SlimChainParser cp)
        {
            TupleList attr = null;
            var isFunc = false;
            var name = string.Empty;
            TupleList generic = null;
            TupleList args = null;
            Element expl = null;
            ExpressionList block = null;
            return cp.Begin
                .Transfer(e => attr = e, AttributeList)
                .Any(
                    icp => icp.Text(e => isFunc = false, "rout", "routine"),
                    icp => icp.Text(e => isFunc = true, "func", "function")
                ).Lt()
                .Type(t => name = t.Text, TokenType.LetterStartString).Lt()
                .Transfer(e => generic = e, GenericList)
                .Transfer(e => args = e, ArgumentList)
                .If(icp => icp.Type(TokenType.Peir).Lt())
                .Than(icp => icp.Transfer(e => expl = e, NonTupleExpression))
                .Transfer(e => block = e, icp => Block(icp, true))
                .End(tp => new RoutineDeclaration(tp, name, TokenType.Unknoun, isFunc, attr, generic, args, expl, block));
        }

        private static RoutineDeclaration OperatorDeclaration(SlimChainParser cp)
        {
            TupleList attr = null;
            var op = TokenType.Unknoun;
            var name = string.Empty;
            TupleList generic = null;
            TupleList args = null;
            Element expl = null;
            ExpressionList block = null;
            return cp.Begin
                .Transfer(e => attr = e, AttributeList)
                .Text("operator").Lt()
                .Take(t => { op = t.TokenType; name = t.Text; }).Lt()
                .Transfer(e => generic = e, GenericList)
                .Transfer(e => args = e, ArgumentList)
                .If(icp => icp.Type(TokenType.Peir).Lt())
                .Than(icp => icp.Transfer(e => expl = e, NonTupleExpression))
                .Transfer(e => block = e, icp => Block(icp, true))
                .End(tp => new RoutineDeclaration(tp, name, op, false, attr, generic, args, expl, block));
        }

        private static ClassDeclaration ClassDeclaration(SlimChainParser cp)
        {
            TupleList attr = null;
            var isTrait = false;
            var name = string.Empty;
            TupleList generic = null;
            TupleList inherit = new TupleList();
            ExpressionList block = null;
            return cp.Begin
                .Transfer(e => attr = e, AttributeList)
                .Any(
                    icp => icp.Text(e => isTrait = false, "class"),
                    icp => icp.Text(e => isTrait = true, "trait")
                ).Lt()
                .Type(t => name = t.Text, TokenType.LetterStartString).Lt()
                .Transfer(e => generic = e, GenericList)
                .If(icp => icp.Type(TokenType.Peir).Lt())
                .Than(icp => icp.Transfer(e => inherit = e, c => ParseTuple(c, Identifier)))
                .Transfer(e => block = e, icp => Block(icp, true))
                .End(tp => new ClassDeclaration(tp, name, isTrait, attr, generic, inherit, block));
        }

        private static TupleList AttributeList(SlimChainParser cp)
        {
            var child = new List<Element>();
            var atFlag = false;
            var ret = cp.Begin
                .Loop(icp =>
                {
                    icp
                    .If(iicp => iicp.Type(TokenType.Attribute).Lt())
                    .Than(iicp => { atFlag = true; iicp.Transfer(e => child.Add(e), Identifier); })
                    .ElseIf(iicp => iicp.Is(atFlag).Type(TokenType.List).Lt())
                    .Than(iicp => { atFlag = true; iicp.Transfer(e => child.Add(e), Identifier); })
                    .Else(iicp => { atFlag = false; iicp.Transfer(e => child.Add(e), iiicp => Identifier(iiicp, attribute)); });
                })
                .End(tp => new TupleList(tp, child));
            return ret ?? new TupleList();
        }

        private static TupleList GenericList(SlimChainParser cp)
        {
            var child = new List<Element>();
            var ret = cp.Begin
                .Type(TokenType.Template).Lt()
                .Type(TokenType.LeftParenthesis).Lt()
                .Loop(icp =>
                {
                    icp
                    .Transfer(e => child.Add(e), GenericDeclaration)
                    .Type(TokenType.List).Lt();
                })
                .Type(TokenType.RightParenthesis).Lt()
                .End(tp => new TupleList(tp, child));
            return ret ?? new TupleList();
        }

        private static GenericDeclaration GenericDeclaration(SlimChainParser cp)
        {
            var name = string.Empty;
            Element special = null;
            return cp.Begin
                .Type(t => name = t.Text, TokenType.LetterStartString)
                .If(icp => icp.Type(TokenType.Peir).Lt())
                .Than(icp => icp.Transfer(e => special = e, NonTupleExpression))
                .End(tp => new GenericDeclaration(tp, name, special));
        }

        private static TupleList ArgumentList(SlimChainParser cp)
        {
            var child = new List<Element>();
            var ret = cp.Begin
                .Type(TokenType.LeftParenthesis).Lt()
                .Loop(icp =>
                {
                    icp
                    .Transfer(e => child.Add(e), ArgumentDeclaration)
                    .Type(TokenType.List).Lt();
                })
                .Type(TokenType.RightParenthesis).Lt()
                .End(tp => new TupleList(tp, child));
            return ret ?? new TupleList();
        }

        //todo デフォルト引数に対応した専用の構文が必要。
        private static ArgumentDeclaration ArgumentDeclaration(SlimChainParser cp)
        {
            TupleList attr = null;
            Identifier ident = null;
            Identifier expl = null;
            return cp.Begin
                .Transfer(e => attr = e, AttributeList)
                .Transfer(e => ident = e, Identifier)
                .If(icp => icp.Type(TokenType.Peir).Lt())
                .Than(icp => icp.Transfer(e => expl = e, Identifier))
                .End(tp => new ArgumentDeclaration(tp, attr, ident, expl));
        }

        private static AttributeScope AttributeScope(SlimChainParser cp)
        {
            var child = new List<Element>();
            return cp.Begin
                .Type(TokenType.Zone).Lt()
                .Loop(icp =>
                {
                    icp
                    .Transfer(e => child.Add(e), Identifier)
                    .Type(TokenType.List).Lt();
                })
                .End(tp => new AttributeScope(tp, child));
        }
    }
}
