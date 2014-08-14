﻿using AbstractSyntax.SpecialSymbol;
using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;
using System.Linq;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class Calculate : DyadicExpression
    {
        private RoutineSymbol _CallScope;

        public Calculate(TextPosition tp, TokenType op, Element left, Element right)
            :base(tp, op, left, right)
        {

        }

        public RoutineSymbol CallScope
        {
            get
            {
                if (_CallScope == null)
                {
                    _CallScope = Root.OpManager.FindDyadic(Operator, Left.ReturnType, Right.ReturnType);
                }
                return _CallScope;
            }
        }

        public override Scope ReturnType
        {
            get { return CallScope.CallReturnType; }
        }

        public override bool IsConstant
        {
            get { return Left.IsConstant && Right.IsConstant && CallScope.IsFunction; }
        }

        internal override void CheckSemantic(CompileMessageManager cmm)
        {
            if (CallScope is ErrorRoutineSymbol)
            {
                cmm.CompileError("impossible-calculate", this);
            }
        }
    }
}
