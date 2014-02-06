﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CilTranslate;

namespace AbstractSyntax
{
    public class DeclareVariant : Element
    {
        public Identifier Ident { get; set; }
        public Identifier ExplicitType { get; set; }
        public Translator IdentType { get; set; }

        public override bool IsReference
        {
            get { return true; }
        }

        public override int ChildCount
        {
            get { return 2; }
        }

        public override Element GetChild(int index)
        {
            switch (index)
            {
                case 0: return Ident;
                case 1: return ExplicitType;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected override string ElementInfo()
        {
            string temp = " (" + IdentType.Name + ")";
            if (ExplicitType == null)
            {
                return base.ElementInfo() + Ident.Value + temp;
            }
            else
            {
                return base.ElementInfo() + Ident.Value + ":" + ExplicitType.Value + temp;
            }
        }

        protected override Translator CreateTranslator(Translator trans)
        {
            return trans.CreateVariant(Ident.Value);
        }

        public override void CheckDataType()
        {
            if (ExplicitType != null)
            {
                IdentType = Trans.NameResolution(ExplicitType.Value);
                Trans.SetBaseType(IdentType);
            }
            base.CheckDataType();
        }

        public override void CheckDataTypeAssign(Translator type)
        {
            if (IdentType == null)
            {
                IdentType = type;
                Trans.SetBaseType(IdentType);
            }
            base.CheckDataTypeAssign(type);
        }

        public override Translator GetDataType()
        {
            return IdentType;
        }

        public override void Translate()
        {
            Parent.Trans.GenelateLoad(Trans);
        }

        public override void TranslateAssign()
        {
            Parent.Trans.GenelateStore(Trans);
        }
    }
}