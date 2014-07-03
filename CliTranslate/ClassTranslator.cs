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
        private Dictionary<IScope, dynamic> InitDictonary;
        private MethodBuilder InitContext;
        private ILGenerator InitGenerator;

        public ClassTranslator(DeclateClass path, Translator parent, TypeBuilder builder)
            : base(path, parent)
        {
            Class = builder;
            ClassContext = Class.DefineMethod("@@static_init", MethodAttributes.SpecialName | MethodAttributes.Static);
            parent.GenerateCall(ClassContext);
            InitDictonary = new Dictionary<IScope, dynamic>();
            InitContext = Class.DefineMethod("@@init", MethodAttributes.SpecialName);
            InitGenerator = InitContext.GetILGenerator();
            Generator = ClassContext.GetILGenerator();
            Root.RegisterBuilder(path, Class);
            if (path.IsDefaultConstructor)
            {
                CreateConstructor(path.Default, new IScope[] { });
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

        public RoutineTranslator CreateConstructor(RoutineSymbol path, IEnumerable<IScope> argumentType)
        {
            var argbld = Root.GetTypeBuilders(argumentType);
            var attr = MakeMethodAttributes(path.Attribute);
            var ctor = Class.DefineConstructor(attr, CallingConventions.Any, argbld);
            Root.RegisterBuilder(path, ctor);
            var ret = new RoutineTranslator(path, this, ctor);
            var iinit = path.InheritInitializer;
            var ictor = iinit != null ? (ConstructorInfo)Root.GetBuilder(iinit) : typeof(object).GetConstructor(Type.EmptyTypes);
            ret.GenelateConstructorInit(InitContext, ictor);
            return ret;
        }

        public RoutineTranslator CreateDestructor(RoutineSymbol path)
        {
            var builder = Class.DefineMethod("Finalize", MethodAttributes.Family | MethodAttributes.Virtual);
            return new RoutineTranslator(path, this, builder);
        }

        public override RoutineTranslator CreateRoutine(DeclateRoutine path)
        {
            var retbld = Root.GetTypeBuilder(path.CallReturnType);
            var argbld = Root.GetTypeBuilders(path.ArgumentType);
            var attr = MakeMethodAttributes(path.Attribute, path.IsVirtual);
            var builder = Class.DefineMethod(path.Name, attr, retbld, argbld);
            return new RoutineTranslator(path, this, builder);
        }

        public override ClassTranslator CreateClass(DeclateClass path)
        {
            var cls = Root.GetTypeBuilder(path.InheritClass) ?? typeof(Object);
            var trait = Root.GetTypeBuilders(path.InheritTraits);
            var builder = Class.DefineNestedType(path.Name, TypeAttributes.Class, cls, trait);
            return new ClassTranslator(path, this, builder);
        }

        public override void CreateVariant(DeclateVariant path)
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

        private MethodAttributes MakeMethodAttributes(IReadOnlyList<IScope> attr, bool vtl = false)
        {
            MethodAttributes ret = vtl ? MethodAttributes.Virtual | MethodAttributes.Public : 0;
            foreach(var v in attr)
            {
                var a = v as AttributeSymbol;
                if(a == null)
                {
                    continue;
                }
                switch(a.Attr)
                {
                    case AttributeType.Static: ret |= MethodAttributes.Static; break;
                    case AttributeType.Public: ret |= MethodAttributes.Public; break;
                    case AttributeType.Protected: ret |= MethodAttributes.Family; break;
                    case AttributeType.Private: ret |= MethodAttributes.Private; break;
                }
            }
            return ret;
        }

        private FieldAttributes MakeFieldAttributes(IReadOnlyList<IScope> attr)
        {
            FieldAttributes ret = 0;
            foreach (var v in attr)
            {
                var a = v as AttributeSymbol;
                if (a == null)
                {
                    continue;
                }
                switch (a.Attr)
                {
                    case AttributeType.Static: ret |= FieldAttributes.Static; break;
                    case AttributeType.Public: ret |= FieldAttributes.Public; break;
                    case AttributeType.Protected: ret |= FieldAttributes.Family; break;
                    case AttributeType.Private: ret |= FieldAttributes.Private; break;
                }
            }
            return ret;
        }

        public override void GenerateLoad(IScope name, bool address = false)
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

        public override void GenerateStore(IScope name, bool address = false)
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
