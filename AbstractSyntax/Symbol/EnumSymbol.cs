﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Symbol
{
    [Serializable]
    public class EnumSymbol : Scope
    {
        public override bool IsDataType
        {
            get { return true; }
        }

    }
}
