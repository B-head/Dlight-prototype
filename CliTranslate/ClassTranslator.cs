﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using AbstractSyntax;
using AbstractSyntax.Symbol;
using AbstractSyntax.Declaration;

namespace CliTranslate
{
    public class ClassTranslator : Translator
    {
        private TypeBuilder Class;
        private MethodBuilder ClassContext;
        private Dictionary<Scope, dynamic> InitDictonary;
        private MethodBuilder InitContext;
        private ILGenerator InitGenerator;

        public ClassTranslator(ClassDeclaration path, Translator parent, TypeBuilder builder)
            : base(path, parent)
        {
            Class = builder;
            ClassContext = Class.DefineMethod("@@static_init", MethodAttributes.SpecialName | MethodAttributes.Static);
            parent.GenerateCall(ClassContext);
            InitDictonary = new Dictionary<Scope, dynamic>();
            InitContext = Class.DefineMethod("@@init", MethodAttributes.SpecialName);
            InitGenerator = InitContext.GetILGenerator();
            Generator = ClassContext.GetILGenerator();
            Root.RegisterBuilder(path, Class);
            if (path.IsDefaultConstructor)
            {
                CreateConstructor(path.Default);
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

        public RoutineTranslator CreateConstructor(RoutineSymbol path)
        {
            var argbld = Root.GetTypeBuilders(path.ArgumentTypes);
            var attr = MakeMethodAttributes(path.Attribute);
            var ctor = Class.DefineConstructor(attr, CallingConventions.Any, argbld);
            Root.RegisterBuilder(path, ctor);
            var ret = new RoutineTranslator(path, this, ctor);
            var iinit = path.InheritInitializer;
            ret.GenelateConstructorInit(InitContext, (ConstructorInfo)Root.GetBuilder(iinit));
            return ret;
        }

        public RoutineTranslator CreateDestructor(RoutineSymbol path)
        {
            var builder = Class.DefineMethod("Finalize", MethodAttributes.Family | MethodAttributes.Virtual);
            return new RoutineTranslator(path, this, builder);
        }

        public override RoutineTranslator CreateRoutine(RoutineDeclaration path)
        {
            var attr = MakeMethodAttributes(path.Attribute, path.IsVirtual);
            var builder = Class.DefineMethod(path.Name, attr);
            return new RoutineTranslator(path, this, builder);
        }

        public override ClassTranslator CreateClass(ClassDeclaration path)
        {
            var cls = Root.GetTypeBuilder(path.InheritClass);
            var trait = Root.GetTypeBuilders(path.InheritTraits);
            var attr = MakeTypeAttributes(path.Attribute, path.IsTrait);
            var builder = Class.DefineNestedType(path.Name, attr, cls, trait);
            return new ClassTranslator(path, this, builder);
        }

        public override void CreateVariant(VariantDeclaration path)
        {
            var type = Root.GetBuilder(path.ReturnType);
            var attr = MakeFieldAttributes(path.Attribute);
            var builder = Class.DefineField(path.Name, type, attr);
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
