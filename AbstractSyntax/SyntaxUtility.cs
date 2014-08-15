﻿using AbstractSyntax.SpecialSymbol;
using AbstractSyntax.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax
{
    public static class SyntaxUtility
    {
        public static IReadOnlyList<T> FindElements<T>(this IReadOnlyList<Element> list) where T : Element
        {
            var result = new List<T>();
            foreach (var v in list)
            {
                if (v is T)
                {
                    result.Add((T)v);
                }
            }
            return result;
        }

        public static IReadOnlyList<Scope> GetDataTypes(this IReadOnlyList<Element> list)
        {
            var result = new List<Scope>();
            foreach (var v in list)
            {
                result.Add(v.ReturnType);
            }
            return result;
        }

        internal static bool HasAnyAttribute(this IReadOnlyList<Scope> attribute, params AttributeType[] type)
        {
            foreach (var v in attribute)
            {
                var a = v as AttributeSymbol;
                if (a == null)
                {
                    continue;
                }
                if (type.Any(t => t == a.AttributeType))
                {
                    return true;
                }
            }
            return false;
        }

        public static IReadOnlyList<ParameterSymbol> MakeParameters(params Scope[] types)
        {
            var ret = new List<ParameterSymbol>();
            for(var i = 0; i < types.Length; ++i)
            {
                var p = new ParameterSymbol("@@arg" + (i + 1), VariantType.Let, new List<Scope>(), types[i]);
                ret.Add(p);
            }
            return ret;
        }

        public static bool HasVariadicArguments(this Scope scope)
        {
            var r = scope as RoutineSymbol;
            if(r == null)
            {
                return false;
            }
            if(r.Arguments.Count == 0)
            {
                return false;
            }
            return r.Arguments.Last().Attribute.HasAnyAttribute(AttributeType.Variadic);
        }

        internal static bool HasAnyErrorType(params Scope[] scope)
        {
            return HasAnyErrorType((IReadOnlyList<Scope>)scope);
        }

        internal static bool HasAnyErrorType(IReadOnlyList<Scope> scope)
        {
            foreach (var v in scope)
            {
                if (v is VoidSymbol || v is UnknownSymbol || v is ErrorTypeSymbol)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
