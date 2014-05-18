﻿using AbstractSyntax.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AbstractSyntax
{
    [Serializable]
    public abstract class Scope : Element
    {
        public string Name { get; set; }
        private Dictionary<string, OverLoad> ScopeSymbol;
        private List<Scope> _ScopeChild;
        public IReadOnlyList<Scope> ScopeChild { get { return _ScopeChild; } }

        public Scope()
        {
            ScopeSymbol = new Dictionary<string, OverLoad>();
            _ScopeChild = new List<Scope>();
        }

        protected override string ElementInfo
        {
            get { return Name; }
        }

        private void Merge(Scope other)
        {
            foreach (var v in other.ScopeSymbol)
            {
                OverLoad ol;
                if (ScopeSymbol.ContainsKey(v.Key))
                {
                    ol = ScopeSymbol[v.Key];
                }
                else
                {
                    ol = new OverLoad(Root.Unknown);
                    ScopeSymbol[v.Key] = ol;
                }
                ol.Merge(v.Value);
            }
        }

        internal OverLoad NameResolution(string name)
        {
            OverLoad temp;
            if(ScopeSymbol.TryGetValue(name, out temp))
            {
                return temp;
            }
            if (this is Root)
            {
                return Root.UnknownOverLoad;
            }
            return CurrentScope.NameResolution(name);
        }

        public string GetFullName()
        {
            StringBuilder builder = new StringBuilder();
            BuildFullName(builder);
            return builder.ToString();
        }

        private void BuildFullName(StringBuilder builder)
        {
            if(CurrentScope != null && !(CurrentScope is Root))
            {
                CurrentScope.BuildFullName(builder);
                builder.Append(".");
            }
            builder.Append(Name);
        }

        public void AppendChild(Scope scope)
        {
            OverLoad ol;
            if (ScopeSymbol.ContainsKey(scope.Name))
            {
                ol = ScopeSymbol[scope.Name];
            }
            else
            {
                ol = new OverLoad(Root.Unknown);
                ScopeSymbol[scope.Name] = ol;
            }
            ol.Append(scope);//todo 重複判定を実装する。
            _ScopeChild.Add(scope);
            if (scope is NameSpace)
            {
                Merge(scope);
            }
        }

        protected override void SpreadElement(Element parent, Scope scope)
        {
            base.SpreadElement(parent, scope);
            if (!(this is Root))
            {
                scope.AppendChild(this);
            }
        }

        internal virtual IEnumerable<TypeMatch> GetTypeMatch(IReadOnlyList<DataType> type)
        {
            yield return TypeMatch.MakeNotCallable(Root.Unknown);
        }

        internal override void CheckSyntax()
        {
            if (string.IsNullOrEmpty(Name))
            {
                CompileError("null-scope-name");
            }
            /*else if (false)
            {
                CompileError("識別子 " + Name + " は既に宣言されています。");
            }*/
            base.CheckSyntax();
        }
    }
}
