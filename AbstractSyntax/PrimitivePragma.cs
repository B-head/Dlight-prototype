﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace AbstractSyntax
{
    public class PrimitivePragma : DeclateClass
    {
        public PrimitivePragmaType Type { get; private set; }

        public PrimitivePragma(PrimitivePragmaType type)
        {
            Type = type;
        }

        public override bool IsPragma
        {
            get { return true; }
        }
    }

    public enum PrimitivePragmaType
    {
        Root,
        Boolean,
        Integer8,
        Integer16,
        Integer32,
        Integer64,
        Natural8,
        Natural16,
        Natural32,
        Natural64,
        Binary32,
        Binary64,
    }
}
