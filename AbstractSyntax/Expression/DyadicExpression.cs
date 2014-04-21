﻿using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;

namespace AbstractSyntax.Expression
{
    [DebuggerVisualizer(typeof(SyntaxVisualizer))]
    [Serializable]
    public abstract class DyadicExpression : Element
    {
        public Element Left { get; set; }
        public Element Right { get; set; }
        public TokenType Operator { get; set; }

        public override int Count
        {
            get { return 2; }
        }

        public override Element GetChild(int index)
        {
            switch (index)
            {
                case 0: return Left;
                case 1: return Right;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected override string AdditionalInfo()
        {
            return Enum.GetName(typeof(TokenType), Operator);
        }

        internal override void CheckSyntax()
        {
            foreach (Element v in this)
            {
                if (v == null)
                {
                    CompileError("式が必要です。");
                    continue;
                }
            }
            base.CheckSyntax();
        }
    }
}
