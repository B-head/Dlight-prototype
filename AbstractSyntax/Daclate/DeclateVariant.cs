﻿using AbstractSyntax.Expression;
using AbstractSyntax.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AbstractSyntax.Daclate
{
    [Serializable]
    public class DeclateVariant : Scope, IAccess
    {
        public IdentifierAccess Ident { get; set; }
        public Element ExplicitType { get; set; }
        public DataType _DataType { get; set; }

        public override DataType DataType
        {
            get
            {
                if(_DataType != null)
                {
                    return _DataType;
                }
                if (ExplicitType != null)
                {
                    _DataType = ExplicitType.DataType;
                }
                else
                {
                    _DataType = Root.Void;
                }
                return _DataType;
            }
        }

        public OverLoadScope Reference
        {
            get { return Ident.Reference; }
        }

        public void GetReference(Scope scope)
        {
            Ident.GetReference(scope);
        }

        public override int Count
        {
            get { return 2; }
        }

        public override Element this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Ident;
                    case 1: return ExplicitType;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void SetDataType(DataType type)
        {
            _DataType = type;
        }

        internal override TypeMatchResult TypeMatch(List<DataType> type)
        {
            if (type.Count == 0)
            {
                return TypeMatchResult.PerfectMatch;
            }
            else if (type.Count == 1)
            {
                if (type[0] == _DataType)
                {
                    return TypeMatchResult.PerfectMatch;
                }
                else
                {
                    return TypeMatchResult.MissMatchType;
                }
            }
            else
            {
                return TypeMatchResult.MissMatchCount;
            }
        }

        protected override void SpreadElement(Element parent, Scope scope)
        {
            Name = Ident == null ? string.Empty : Ident.Value;
            base.SpreadElement(parent, scope);
        }
    }
}
