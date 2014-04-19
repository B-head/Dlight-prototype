﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractSyntax;
using AbstractSyntax.Expression;

namespace AbstractSyntax.Daclate
{
    public class DeclateClass : Scope
    {
        public IdentifierAccess Ident { get; set; }
        public TupleList Generic { get; set; }
        public TupleList Inherit { get; set; }
        public DirectiveList Block { get; set; }
        public ThisScope This { get; set; }
        public List<DeclateClass> InheritRefer { get; private set; }

        public DeclateClass()
        {
            InheritRefer = new List<DeclateClass>();
            This = new ThisScope(this);
        }

        public override bool IsVoidValue
        {
            get { return true; }
        }

        public override int Count
        {
            get { return 5; }
        }

        public override Element GetChild(int index)
        {
            switch (index)
            {
                case 0: return Ident;
                case 1: return Generic;
                case 2: return Inherit;
                case 3: return Block;
                case 4: return This;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected override string CreateName()
        {
            return Ident == null ? Name : Ident.Value;
        }

        internal override TypeMatchResult TypeMatch(List<Scope> type)
        {
            if (type.Count == 0)
            {
                return TypeMatchResult.PerfectMatch;
            }
            else
            {
                return TypeMatchResult.MissMatchCount;
            }
        }

        internal override void SpreadReference(Scope scope)
        {
            base.SpreadReference(scope);
            if(Inherit == null)
            {
                return;
            }
            foreach (var v in Inherit)
            {
                if(v.DataType is DeclateClass)
                {
                    InheritRefer.Add((DeclateClass)v.DataType);
                }
                else
                {
                    CompileError("継承元はクラスである必要があります。");
                }
            }
        }

        public bool IsContain(DeclateClass other)
        {
            return Object.ReferenceEquals(this, other);
        }

        public bool IsConvert(DeclateClass other)
        {
            if(IsContain(other))
            {
                return true;
            }
            foreach(var v in InheritRefer)
            {
                if(v.IsConvert(other))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
