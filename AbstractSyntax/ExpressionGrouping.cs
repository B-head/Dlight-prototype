﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliTranslate;

namespace AbstractSyntax
{
    public class ExpressionGrouping : MonadicExpression
    {
        internal override Scope GetDataType()
        {
            return Child.GetDataType();
        }
    }
}
