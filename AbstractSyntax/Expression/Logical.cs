﻿using AbstractSyntax.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class Logical : DyadicExpression
    {
        public Logical(TextPosition tp, TokenType op, Element left, Element right)
            : base(tp, op, left, right)
        {

        }

        public override TypeSymbol ReturnType
        {
            get { return Left.ReturnType; }
        }

        public override bool IsConstant
        {
            get { return Left.IsConstant && Right.IsConstant; }
        }

        public bool IsOr
        {
            get { return Operator == TokenType.Or; }
        }
    }
}
