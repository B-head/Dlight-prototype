﻿using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;
using System.Linq;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class Calculate : DyadicExpression
    {
        private Scope _CallScope;

        public Calculate(TextPosition tp, TokenType op, Element left, Element right)
            :base(tp, op, left, right)
        {

        }

        public Scope CallScope
        {
            get
            {
                if (_CallScope == null)
                {
                    _CallScope = Root.OpManager.Find(Operator, Right.ReturnType, Left.ReturnType);
                }
                return _CallScope;
            }
        }

        public override Scope ReturnType
        {
            get { return CallScope.CallReturnType; }
        }

        internal override void CheckSemantic(CompileMessageManager cmm)
        {
            if (CallScope is ErrorSymbol)
            {
                cmm.CompileError("impossible-calculate", this);
            }
        }
    }
}
