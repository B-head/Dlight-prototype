﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Symbol
{
    [Serializable]
    public class UnknownSymbol : Scope, IDataType
    {
        public override IDataType ReturnType
        {
            get { return Root.Unknown; }
        }
    }
}
