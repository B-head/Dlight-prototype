﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace AbstractSyntax
{
    public class TupleList : Element
    {
        public List<Element> Child { get; set; }

        public TupleList()
        {
            Child = new List<Element>();
        }

        public void Append(Element append)
        {
            Child.Add(append);
        }

        public override int Count
        {
            get { return Child.Count; }
        }

        public override Element GetChild(int index)
        {
            return Child[index];
        }

        public TupleList GetDataTypes()
        {
            TupleList result = new TupleList();
            foreach(var v in Child)
            {
                result.Append(v.DataType);
            }
            return result;
        }
    }
}
