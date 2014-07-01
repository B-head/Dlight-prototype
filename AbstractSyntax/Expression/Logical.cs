﻿using AbstractSyntax.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class Logical : DyadicExpression
    {
        public override IDataType ReturnType
        {
            get { return Left.ReturnType; }
        }
    }
}
