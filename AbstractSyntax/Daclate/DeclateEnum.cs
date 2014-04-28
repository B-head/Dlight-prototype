﻿using AbstractSyntax.Expression;
using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;

namespace AbstractSyntax.Daclate
{
    [Serializable]
    public class DeclateEnum : DataType
    {
        public IdentifierAccess Ident { get; set; }

        public override int Count
        {
            get { return 1; }
        }

        public override Element this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Ident;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override string CreateName()
        {
            return Ident == null ? Name : Ident.Value;
        }
    }
}
