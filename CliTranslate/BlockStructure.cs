﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CliTranslate
{
    [Serializable]
    public class BlockStructure : ExpressionStructure
    {
        public IReadOnlyList<CilStructure> Expressions { get; private set; }
        public bool IsInline { get; private set; }

        public BlockStructure(TypeStructure rt, IReadOnlyList<CilStructure> exps)
            :base(rt)
        {
            Expressions = exps;
            AppendChild(exps);
        }

        internal override void BuildCode()
        {
            ChildBuildCode(this);
            if (!ResultType.IsVoid && !CurrentContainer.IsDataTypeContext)
            {
                var cg = CurrentContainer.GainGenerator();
                cg.GenerateControl(OpCodes.Pop);
            }
        }

        public bool IsValueReturn
        {
            get 
            { 
                if(Expressions.Count == 0)
                {
                    return false;
                }
                var exp = Expressions[Expressions.Count - 1] as ExpressionStructure;
                if(exp == null)
                {
                    return false;
                }
                return !exp.ResultType.IsVoid;
            }
        }
    }
}
