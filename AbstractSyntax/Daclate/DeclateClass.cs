﻿using AbstractSyntax.Expression;
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
        public TupleList DecGeneric { get; set; }
        public TupleList InheritAccess { get; set; }
        public DirectiveList Block { get; set; }

        public DeclateClass(TextPosition tp, string name, bool isTrait, TupleList attr, TupleList generic, TupleList inherit, DirectiveList block)
            :base(tp, name, isTrait)
        {
            AttributeAccess = attr;
            DecGeneric = generic;
            InheritAccess = inherit;
            Block = block;
        }

        public override IReadOnlyList<Scope> Attribute
        {
            get
            {
                if (_Attribute != null)
                {
                    return _Attribute;
                }
                _Attribute = new List<Scope>();
                foreach (var v in AttributeAccess)
                {
                    _Attribute.Add(v.Reference.FindDataType());
                }
                if(IsClass)
                {
                    _Attribute.Add(Root.Class);
                }
                else if(IsTrait)
                {
                    _Attribute.Add(Root.Trait);
                }
                if (!IsAnyAttribute(AttributeType.Public, AttributeType.Protected, AttributeType.Private))
                {
                    var p = NameResolution("public").FindDataType();
                    _Attribute.Add(p);
                }
                return _Attribute;
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
                _Inherit = new List<ClassSymbol>();
                foreach (var v in InheritAccess)
                {
                    var dt = v.Reference.FindDataType() as ClassSymbol;
                    if (dt != null)
                    {
                        _Inherit.Add(dt);
                    }
                }
                return _Inherit;
            }
        }

        public bool IsDefaultConstructor
        {
            get { return Initializer.Any(v => v is DefaultSymbol); }
        }

        public override int Count
        {
            get { return 6; }
        }

        public override Element this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return AttributeAccess;
                    case 1: return DecGeneric;
                    case 2: return InheritAccess;
                    case 3: return Block;
                    case 4: return This;
                    case 5: return Default;
                    default: throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        internal override void SpreadElement(Element parent, Scope scope)
        {
            base.SpreadElement(parent, scope);
            var newFlag = false;
            foreach(var e in Block)
            {
                var r = e as RoutineSymbol;
                if (r == null)
                {
                    continue;
                }
                if(r.IsConstructor)
                {
                    Initializer.Add(r);
                    newFlag = true;
                }
                else if (r.IsConvertor) //todo 1引数で使える型変換関数の生成が必要。
                {
                    Root.Conversion.Append(r);
                    Initializer.Add(r);
                }
                else if (r.Operator != TokenType.Unknoun)
                {
                    Root.OpManager[r.Operator].Append(r);
                }
            }
            if(!newFlag)
            {
                Initializer.Add(Default);
            }
        }

        internal override void CheckSemantic()
        {
            base.CheckSemantic();
            foreach (var v in InheritAccess)
            {
                var dt = v.Reference.FindDataType();
                if (!(dt is ClassSymbol))
                {
                    CompileError("not-datatype-inherit");
                }
            }
        }
    }
}
