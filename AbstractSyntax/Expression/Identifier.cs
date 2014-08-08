﻿using AbstractSyntax.Declaration;
using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class Identifier : Element
    {
        public string Value { get; private set; }
        public bool IsPragmaAccess { get; private set; }
        private bool? _IsTacitThis;
        private OverLoadReference _Reference;

        public Identifier(TextPosition tp, string value, bool isPragma)
            :base(tp)
        {
            Value = value;
            IsPragmaAccess = isPragma;
        }

        public bool IsTacitThis
        {
            get
            {
                if (_IsTacitThis != null)
                {
                    return _IsTacitThis.Value;
                }
                if (!HasThisMember(CallScope))
                {
                    _IsTacitThis = false;
                }
                else if (!(CurrentScope is RoutineDeclaration)) //todo この条件が必要な理由を調べる。（忘れたｗ）
                {
                    _IsTacitThis = false;
                }
                else
                {
                    _IsTacitThis = true;
                }
                return _IsTacitThis.Value;
            }
        }

        public ThisSymbol ThisReference
        {
            get
            {
                var c = GetParent<ClassSymbol>();
                return c.This;
            }
        }

        protected override string ElementInfo
        {
            get
            {
                if (IsPragmaAccess)
                {
                    return "@@" + Value;
                }
                else
                {
                    return Value;
                }
            }
        }

        public override Scope ReturnType
        {
            get { return CallScope.CallReturnType; }
        }

        public override bool IsConstant
        {
            get { return ((RoutineSymbol)CallScope).IsFunction; }
        }

        public Scope CallScope
        {
            get { return OverLoad.CallSelect().Call; }
        }

        public override OverLoadReference OverLoad
        {
            get
            {
                if(_Reference != null)
                {
                    return _Reference;
                }
                if (IsPragmaAccess)
                {
                    _Reference = CurrentScope.NameResolution("@@" + Value);
                }
                else
                {
                    _Reference = CurrentScope.NameResolution(Value);
                }
                return _Reference;
            }
        }

        private bool IsStaticLocation()
        {
            var r = GetParent<RoutineSymbol>();
            if (r == null)
            {
                return false;
            }
            return r.IsStaticMember;
        }

        private bool HasThisMember(Scope scope)
        {
            if (scope.IsStaticMember || scope.IsThisCall)
            {
                return false;
            }
            var cls = GetParent<ClassSymbol>();
            while (cls != null)
            {
                if (scope.GetParent<ClassSymbol>() == cls)
                {
                    return true;
                }
                cls = cls.InheritClass as ClassSymbol;
            }
            return false;
        }

        internal override void CheckSemantic(CompileMessageManager cmm)
        {
            if (OverLoad.IsUndefined)
            {
                if (IsPragmaAccess)
                {
                    cmm.CompileError("undefined-pragma", this);
                }
                else
                {
                    cmm.CompileError("undefined-identifier", this);
                }
            }
            if (HasAnyAttribute(CallScope.Attribute, AttributeType.Private) && !HasCurrentAccess(CallScope.GetParent<ClassSymbol>()))
            {
                cmm.CompileError("not-accessable", this);
            }
            if (CallScope.IsInstanceMember && IsStaticLocation() && !(Parent is Postfix)) //todo Postfixだけではなく包括的な例外処理をする。
            {
                cmm.CompileError("not-accessable", this);
            }
        }
    }
}
