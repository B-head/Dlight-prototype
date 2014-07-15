﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliTranslate
{
    [Serializable]
    public class BlockStructure : CilStructure
    {
        public IReadOnlyList<CilStructure> Expressions { get; private set; }

        public BlockStructure(IReadOnlyList<CilStructure> exps)
        {
            Expressions = exps;
            AppendChild(Expressions);
        }
    }
}
