﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractSyntax;
using Common;

namespace SyntacticAnalysis
{
    public partial class Parser
    {
        private Element Primary(ref int c)
        {
            return CoalesceParser(ref c, Group, Number, DeclateRoutine, DeclareVariant, Identifier);
        }

        private ExpressionGrouping Group(ref int c)
        {
            int temp = c;
            if (!CheckToken(temp, TokenType.LeftParenthesis))
            {
                return null;
            }
            TextPosition position = Read(temp).Position;
            SkipSpaser(++temp);
            Element exp = DirectiveList(ref temp);
            if (!CheckToken(temp, TokenType.RightParenthesis))
            {
                return null;
            }
            SkipSpaser(++temp);
            c = temp;
            return new ExpressionGrouping { _Child = exp, Operation = TokenType.Special, Position = position };
        }

        private NumberLiteral Number(ref int c)
        {
            int temp = c;
            string value = string.Empty;
            if (!CheckToken(temp, TokenType.DigitStartString))
            {
                return null;
            }
            Token i = Read(temp);
            SkipSpaser(++temp);
            c = temp;
            if (CheckToken(temp, TokenType.Access))
            {
                SkipSpaser(++temp);
                if (CheckToken(temp, TokenType.DigitStartString))
                {
                    Token f = Read(temp);
                    SkipSpaser(++temp);
                    c = temp;
                    return new NumberLiteral { Integral = i.Text, Fraction = f.Text, Position = i.Position };
                }
            }
            return new NumberLiteral { Integral = i.Text, Position = i.Position };
        }

        private Identifier Identifier(ref int c)
        {
            if (!CheckToken(c, TokenType.LetterStartString))
            {
                return null;
            }
            Token t = Read(c);
            SkipSpaser(++c);
            return new Identifier { Value = t.Text, Position = t.Position };
        }

        private DeclateVariant DeclareVariant(ref int c)
        {
            if (!CheckText(c, "var", "variant"))
            {
                return null;
            }
            SkipSpaser(++c);
            Identifier ident = Identifier(ref c);
            Element explType = null;
            if (CheckToken(c, TokenType.Peir))
            {
                SkipSpaser(++c);
                explType = MemberAccess(ref c);
            }
            return new DeclateVariant { Ident = ident, ExplicitVariantType = explType, Position = ident.Position };
        }

        private DeclateRoutine DeclateRoutine(ref int c)
        {
            if (!CheckText(c, "rout", "routine"))
            {
                return null;
            }
            SkipSpaser(++c);
            Identifier ident = Identifier(ref c);
            Element attr = null;
            Element retType = null;
            Element block = null;
            if (CheckToken(c, TokenType.LeftParenthesis))
            {
                SkipSpaser(++c);
                attr = ParseTuple(ref c, DeclateArgument);
                if (CheckToken(c, TokenType.RightParenthesis))
                {
                    SkipSpaser(++c);
                }
            }
            if (CheckToken(c, TokenType.Peir))
            {
                SkipSpaser(++c);
                retType = MemberAccess(ref c);
            }
            block = Block(ref c);
            return new DeclateRoutine { Ident = ident, Argument = attr, ExplicitResultType = retType, Block = block, Position = ident.Position };
        }

        private DeclateArgument DeclateArgument(ref int c)
        {
            Identifier ident = Identifier(ref c);
            Element explType = null;
            if (CheckToken(c, TokenType.Peir))
            {
                SkipSpaser(++c);
                explType = MemberAccess(ref c);
            }
            return new DeclateArgument { Ident = ident, ExplicitArgumentType = explType, Position = ident.Position };
        }

        private Element Block(ref int c)
        {
            Element result = null;
            if (CheckToken(c, TokenType.Separator))
            {
                SkipSpaser(++c);
                result = Directive(ref c);
            }
            else if (CheckToken(c, TokenType.LeftBrace))
            {
                SkipSpaser(++c);
                result = DirectiveList(ref c);
                if (CheckToken(c, TokenType.RightBrace))
                {
                    SkipSpaser(++c);
                }
            }
            return result;
        }
    }
}
