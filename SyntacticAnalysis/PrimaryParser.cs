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
            return CoalesceParser(ref c, Group, Number, DeclateClass, DeclateRoutine, DeclateOperator, DeclareVariant, IdentifierAccess);
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
            return new ExpressionGrouping { _Child = exp, Operator = TokenType.Special, Position = position };
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

        private IdentifierAccess IdentifierAccess(ref int c)
        {
            bool pragma = false;
            if (CheckToken(c, TokenType.Pragma))
            {
                pragma = true;
                SkipSpaser(++c);
            }
            if (!CheckToken(c, TokenType.LetterStartString))
            {
                return null;
            }
            Token t = Read(c);
            SkipSpaser(++c);
            return new IdentifierAccess { Value = t.Text, IsPragmaAccess = pragma, Position = t.Position };
        }
    }
}
