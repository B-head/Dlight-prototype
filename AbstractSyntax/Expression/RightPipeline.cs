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
using AbstractSyntax.Declaration;
using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class RightPipeline : CallExpression
    {
        public TokenType Operator { get; set; }

        public RightPipeline(TextPosition tp, TokenType op, Element left, Element right)
            :base(tp, right, left)
        {
            Operator = op;
        }

        public override Element Right
        {
            get
            {
                return Access;
            }
        }

        public override Element Left
        {
            get
            {
                return (Element)Arguments[0];
            }
        }

        public override TokenType CalculateOperator
        {
            get { return Operator ^ TokenType.RightPipeline; }
        }

        protected override string ElementInfo
        {
            get { return Operator.ToString(); }
        }

        public override bool IsPipeline
        {
            get { return true; }
        }
    }
}
