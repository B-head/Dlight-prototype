﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Symbol
{
    [Serializable]
    public class RoutineSymbol : Scope
    {
        protected List<DataType> _ArgumentType;
        protected DataType _ReturnType;

        public virtual List<DataType> ArgumentType
        {
            get { return _ArgumentType; }
        }

        public virtual DataType ReturnType
        {
            get { return _ReturnType; }
        }

        internal override IEnumerable<TypeMatch> GetTypeMatch(IReadOnlyList<DataType> type)
        {
            yield return TypeMatch.MakeTypeMatch(Root.Conversion, this, type, ArgumentType);
        }
    }
}
