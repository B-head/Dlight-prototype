﻿using AbstractSyntax.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AbstractSyntax
{
    [Serializable]
    public class TupleList : Element
    {
        private List<Element> Child;
        private List<Scope> DataTypes;

        public TupleList()
        {
            Child = new List<Element>();
        }

        public TupleList(Element append)
        {
            Child = new List<Element>();
            Append(append);
        }

        public TupleList(TextPosition tp, List<Element> child)
            :base(tp)
        {
            Child = child;
        }

        public void Append(Element value)
        {
            Child.Add(value);
        }

        public override int Count
        {
            get { return Child.Count; }
        }

        public override Element this[int index]
        {
            get { return Child[index]; }
        }

        protected override string ElementInfo
        {
            get { return "Count = " + Child.Count; }
        }

        public IReadOnlyList<Scope> GetDataTypes()
        {
            if (DataTypes != null)
            {
                return DataTypes;
            }
            DataTypes = new List<Scope>();
            foreach(var v in Child)
            {
                DataTypes.Add(v.ReturnType);
            }
            return DataTypes;
        }
    }
}
