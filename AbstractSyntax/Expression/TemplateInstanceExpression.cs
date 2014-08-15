﻿using AbstractSyntax.Literal;
using AbstractSyntax.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class TemplateInstanceExpression : Element
    {
        public Element Access { get; private set; }
        public TupleLiteral DecParameters { get; private set; }
        private IReadOnlyList<TypeSymbol> _Parameter;

        public TemplateInstanceExpression(TextPosition tp, Element acs, TupleLiteral args)
            : base(tp)
        {
            Access = acs;
            DecParameters = args;
            AppendChild(Access);
            AppendChild(DecParameters);
        }

        public override TypeSymbol ReturnType
        {
            get { return Root.ClassManager.Issue(Access.OverLoad.FindDataType(), Parameter.ToArray()); }
        }

        public override OverLoad OverLoad
        {
            get { return ReturnType.OverLoad; }
        }

        public override bool IsConstant
        {
            get { return Access.IsConstant; }
        }

        public IReadOnlyList<TypeSymbol> Parameter
        {
            get
            {
                if (_Parameter != null)
                {
                    return _Parameter;
                }
                var pt = new List<TypeSymbol>();
                foreach (var v in DecParameters)
                {
                    var temp = v.OverLoad.FindDataType();
                    pt.Add(temp);
                }
                _Parameter = pt;
                return _Parameter;
            }
        }

    }
}
