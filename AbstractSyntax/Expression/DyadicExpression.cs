﻿/*
Copyright 2014 B_head

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public abstract class DyadicExpression : Element
    {
        public TokenType Operator { get; private set; }
        public Element Left { get; private set; }
        public Element Right { get; private set; }

        protected DyadicExpression(TextPosition tp, TokenType op, Element left, Element right)
            :base(tp)
        {
            Operator = op;
            Left = left;
            Right = right;
            AppendChild(Left);
            AppendChild(Right);
        }

        protected override string ElementInfo
        {
            get { return Operator.ToString(); }
        }

        internal override void CheckSemantic(CompileMessageManager cmm)
        {
            if (Left == null)
            {
                cmm.CompileError("require-expression", this);
            }
            if (Right == null)
            {
                cmm.CompileError("require-expression", this);
            }
        }
    }
}
