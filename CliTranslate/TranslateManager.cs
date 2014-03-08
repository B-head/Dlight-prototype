﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbstractSyntax;
using Common;

namespace CliTranslate
{
    public class TranslateManager
    {
        private Dictionary<Scope, Translator> TransDictionary;
        private RootTranslator Root;

        public TranslateManager(string name)
        {
            TransDictionary = new Dictionary<Scope, Translator>();
            Root = new RootTranslator(name);
        }

        public void TranslateTo(Root root, ImportManager manager)
        {
            manager.TranslateImport(Root);
            SpreadTranslate(root, Root);
            Translate(root, Root);
        }

        internal virtual void ChildSpreadTranslate(Scope scope, Translator trans)
        {
            foreach (var v in scope.ScopeChild.Values)
            {
                if (v == null || v.IsImport)
                {
                    continue;
                }
                Translate((dynamic)v, trans);
            }
        }

        internal virtual void SpreadTranslate(Scope scope, Translator trans)
        {
            ChildSpreadTranslate(scope, trans);
        }

        internal virtual void SpreadTranslate(DeclateModule scope, Translator trans)
        {
            var temp = trans.CreateModule(scope.FullPath);
            TransDictionary.Add(scope, temp);
            ChildSpreadTranslate(scope, temp);
        }

        private void ChildTranslate(Element element, Translator trans)
        {
            foreach(var v in element)
            {
                if (v == null || v.IsImport)
                {
                    continue;
                }
                Translate((dynamic)v, trans);
            }
        }

        private void Translate(Element element, Translator trans)
        {
            ChildTranslate(element, trans);
        }

        private void Translate(DirectiveList element, Translator trans)
        {
            foreach (Element v in element)
            {
                Translate((dynamic)v, trans);
                if (!v.IsVoidValue && element.IsInline)
                {
                    trans.GenerateControl(CodeType.Pop);
                }
            }
            if (element.Count <= 0 || !(element.GetChild(element.Count - 1) is ReturnDirective))
            {
                trans.GenerateControl(CodeType.Ret);
            }
        }
    }
}
