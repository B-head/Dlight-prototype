﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace AbstractSyntax
{
    public class TupleList : DyadicExpression
    {
        public TupleList()
        {
            Operation = TokenType.List;
        }
    }
}