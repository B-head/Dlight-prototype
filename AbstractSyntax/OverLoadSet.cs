﻿using AbstractSyntax.Declaration;
using AbstractSyntax.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AbstractSyntax
{
    [Serializable]
    public class OverLoadSet : OverLoad
    {
        public Scope ThisScope { get; private set; }
        private List<Scope> Symbols;
        private bool IsHoldAlias;

        internal OverLoadSet(Scope scope)
        {
            ThisScope = scope;
            Symbols = new List<Scope>();
        }

        internal void Append(Scope scope)
        {
            if(scope is AliasDeclaration)
            {
                IsHoldAlias = true;
            }
            Symbols.Add(scope);
        }

        internal OverLoadSet Clone(Scope thisScope)
        {
            var ret = new OverLoadSet(thisScope);
            ret.Symbols = Symbols;
            ret.IsHoldAlias = IsHoldAlias;
            return ret;
        }

        public override bool IsUndefined
        {
            get { return Symbols.Count == 0; }
        }

        internal override Root Root
        {
            get { return ThisScope.Root; }
        }

        internal override IEnumerable<Scope> TraversalChilds()
        {
            if (IsHoldAlias)
            {
                SpreadAlias();
            }
            foreach (var v in Symbols)
            {
                yield return v;
            }
        }

        internal override IEnumerable<VariantSymbol> TraversalVariant()
        {
            if (IsHoldAlias)
            {
                SpreadAlias();
            }
            foreach (var v in Symbols)
            {
                var variant = v as VariantSymbol;
                if (variant != null)
                {
                    yield return variant;
                }
            }
        }

        internal override IEnumerable<AttributeSymbol> TraversalAttribute()
        {
            if (IsHoldAlias)
            {
                SpreadAlias();
            }
            foreach (var v in Symbols)
            {
                var attr = v as AttributeSymbol;
                if (attr != null)
                {
                    yield return attr;
                }
            }
        }

        internal override IEnumerable<TypeSymbol> TraversalDataType(IReadOnlyList<TypeSymbol> pars)
        {
            if(IsHoldAlias)
            {
                SpreadAlias();
            }
            var inst = GetGenericInstance();
            foreach(var v in Symbols)
            {
                var type = v as TypeSymbol;
                if (type != null)
                {
                    yield return type;
                }
            }
        }

        internal override IEnumerable<OverLoadMatch> TraversalCall(IReadOnlyList<TypeSymbol> pars, IReadOnlyList<TypeSymbol> args)
        {
            if (IsHoldAlias)
            {
                SpreadAlias();
            }
            var inst = GetGenericInstance();
            foreach (var s in Symbols)
            {
                foreach (var m in s.GetTypeMatch(inst, pars, args))
                {
                    yield return m;
                }
            }
        }

        private void SpreadAlias()
        {
            var alias = Symbols.FindAll(v => v is AliasDeclaration);
            Symbols.RemoveAll(v => v is AliasDeclaration);
            foreach(var v in alias)
            {
                var ol = v.OverLoad;
                foreach (var s in ol.TraversalChilds())
                {
                    Append(s);
                }
            }
        }

        private IReadOnlyList<GenericsInstance> GetGenericInstance()
        {
            var cti = ThisScope as ClassTemplateInstance;
            if (cti != null)
            {
                return cti.GetGenericInstance();
            }
            var rti = ThisScope as RoutineTemplateInstance;
            if (rti != null)
            {
                return rti.GetGenericInstance();
            }
            return new List<GenericsInstance>();
        }

        public override string ToString()
        {
            return string.Format("Symbols = {0}, This = {{{1}}}", Symbols.Count, ThisScope);
        }
    }
}
