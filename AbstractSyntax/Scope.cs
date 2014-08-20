﻿using AbstractSyntax.SpecialSymbol;
using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AbstractSyntax
{
    [Serializable]
    public abstract class Scope : Element
    {
        public string Name { get; protected set; }
        internal Dictionary<string, OverLoadSet> ChildSymbols { get; set; }
        protected Dictionary<string, OverLoadChain> ReferenceCache { get; set; }

        protected Scope()
        {
            Name = string.Empty;
            ChildSymbols = new Dictionary<string, OverLoadSet>();
            ReferenceCache = new Dictionary<string, OverLoadChain>();
        }

        protected Scope(TextPosition tp)
            : base(tp)
        {
            Name = string.Empty;
            ChildSymbols = new Dictionary<string, OverLoadSet>();
            ReferenceCache = new Dictionary<string, OverLoadChain>();
        }

        internal void SpreadChildScope(Element child)
        {
            var s = child as Scope;
            if (s != null)
            {
                AppendChildScope(s);
                return;
            }
            foreach (var v in child)
            {
                SpreadChildScope(v);
            }
        }

        internal void AppendChildScope(Scope scope)
        {
            if (string.IsNullOrEmpty(scope.Name))
            {
                return;
            }
            if (!ChildSymbols.ContainsKey(scope.Name))
            {
                var ol = new OverLoadSet(this);
                ChildSymbols.Add(scope.Name, ol);
            }
            ChildSymbols[scope.Name].Append(scope);
        }

        internal virtual OverLoadChain NameResolution(string name)
        {
            if(ReferenceCache.ContainsKey(name))
            {
                return ReferenceCache[name];
            }
            var n = CurrentScope.NameResolution(name);
            if(ChildSymbols.ContainsKey(name))
            {
                var s = ChildSymbols[name];
                n = new OverLoadChain(this, n, s);
            }
            ReferenceCache.Add(name, n);
            return n;
        }

        public string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                BuildFullName(builder);
                return builder.ToString();
            }
        }

        private void BuildFullName(StringBuilder builder)
        {
            if(CurrentScope != null)
            {
                CurrentScope.BuildFullName(builder);
                builder.Append(".");
            }
            builder.Append(Name);
        }

        public Scope FindName(string name)
        {
            foreach(var v in this)
            {
                var s = v as Scope;
                if(s == null)
                {
                    continue;
                }
                if(s.Name == name)
                {
                    return s;
                }
            }
            return null;
        }

        internal virtual void BuildTacitGeneric(List<GenericSymbol> list)
        {
            if (CurrentScope != null)
            {
                CurrentScope.BuildTacitGeneric(list);
            }
        }

        protected override string ElementInfo
        {
            get { return string.Format("{0}, Child = {1}", string.IsNullOrWhiteSpace(Name) ? "<no-name>" : Name, Count); }
        }

        public virtual IReadOnlyList<AttributeSymbol> Attribute
        {
            get { return new List<AttributeSymbol>(); }
        }

        public virtual TypeSymbol DeclaringType
        {
            get { return GetParent<TypeSymbol>(); }
        }

        public virtual bool IsExecutionContext
        {
            get { return true; }
        }

        public virtual bool IsStaticMember
        {
            get { return CurrentScope is ClassSymbol && Attribute.HasAnyAttribute(AttributeType.Static); }
        }

        public virtual bool IsInstanceMember
        {
            get { return CurrentScope is ClassSymbol && !Attribute.HasAnyAttribute(AttributeType.Static); }
        }
    }
}
