﻿using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class GroupingExpression : MonadicExpression
    {
        public GroupingExpression(TextPosition tp, Element exp)
            :base(tp, TokenType.Unknoun, exp)
        {

        }

        public override Scope ReturnType
        {
            get { return Exp.ReturnType; }
        }

        public override bool IsConstant
        {
            get { return Exp.IsConstant; }
        }
    }
}
