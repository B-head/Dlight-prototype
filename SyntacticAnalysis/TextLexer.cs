﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractSyntax;

namespace SyntacticAnalysis
{
    public partial class Lexer
    {
        private Token LineTerminator(Tokenizer t)
        {
            int i = 0;
            if (t.IsReadable(i) && t.MatchAny(i, "\x0A"))
            {
                i++;
                if (t.IsReadable(i) && t.MatchAny(i, "\x0D"))
                {
                    i++;
                }
            }
            else if (t.IsReadable(i) && t.MatchAny(i, "\x0D"))
            {
                i++;
                if (t.IsReadable(i) && t.MatchAny(i, "\x0A"))
                {
                    i++;
                }
            }
            return t.TakeToken(i, TokenType.LineTerminator);
        }

        private Token WhiteSpace(Tokenizer t)
        {
            int i;
            for (i = 0; t.IsReadable(i); i++)
            {
                if (t.MatchRange(i, '\x00', '\x20') || t.MatchAny(i, "\x7F"))
                {
                    continue;
                }
                break;
            }
            return t.TakeToken(i, TokenType.WhiteSpace);
        }

        private Token BlockComment(Tokenizer t)
        {
            int i = 0, nest = 1;
            if(!t.IsReadable(i + 1) || t.Read(i, 2) != "/*")
            {
                return null;
            }
            for (i = 2; !t.IsReadable(i + 1); i++)
            {
                if (t.Read(i, 2) == "*/")
                {
                    ++i;
                    if (--nest == 0)
                    {
                        break;
                    }
                }
                if (t.Read(i, 2) == "/*")
                {
                    ++i;
                    ++nest;
                }
            }
            return t.TakeToken(++i, TokenType.BlockComment);
        }

        private Token LineCommnet(Tokenizer t)
        {
            int i = 0;
            if(!t.IsReadable(i + 1))
            {
                return null;
            }
            if (t.Read(i, 2) != "//" && t.Read(i, 2) != "#!")
            {
                return null;
            }
            for (i = 2; !t.IsReadable(i); i++)
            {
                if (t.MatchAny(i, "\x0A\x0D"))
                {
                    break;
                }
            }
            return t.TakeToken(++i, TokenType.LineCommnet);
        }

        private bool StringLiteral(Tokenizer t, List<Token> tokenList, List<Token> errorToken)
        {
            if (!t.IsReadable(0) || !t.MatchAny(0, "\'\"`"))
            {
                return false;
            }
            string quote = t.Read(0, 1);
            tokenList.Add(t.TakeToken(1, TokenType.QuoteSeparator));
            int i;
            for (i = 0; t.IsReadable(i); i++)
            {
                if (t.MatchAny(i, quote))
                {
                    tokenList.Add(t.TakeToken(i, TokenType.PlainText));
                    tokenList.Add(t.TakeToken(1, TokenType.QuoteSeparator));
                    return true;
                }
                if (t.MatchAny(i, "{"))
                {
                    tokenList.Add(t.TakeToken(i, TokenType.PlainText));
                    BuiltInExpression(t, tokenList, errorToken);
                    i = -1;
                }
            }
            tokenList.Add(t.TakeToken(i, TokenType.PlainText));
            return true;
        }

        private void BuiltInExpression(Tokenizer t, List<Token> tokenList, List<Token> errorToken)
        {
            if (!t.IsReadable(0) || !t.MatchAny(0, "{"))
            {
                return;
            }
            var result = new List<Token>();
            int nest = 0;
            while (t.IsReadable())
            {
                var tt = LexPartion(t, tokenList, errorToken);
                if (tt == TokenType.LeftBrace)
                {
                    ++nest;
                }
                if (tt == TokenType.RightBrace)
                {
                    if(--nest == 0)
                    {
                        break;
                    }
                }
            }
        }

        private Token LetterStartString(Tokenizer t)
        {
            int i;
            bool escape = false;
            for (i = 0; t.IsReadable(i); i++)
            {
                if (escape && t.MatchRange(i, '!', '~'))
                {
                    escape = false;
                    continue;
                }
                if (t.MatchRange(i, 'a', 'z') || t.MatchRange(i, 'A', 'Z') || t.MatchAny(i, "_"))
                {
                    continue;
                }
                if (i > 0 && t.MatchRange(i, '0', '9'))
                {
                    continue;
                }
                if (t.MatchAny(i, "\\"))
                {
                    escape = true;
                    continue;
                }
                break;
            }
            return t.TakeToken(i, TokenType.LetterStartString);
        }

        private Token DigitStartString(Tokenizer t)
        {
            int i;
            bool escape = false;
            for (i = 0; t.IsReadable(i); i++)
            {
                if (escape && t.MatchRange(i, '!', '~'))
                {
                    escape = false;
                    continue;
                }
                if t.MatchRange(i, '0', '9') || t.MatchAny(i, "_"))
                {
                    continue;
                }
                if (i > 0 && (t.MatchRange(i, 'a', 'z') || t.MatchRange(i, 'A', 'Z')))
                {
                    continue;
                }
                if (i > 0 && t.MatchAny(i, "\\"))
                {
                    escape = true;
                    continue;
                }
                break;
            }
            return t.TakeToken(i, TokenType.DigitStartString);
        }

        private Token OtherString(Tokenizer t)
        {
            int i;
            for (i = 0; t.IsReadable(i); i++)
            {
                if (!t.MatchRange(i, '\x00', '\x7F'))
                {
                    continue;
                }
                break;
            }
            return t.TakeToken(i, TokenType.OtherString);
        }
    }
}
