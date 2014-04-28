﻿using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public abstract class MonadicExpression : Element
    {
        public Element Child { get; set; }
        public TokenType Operator { get; set; }

        public override int Count
        {
            get { return 1; }
        }

        public override Element this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Child;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override string ElementInfo()
        {
            return Enum.GetName(typeof(TokenType), Operator);
        }

        internal override void CheckSyntax()
        {
            foreach (Element v in this)
            {
                if (v == null)
                {
                    CompileError("require-expression");
                    continue;
                }
            }
            base.CheckSyntax();
        }
    }
}
