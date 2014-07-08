﻿using System;
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
        public TupleList DecParameters { get; private set; }
        private IReadOnlyList<Scope> _Parameter;

        public TemplateInstanceExpression(TextPosition tp, Element acs, TupleList args)
            : base(tp)
        {
            Access = acs;
            DecParameters = args;
            AppendChild(Access);
            AppendChild(DecParameters);
        }

        public override Scope ReturnType
        {
            get { return Root.TypeManager.IssueTemplateInstance(Access.OverLoad, Parameter.ToArray()); }
        }

        public override OverLoadReference OverLoad
        {
            get { return ReturnType.OverLoad; }
        }

        public IReadOnlyList<Scope> Parameter
        {
            get
            {
                if (_Parameter != null)
                {
                    return _Parameter;
                }
                var pt = new List<Scope>();
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
