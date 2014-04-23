﻿using AbstractSyntax.Pragma;
using AbstractSyntax.Visualizer;
using System;
using System.Diagnostics;

namespace AbstractSyntax
{
    [Serializable]
    public abstract class DataType : Scope
    {
        public virtual PrimitivePragmaType GetPrimitiveType()
        {
            throw new NotSupportedException();
        }
    }

    enum TypeMatchResult
    {
        Unknown,
        PerfectMatch,
        NotCallable,
        MissMatchCount,
        MissMatchType,
    }
}