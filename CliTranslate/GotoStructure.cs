﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CliTranslate
{
    [Serializable]
    public class GotoStructure : ExpressionStructure
    {
        public LabelStructure Label { get; private set; }

        public GotoStructure(TypeStructure rt, LabelStructure label)
            :base(rt)
        {
            Label = label;
        }

        internal override void BuildCode()
        {
            var cg = CurrentContainer.GainGenerator();
            cg.GenerateJump(OpCodes.Br, Label);
        }
    }
}
