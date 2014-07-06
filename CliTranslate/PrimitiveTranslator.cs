﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using AbstractSyntax;
using AbstractSyntax.Pragma;
using AbstractSyntax.Daclate;

namespace CliTranslate
{
    public class PrimitiveTranslator : Translator
    {
        private TypeBuilder Class;
        private Type Prim;
        private MethodBuilder ClassContext;

        public PrimitiveTranslator(DeclateClass path, Translator parent, TypeBuilder builder)
            : base(path, parent)
        {
            Class = builder;
            Prim = GetPrimitiveType(path.PrimitiveType);
            ClassContext = Class.DefineMethod("@@init", MethodAttributes.SpecialName | MethodAttributes.Static);
            parent.GenerateCall(ClassContext);
            Generator = ClassContext.GetILGenerator();
            Root.RegisterBuilder(path, Prim);
            var ctor = Prim.GetConstructor(Type.EmptyTypes);
            if(ctor != null)
            {
                Root.RegisterBuilder(path.DefaultInitializer, ctor);
            }
        }

        internal static Type GetPrimitiveType(PrimitivePragmaType type)
        {
            switch(type)
            {
                case PrimitivePragmaType.Object: return typeof(Object);
                case PrimitivePragmaType.String: return typeof(String);
                case PrimitivePragmaType.Boolean: return typeof(Boolean);
                case PrimitivePragmaType.Integer8: return typeof(SByte);
                case PrimitivePragmaType.Integer16: return typeof(Int16);
                case PrimitivePragmaType.Integer32: return typeof(Int32);
                case PrimitivePragmaType.Integer64: return typeof(Int64);
                case PrimitivePragmaType.Natural8: return typeof(Byte);
                case PrimitivePragmaType.Natural16: return typeof(UInt16);
                case PrimitivePragmaType.Natural32: return typeof(UInt32);
                case PrimitivePragmaType.Natural64: return typeof(UInt64);
                case PrimitivePragmaType.Binary32: return typeof(Single);
                case PrimitivePragmaType.Binary64: return typeof(Double);
                default: throw new ArgumentException();
            }
        }

        public override void BuildCode()
        {
            base.BuildCode();
            Class.CreateType();
        }

        internal override TypeBuilder CreateLexical(string name)
        {
            return Class.DefineNestedType(name + "@@lexical", TypeAttributes.SpecialName | TypeAttributes.NestedPrivate);
        }

        public override RoutineTranslator CreateRoutine(DeclateRoutine path)
        {
            var retbld = Root.GetTypeBuilder(path.CallReturnType);
            var argbld = Root.GetTypeBuilders(Prim, path.ArgumentTypes);
            var attr = MakeMethodAttributes(path.Attribute, path.IsVirtual) | MethodAttributes.Static;
            var builder = Class.DefineMethod(path.Name, attr, retbld, argbld);
            return new RoutineTranslator(path, this, builder);
        }
    }
}
