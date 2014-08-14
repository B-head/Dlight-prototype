﻿using AbstractSyntax.SpecialSymbol;
using AbstractSyntax.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Statement
{
    [Serializable]
    public class UnStatement : Element
    {
        public Element Exp { get; private set; }
        private RoutineSymbol _CallScope;

        public UnStatement(TextPosition tp, Element exp)
            :base(tp)
        {
            Exp = exp;
            AppendChild(Exp);
        }

        public RoutineSymbol CallScope
        {
            get
            {
                if (_CallScope == null)
                {
                    _CallScope = Root.OpManager.FindMonadic(TokenType.Not, Exp.ReturnType);
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
            get { return Exp.IsConstant && CallScope.IsFunction; }
        }

        internal override void CheckSemantic(CompileMessageManager cmm)
        {
            if (CallScope is ErrorRoutineSymbol)
            {
                cmm.CompileError("undefined-monadic-operator", this);
            }
        }
    }
}
