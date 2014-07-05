﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AbstractSyntax.Literal
{
    [Serializable]
    public class PlainText : Element
    {
        public string Value { get; private set; }
        private IDataType _ReturnType;

        public PlainText(TextPosition tp, string value)
            :base(tp)
        {
            Value = value;
        }

        public override IDataType ReturnType
        {
            get 
            {
                if(_ReturnType == null)
                {
                    _ReturnType = CurrentScope.NameResolution("String").FindDataType();
                }
                return _ReturnType; 
            }
        }

        public string ShowValue
        {
            get { return Regex.Replace(Value, @"\\.", TrimEscape); }
        }

        private string TrimEscape(Match m)
        {
            return m.Value.Substring(1);
        }
    }
}
