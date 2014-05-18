﻿using AbstractSyntax.Expression;
using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AbstractSyntax.Daclate
{
    [Serializable]
    public class DeclateRoutine : RoutineSymbol
    {
        public TupleList Generic { get; set; }
        public TupleList Arguments { get; set; }
        public Element ExplicitType { get; set; }
        public DirectiveList Block { get; set; }

        public override List<DataType> ArgumentType
        {
            get
            {
                if (_ArgumentType != null)
                {
                    return _ArgumentType;
                }
                _ArgumentType = new List<DataType>();
                foreach (var v in Arguments)
                {
                    var temp = v.DataType;
                    _ArgumentType.Add(temp);
                }
                return _ArgumentType;
            }
        }

        public override DataType ReturnType
        {
            get
            {
                if (_ReturnType != null)
                {
                    return _ReturnType;
                }
                if (ExplicitType != null)
                {
                    _ReturnType = ExplicitType.DataType;
                }
                else if (Block.IsInline)
                {
                    var ret = Block[0];
                    if (ret is ReturnDirective)
                    {
                        _ReturnType = ((ReturnDirective)ret).Exp.DataType;
                    }
                    else
                    {
                        _ReturnType = ret.DataType;
                    }
                }
                else
                {
                    var ret = Block.FindElements<ReturnDirective>();
                    if (ret.Count > 0)
                    {
                        _ReturnType = ret[0].Exp.DataType;
                    }
                    else
                    {
                        _ReturnType = Root.Void;
                    }
                }
                return _ReturnType;
            }
        }

        public override bool IsVoidValue
        {
            get { return true; } //todo この代わりのプロパティが必要。
        }

        public override DataType DataType
        {
            get { return ReturnType; }
        }

        public override int Count
        {
            get { return 4; }
        }

        public override Element this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Generic;
                    case 1: return Arguments;
                    case 2: return ExplicitType;
                    case 3: return Block;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        internal override void CheckDataType()
        {
            base.CheckDataType();
            if (Block.IsInline)
            {
                var ret = Block[0];
                if (_ReturnType != ret.DataType)
                {
                    CompileError("disagree-return-type");
                }
            }
            else
            {
                var ret = Block.FindElements<ReturnDirective>();
                foreach (var v in ret)
                {
                    if (_ReturnType != v.Exp.DataType)
                    {
                        CompileError("disagree-return-type");
                    }
                }
            }
        }
    }
}
