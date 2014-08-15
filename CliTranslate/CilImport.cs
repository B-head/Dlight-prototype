﻿using AbstractSyntax;
using AbstractSyntax.Expression;
using AbstractSyntax.SpecialSymbol;
using AbstractSyntax.Symbol;
using CoreLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CliTranslate
{
    public class CilImport
    {
        private Root Root;
        public Dictionary<object, Scope> ImportDictionary { get; private set; }
        private const BindingFlags Binding = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        public CilImport(Root root)
        {
            Root = root;
            ImportDictionary = new Dictionary<object, Scope>();
            ImportDictionary.Add(typeof(void), Root.Void);
        }

        private NameSpaceSymbol GetNameSpace(string name)
        {
            var nl = name.Split('.');
            NameSpaceSymbol ret = Root;
            foreach(var v in nl)
            {
                var temp = (NameSpaceSymbol)ret.FindName(v);
                if(temp == null)
                {
                    temp = new NameSpaceSymbol(v);
                    ret.Append(temp);
                }
                ret = temp;
            }
            return ret;
        }

        public void ImportAssembly(Assembly assembly)
        {
            var module = assembly.GetModules();
            foreach (var m in module)
            {
                ImportModule(m);
            }
        }

        private void ImportModule(Module module)
        {
            var type = module.GetTypes();
            foreach (var t in type)
            {
                if (!t.IsPublic)
                {
                    continue;
                }
                var ns = GetNameSpace(t.Namespace);
                if (typeof(void) == t)
                {
                    continue;
                }
                ns.Append(ImportType(t));
            }
        }

        private Scope ImportType(Type type)
        {
            if (type.HasElementType)
            {
                return ImportModifyType(type);
            }
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                return ImportTemplateInstance(type);
            }
            if (type.IsGenericParameter)
            {
                return ImportGenericType(type);
            }
            if (type.IsEnum)
            {
                return ImportEnum(type);
            }
            return ImportPureType(type);
        }

        private ClassSymbol ImportPureType(Type type)
        {
            ClassType classType;
            var attribute = new List<Scope>();
            AppendEmbededAttribute(attribute, type, out classType);
            var generic = CreateGenericList(type.GetGenericArguments());
            var inherit = CreateInheritList(type);
            var block = new ProgramContext();
            ClassSymbol elem;
            if (type.IsSubclassOf(typeof(Attribute)))
            {
                var attrUsage = (AttributeUsageAttribute)type.GetCustomAttribute<AttributeUsageAttribute>(true);
                elem = new AttributeSymbol(TrimTypeNameMangling(type.Name), classType, block, attribute, generic, inherit, 
                    attrUsage.ValidOn, attrUsage.AllowMultiple, attrUsage.Inherited);
            }
            else
            {
                elem = new ClassSymbol(TrimTypeNameMangling(type.Name), classType, block, attribute, generic, inherit);
            }
            if (ImportDictionary.ContainsKey(type))
            {
                return (ClassSymbol)ImportDictionary[type];
            }
            ImportDictionary.Add(type, elem);
            var ctor = type.GetConstructors(Binding);
            foreach (var c in ctor)
            {
                if(c.IsAssembly || c.IsFamilyAndAssembly || c.IsPrivate)
                {
                    continue;
                }
                block.Append(ImportConstructor(c));
            }
            //todo Eventのインポートに対応する。
            //var eve = type.GetEvents();
            //foreach (var e in eve)
            //{
            //    block.Append(ImportEvent(e));
            //}
            var property = type.GetProperties(Binding);
            foreach (var p in property)
            {
                var g = p.GetMethod;
                if (g != null && !g.IsAssembly && !g.IsFamilyAndAssembly && !g.IsPrivate)
                {
                    block.Append(ImportProperty(p.GetMethod, p.Name));
                }
                var s = p.SetMethod;
                if (s != null && !s.IsAssembly && !s.IsFamilyAndAssembly && !s.IsPrivate)
                {
                    block.Append(ImportProperty(p.SetMethod, p.Name));
                }
            }
            var method = type.GetMethods(Binding);
            foreach (var m in method)
            {
                if (m.IsAssembly || m.IsFamilyAndAssembly || m.IsPrivate)
                {
                    continue;
                }
                if(ImportDictionary.ContainsKey(m))
                {
                    continue;
                }
                block.Append(ImportMethod(m));
            }
            var field = type.GetFields(Binding);
            foreach (var f in field)
            {
                if (f.IsAssembly || f.IsFamilyAndAssembly || f.IsPrivate)
                {
                    continue;
                }
                block.Append(ImportField(f));
            }
            var nested = type.GetNestedTypes(Binding);
            foreach (var n in nested)
            {
                if (n.IsNestedAssembly || n.IsNestedFamANDAssem || n.IsNestedPrivate)
                {
                    continue;
                }
                block.Append(ImportType(n));
            }
            elem.Initialize();
            return elem;
        }

        private TemplateInstanceSymbol ImportModifyType(Type type)
        {
            var elementType = ImportType(type.GetElementType());
            TemplateInstanceSymbol elem = null;
            if(type.IsArray)
            {
                elem = Root.TemplateInstanceManager.Issue(Root.EmbedArray, elementType);
            }
            else if(type.IsByRef)
            {
                elem = Root.TemplateInstanceManager.Issue(Root.Refer, elementType);
            }
            else if(type.IsPointer)
            {
                elem = Root.TemplateInstanceManager.Issue(Root.Pointer, elementType);
            }
            else
            {
                throw new ArgumentException("type");
            }
            if (ImportDictionary.ContainsKey(type))
            {
                return (TemplateInstanceSymbol)ImportDictionary[type];
            }
            ImportDictionary.Add(type, elem);
            return elem;
        }

        private TemplateInstanceSymbol ImportTemplateInstance(Type type)
        {
            var definition = ImportType(type.GetGenericTypeDefinition());
            var parameter = new List<Scope>();
            var elem = Root.TemplateInstanceManager.Issue(definition, parameter);
            if (ImportDictionary.ContainsKey(type))
            {
                return (TemplateInstanceSymbol)ImportDictionary[type];
            }
            ImportDictionary.Add(type, elem);
            AppendParameterType(parameter, type.GetGenericArguments());
            return elem;
        }

        private GenericSymbol ImportGenericType(Type type)
        {
            var attribute = new List<Scope>();
            AppendEmbededAttribute(attribute, type);
            var constraint = CreateConstraintList(type.GetGenericParameterConstraints());
            var elem = new GenericSymbol(type.Name, attribute, constraint);
            if (ImportDictionary.ContainsKey(type))
            {
                return (GenericSymbol)ImportDictionary[type];
            }
            ImportDictionary.Add(type, elem);
            return elem;
        }

        private EnumSymbol ImportEnum(Type type)
        {
            var attribute = new List<Scope>();
            AppendEmbededAttribute(attribute, type);
            var dt = ImportType(type.GetEnumUnderlyingType());
            var block = new ProgramContext();
            var elem = new EnumSymbol(type.Name, block, attribute, dt);
            if (ImportDictionary.ContainsKey(type))
            {
                return (EnumSymbol)ImportDictionary[type];
            }
            ImportDictionary.Add(type, elem);
            foreach(var v in type.GetEnumNames())
            {
                var f = new VariantSymbol(v, VariantType.Const, new List<Scope>(), dt);
                block.Append(f);
            }
            return elem;
        }

        private RoutineSymbol ImportMethod(MethodInfo method)
        {
            if (ImportDictionary.ContainsKey(method))
            {
                return (RoutineSymbol)ImportDictionary[method];
            }
            var attribute = new List<Scope>();
            AppendEmbededAttribute(attribute, method);
            var generic = CreateGenericList(method.GetGenericArguments());
            var arguments = CreateArgumentList(method);
            var rt = ImportType(method.ReturnType);
            var elem = new RoutineSymbol(method.Name, RoutineType.Routine, TokenType.Unknoun, attribute, generic, arguments, rt);
            ImportDictionary.Add(method, elem);
            return elem;
        }

        private RoutineSymbol ImportProperty(MethodInfo prop, string name)
        {
            if (ImportDictionary.ContainsKey(prop))
            {
                return (RoutineSymbol)ImportDictionary[prop];
            }
            var attribute = new List<Scope>();
            AppendEmbededAttribute(attribute, prop);
            var generic = new List<GenericSymbol>();
            var arguments = CreateArgumentList(prop);
            var rt = ImportType(prop.ReturnType);
            var elem = new RoutineSymbol(name, RoutineType.Routine, TokenType.Unknoun, attribute, generic, arguments, rt);
            ImportDictionary.Add(prop, elem);
            return elem;
        }

        private RoutineSymbol ImportConstructor(ConstructorInfo ctor)
        {
            if (ImportDictionary.ContainsKey(ctor))
            {
                return (RoutineSymbol)ImportDictionary[ctor];
            }
            var attribute = new List<Scope>();
            AppendEmbededAttribute(attribute, ctor);
            var generic = new List<GenericSymbol>();
            var arguments = CreateArgumentList(ctor);
            var rt = ImportType(ctor.DeclaringType);
            var elem = new RoutineSymbol(RoutineSymbol.ConstructorIdentifier, RoutineType.Routine, TokenType.Unknoun, attribute, generic, arguments, rt);
            ImportDictionary.Add(ctor, elem);
            return elem;
        }

        private ParameterSymbol ImportArgument(ParameterInfo prm)
        {
            if (ImportDictionary.ContainsKey(prm))
            {
                return (ParameterSymbol)ImportDictionary[prm];
            }
            var attribute = new List<Scope>();
            AppendEmbededAttribute(attribute, prm);
            var dt = ImportType(prm.ParameterType);
            var elem = new ParameterSymbol(prm.Name, VariantType.Var, attribute, dt);
            ImportDictionary.Add(prm, elem);
            return elem;
        }

        private VariantSymbol ImportField(FieldInfo field)
        {
            if (ImportDictionary.ContainsKey(field))
            {
                return (VariantSymbol)ImportDictionary[field];
            }
            VariantType type;
            var attribute = new List<Scope>();
            AppendEmbededAttribute(attribute, field, out type);
            var dt = ImportType(field.FieldType);
            var elem = new VariantSymbol(field.Name, type, attribute, dt);
            ImportDictionary.Add(field, elem);
            return elem;
        }

        private string TrimTypeNameMangling(string name)
        {
            var i = name.IndexOf('`');
            if(i < 0)
            {
                return name;
            }
            return name.Substring(0, i);
        }

        private void AppendEmbededAttribute(List<Scope> list, Type type)
        {
            ClassType classType;
            AppendEmbededAttribute(list, type, out classType);
        }

        private void AppendEmbededAttribute(List<Scope> list, Type type, out ClassType classType)
        {
            classType = ClassType.Class;
            if (type.GetCustomAttribute<GlobalScopeAttribute>() != null) list.Add(Root.GlobalScope);
            if (type.IsAbstract) list.Add(Root.Abstract);
            if (type.IsClass) classType = ClassType.Class;
            if (type.IsInterface) classType = ClassType.Trait;
            if (type.IsNestedFamily) list.Add(Root.Protected);
            if (type.IsNestedFamORAssem) list.Add(Root.Protected);
            if (type.IsNestedPublic) list.Add(Root.Public);
            if (type.IsPublic) list.Add(Root.Public);
            if (type.IsSealed) list.Add(Root.Final);
            if (type.IsValueType) classType = ClassType.Class;
            if (type.IsGenericParameter)
            {
                var gattr = type.GenericParameterAttributes;
                if (gattr.HasFlag(GenericParameterAttributes.Contravariant)) list.Add(Root.Contravariant);
                if (gattr.HasFlag(GenericParameterAttributes.Covariant)) list.Add(Root.Covariant);
                if (gattr.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)) list.Add(Root.ConstructorConstraint);
                if (gattr.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)) list.Add(Root.ValueConstraint);
                if (gattr.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)) list.Add(Root.ReferenceConstraint);
            }
        }

        private void AppendEmbededAttribute(List<Scope> list, MethodBase method)
        {
            if (method.IsAbstract) list.Add(Root.Abstract);
            if (method.IsFamily) list.Add(Root.Protected);
            if (method.IsFamilyOrAssembly) list.Add(Root.Protected);
            if (method.IsFinal) list.Add(Root.Final);
            if (method.IsPublic) list.Add(Root.Public);
            if (method.IsStatic) list.Add(Root.Static);
            if (method.IsVirtual) list.Add(Root.Virtual);
        }

        private void AppendEmbededAttribute(List<Scope> list, FieldInfo field, out VariantType type)
        {
            type = VariantType.Var;
            if (field.IsFamily) list.Add(Root.Protected);
            if (field.IsFamilyOrAssembly) list.Add(Root.Protected);
            if (field.IsInitOnly) type = VariantType.Let;
            if (field.IsLiteral) type = VariantType.Const;
            if (field.IsPublic) list.Add(Root.Public);
            if (field.IsStatic) list.Add(Root.Static);
        }

        private void AppendEmbededAttribute(List<Scope> list, ParameterInfo parameter)
        {
            if (parameter.GetCustomAttribute<ParamArrayAttribute>() != null) list.Add(Root.Variadic);
            if (parameter.IsOptional) list.Add(Root.Optional);
        }

        private void AppendParameterType(List<Scope> list, Type[] prm)
        {
            foreach (var v in prm)
            {
                list.Add(ImportType(v));
            }
        }

        private IReadOnlyList<GenericSymbol> CreateGenericList(Type[] gnr)
        {
            var ret = new List<GenericSymbol>();
            foreach(var v in gnr)
            {
                ret.Add(ImportGenericType(v));
            }
            return ret;
        }

        private IReadOnlyList<Scope> CreateInheritList(Type type)
        {
            var ret = new List<Scope>();
            if (type.BaseType != null)
            {
                ret.Add(ImportType(type.BaseType));
            }
            foreach(var v in type.GetInterfaces())
            {
                if (!v.IsPublic && !v.IsNestedPublic && !v.IsNestedFamily && !v.IsNestedFamORAssem)
                {
                    continue;
                }
                var t = ImportType(v);
                if (t != null)
                {
                    ret.Add(t);
                }
            }
            return ret;
        }

        private IReadOnlyList<Scope> CreateConstraintList(Type[] types)
        {
            var ret = new List<Scope>();
            foreach (var v in types)
            {
                ret.Add(ImportType(v));
            }
            return ret;
        }

        private IReadOnlyList<ParameterSymbol> CreateArgumentList(MethodBase method)
        {
            var ret = new List<ParameterSymbol>();
            foreach (var v in method.GetParameters())
            {
                ret.Add(ImportArgument(v));
            }
            return ret;
        }
    }
}
