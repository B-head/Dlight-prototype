﻿using AbstractSyntax.Directive;
using AbstractSyntax.Expression;
using AbstractSyntax.Pragma;
using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AbstractSyntax.Daclate
{
    [Serializable]
    public class DeclateClass : ClassSymbol
    {
        public TupleList AttributeAccess { get; set; }
        public TupleList DecGenerics { get; set; }
        public TupleList InheritAccess { get; set; }
        public DirectiveList Block { get; set; }

        public DeclateClass(TextPosition tp, string name, bool isTrait, TupleList attr, TupleList generic, TupleList inherit, DirectiveList block)
            :base(tp, name, isTrait)
        {
            AttributeAccess = attr;
            DecGenerics = generic;
            InheritAccess = inherit;
            Block = block;
            AppendChild(AttributeAccess);
            AppendChild(DecGenerics);
            AppendChild(InheritAccess);
            AppendChild(Block);
        }

        public override IReadOnlyList<Scope> Attribute
        {
            get
            {
                if (_Attribute != null)
                {
                    return _Attribute;
                }
                var a = new List<Scope>();
                foreach (var v in AttributeAccess)
                {
                    a.Add(v.OverLoad.FindDataType());
                }
                if(IsClass)
                {
                    a.Add(Root.Class);
                }
                else if(IsTrait)
                {
                    a.Add(Root.Trait);
                }
                if (!HasAnyAttribute(a, AttributeType.Public, AttributeType.Protected, AttributeType.Private))
                {
                    var p = NameResolution("public").FindDataType();
                    a.Add(p);
                }
                _Attribute = a;
                return _Attribute;
            }
        }

        public override IReadOnlyList<GenericSymbol> Generics
        {
            get
            {
                if (_Generics != null)
                {
                    return _Generics;
                }
                var pt = new List<GenericSymbol>();
                foreach (var v in DecGenerics)
                {
                    pt.Add((GenericSymbol)v);
                }
                _Generics = pt;
                return _Generics;
            }
        }

        public override IReadOnlyList<ClassSymbol> Inherit 
        {
            get
            {
                if (_Inherit != null)
                {
                    return _Inherit;
                }
                var i = new List<ClassSymbol>();
                foreach (var v in InheritAccess)
                {
                    var dt = v.OverLoad.FindDataType() as ClassSymbol;
                    if (dt != null)
                    {
                        i.Add(dt);
                    }
                }
                _Inherit = i;
                return _Inherit;
            }
        }

        public bool IsDefaultConstructor
        {
            get { return Initializer.Any(v => v is DefaultSymbol); }
        }
        

        public override IReadOnlyList<RoutineSymbol> Initializer
        {
            get
            {
                if (_Initializer != null)
                {
                    return _Initializer;
                }
                var i = new List<RoutineSymbol>();
                var newFlag = false;
                foreach (var e in Block)
                {
                    var r = e as RoutineSymbol;
                    if (r == null)
                    {
                        continue;
                    }
                    if (r.IsConstructor)
                    {
                        i.Add(r);
                        newFlag = true;
                    }
                    else if (r.IsConvertor) //todo 1引数で使える型変換関数の生成が必要。
                    {
                        Root.ConvManager.Append(r);
                        i.Add(r);
                    }
                    else if (r.Operator != TokenType.Unknoun)
                    {
                        Root.OpManager.Append(r);
                    }
                }
                if (!newFlag)
                {
                    i.Add(Default);
                }
                _Initializer = i;
                return _Initializer;
            }
        }

        internal override void CheckSemantic(CompileMessageManager cmm)
        {
            foreach (var v in InheritAccess)
            {
                var dt = v.OverLoad.FindDataType();
                if (!(dt is ClassSymbol))
                {
                    cmm.CompileError("not-datatype-inherit", this);
                }
            }
        }
    }
}
