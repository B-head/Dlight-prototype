﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace Dlight.Translate
{
    class ModuleTranslator : Translator
    {
        private ModuleBuilder Builder { get; set; }
        private RoutineTranslator GlobalContext { get; set; }

        public ModuleTranslator(FullName fullname, Translator parent, ModuleBuilder builder)
            : base(fullname, parent)
        {
            Builder = builder;
            GlobalContext = CreateGlobalContext("@@globalcontext");
        }

        private RoutineTranslator CreateGlobalContext(string name)
        {
            MethodAttributes attr = MethodAttributes.Static | MethodAttributes.SpecialName;
            MethodBuilder builder = Builder.DefineGlobalMethod(name, attr, typeof(void), Type.EmptyTypes);
            return new RoutineTranslator(name, this, builder);
        }

        public override MethodInfo GetContext()
        {
            return GlobalContext.GetContext();
        }

        public override void Save()
        {
            base.Save();
            GlobalContext.Save();
            Builder.CreateGlobalFunctions();
        }

        public override Translator GenelateVariant(FullName gen, FullName type)
        {
            return GlobalContext.GenelateVariant(gen, type);
        }

        public override void GenelateConstant(int value)
        {
            GlobalContext.GenelateConstant(value);
        }

        public override void GenelateConstant(double value)
        {
            GlobalContext.GenelateConstant(value);
        }

        public override void GenelateLoad(FullName fullName)
        {
            GlobalContext.GenelateLoad(fullName);
        }

        public override void GenelateStore(FullName fullName)
        {
            GlobalContext.GenelateStore(fullName);
        }

        public override void GenelateOperate(FullName fullName, TokenType operation)
        {
            GlobalContext.GenelateOperate(fullName, operation);
        }
    }
}
