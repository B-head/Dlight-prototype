﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using AbstractSyntax;
using AbstractSyntax.Symbol;
using AbstractSyntax.Daclate;

namespace CliTranslate
{
    public class ClassTranslator : Translator
    {
        private TypeBuilder Class;
        private MethodBuilder ClassContext;
        private Dictionary<Scope, dynamic> InitDictonary;
        private MethodBuilder InitContext;
        private ILGenerator InitGenerator;

        public ClassTranslator(Scope path, Translator parent, TypeBuilder builder)
            : base(path, parent)
        {
            Class = builder;
            ClassContext = Class.DefineMethod("@@static_init", MethodAttributes.SpecialName | MethodAttributes.Static);
            parent.BuildInitCall(ClassContext);
            InitDictonary = new Dictionary<Scope, dynamic>();
            InitContext = Class.DefineMethod("@@init", MethodAttributes.SpecialName);
            InitGenerator = InitContext.GetILGenerator();
            Generator = ClassContext.GetILGenerator();
            Root.RegisterBuilder(path, Class);
            var deccls = path as DeclateClass;
            if (deccls != null && deccls.IsDefaultConstructor)
            {
                CreateConstructor(deccls.Default, new Scope[] { });
            }
        }

        public override void BuildCode()
        {
            base.BuildCode();
            InitGenerator.Emit(OpCodes.Ret);
            Class.CreateType();
        }

        internal override TypeBuilder CreateLexical(string name)
        {
            return Class.DefineNestedType(name + "@@lexical", TypeAttributes.SpecialName | TypeAttributes.NestedPrivate);
        }

        public RoutineTranslator CreateConstructor(Scope path, IEnumerable<Scope> argumentType)
        {
            var argbld = Root.GetArgumentBuilders(argumentType);
            var ctor = Class.DefineConstructor(MethodAttributes.Public, CallingConventions.Any, argbld);
            Root.RegisterBuilder(path, ctor);
            return new RoutineTranslator(path, this, ctor, InitContext);
        }

        public RoutineTranslator CreateDestructor(Scope path)
        {
            var builder = Class.DefineMethod("Finalize", MethodAttributes.ReuseSlot | MethodAttributes.Family);
            return new RoutineTranslator(path, this, builder, true);
        }

        public override RoutineTranslator CreateRoutine(Scope path, Scope returnType, IEnumerable<Scope> argumentType)
        {
            var retbld = Root.GetReturnBuilder(returnType);
            var argbld = Root.GetArgumentBuilders(argumentType);
            var builder = Class.DefineMethod(path.Name, MethodAttributes.Public, retbld, argbld);
            return new RoutineTranslator(path, this, builder);
        }

        public override ClassTranslator CreateClass(Scope path)
        {
            var builder = Class.DefineNestedType(path.Name);
            return new ClassTranslator(path, this, builder);
        }

        public override void CreateVariant(Scope path, Scope typeName)
        {
            var type = Root.GetBuilder(typeName);
            var builder = Class.DefineField(path.Name, type, FieldAttributes.Public);
            Root.RegisterBuilder(path, builder);
            var init = Class.DefineField(path.Name + "@@default", type, FieldAttributes.Static | FieldAttributes.SpecialName);
            InitDictonary.Add(path, init);
            InitGenerator.Emit(OpCodes.Ldarg_0);
            InitGenerator.Emit(OpCodes.Ldsfld, init);
            InitGenerator.Emit(OpCodes.Stfld, builder);
        }

        public override void GenerateLoad(Scope name, bool address = false)
        {
            if (name is ThisSymbol)
            {
                GenerateLoad((ThisSymbol)name);
                return;
            }
            dynamic temp;
            if (!InitDictonary.TryGetValue(name, out temp))
            {
                temp = Root.GetBuilder(name);
            }
            BuildLoad(temp, address);
        }

        public override void GenerateStore(Scope name, bool address = false)
        {
            if (name is ThisSymbol)
            {
                GenerateStore((ThisSymbol)name);
                return;
            }
            dynamic temp;
            if (!InitDictonary.TryGetValue(name, out temp))
            {
                temp = Root.GetBuilder(name);
            }
            BuildStore(temp, address);
        }
    }
}
