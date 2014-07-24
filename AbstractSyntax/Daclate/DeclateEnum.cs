﻿using AbstractSyntax.Directive;
using AbstractSyntax.Expression;
using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;

namespace AbstractSyntax.Daclate
{
    [Serializable]
    public class DeclateEnum : EnumSymbol
    {
        public DeclateEnum(TextPosition tp, string name, DirectiveList block)
            :base(tp, name, block)
        {

        }
    }
}
