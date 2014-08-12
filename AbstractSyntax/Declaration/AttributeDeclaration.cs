﻿using AbstractSyntax.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Declaration
{
    [Serializable]
    public class AttributeDeclaration : AttributeSymbol
    {
        public AttributeDeclaration(AttributeType attr)
            :base(attr)
        {
        }

        public AttributeDeclaration(string name, AttributeType attr)
            :base(name, attr)
        {
        }
    }
}
