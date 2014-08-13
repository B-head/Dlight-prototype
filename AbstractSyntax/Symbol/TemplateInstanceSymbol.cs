﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Symbol
{
    [Serializable]
    public class TemplateInstanceSymbol : Scope
    {
        public OverLoad Template { get; private set; }
        public IReadOnlyList<Scope> Parameter { get; private set; }

        public TemplateInstanceSymbol(Scope baseType, IReadOnlyList<Scope> parameter)
        {
            Template = new OverLoadSimplex(baseType);
            Parameter = parameter;
        }

        public TemplateInstanceSymbol(OverLoad template, IReadOnlyList<Scope> parameter)
        {
            Template = template;
            Parameter = parameter;
        }

        public override bool IsDataType
        {
            get { return true; }
        }

        public override OverLoad OverLoad
        {
            get { return Template; }
        }
    }
}
