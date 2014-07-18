﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliTranslate
{
    [Serializable]
    public class CallStructure : ExpressionStructure
    {
        public BuilderStructure Call { get; private set; }
        public ExpressionStructure Pre { get; private set; }
        public IReadOnlyList<ExpressionStructure> Arguments { get; private set; }

        public CallStructure(TypeStructure rt, BuilderStructure call, ExpressionStructure pre, IReadOnlyList<ExpressionStructure> args)
            :base(rt)
        {
            Call = call;
            Pre = pre;
            Arguments = args;
            AppendChild(Pre);
            AppendChild(Arguments);
        }

        internal override void BuildCode()
        {
            if (Pre != null)
            {
                Pre.BuildCode();
            }
            foreach (var v in Arguments)
            {
                v.BuildCode();
            }
            Call.BuildCall();
        }
    }
}