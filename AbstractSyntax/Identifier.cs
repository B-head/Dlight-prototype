﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliTranslate;

namespace AbstractSyntax
{
    public class Identifier : Element
    {
        public string Value { get; set; }
        public bool IsPragma { get; set; }
        public Scope Refer { get; private set; }

        public Identifier()
        {
            Refer = new VoidScope();
        }

        internal override Scope DataType
        {
            get 
            { 
                return Refer.DataType; 
            }
        }

        public override bool IsAssignable
        {
            get { return true; }
        }

        protected override string AdditionalInfo()
        {
            if (IsPragma)
            {
                return "@@" + Value;
            }
            else
            {
                return Value;
            }
        }

        internal override void SpreadReference(Scope scope)
        {
            base.SpreadReference(scope);
            var temp  = scope.NameResolution(Value);
            if (temp == null)
            {
                CompileError("識別子 " + Value + " が宣言されていません。");
            }
            else
            {
                Refer = temp;
            }
        }

        internal override void Translate(Translator trans)
        {
            trans.GenerateLoad(Refer.FullPath);
            base.Translate(trans);
        }

        internal override void TranslateAssign(Translator trans)
        {
            trans.GenerateStore(Refer.FullPath);
            base.TranslateAssign(trans);
        }
    }
}
