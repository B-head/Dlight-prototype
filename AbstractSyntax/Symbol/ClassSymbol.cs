﻿using AbstractSyntax.Declaration;
using AbstractSyntax.Expression;
using AbstractSyntax.SpecialSymbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Symbol
{
    public enum ClassType
    {
        Unknown,
        Class,
        Trait,
        Extend,
    }

    [Serializable]
    public class ClassSymbol : TypeSymbol
    {
        public ThisSymbol This { get; private set; }
        public ClassType ClassType { get; private set; }
        public ProgramContext Block { get; private set; }
        protected IReadOnlyList<AttributeSymbol> _Attribute;
        protected IReadOnlyList<GenericSymbol> _Generics;
        protected IReadOnlyList<TypeSymbol> _Inherit;
        public IReadOnlyList<RoutineSymbol> Initializers { get; private set; }
        public IReadOnlyList<RoutineSymbol> AliasCalls { get; private set; }
        private bool IsInitialize;

        public ClassSymbol()
        {
        }

        protected ClassSymbol(ClassType type)
        {
            ClassType = type;
            Block = new ProgramContext();
            AppendChild(Block);
            IsInitialize = true;
        }

        protected ClassSymbol(TextPosition tp, string name, ClassType type, ProgramContext block)
            :base(tp)
        {
            Name = name;
            ClassType = type;
            Block = block;
            This = new ThisSymbol(this);
            Block.Append(This);
            AppendChild(Block);
            IsInitialize = true;
        }

        public void Initialize(string name, ClassType type, ProgramContext block, IReadOnlyList<AttributeSymbol> attr, IReadOnlyList<GenericSymbol> gnr, IReadOnlyList<TypeSymbol> inherit)
        {
            if (IsInitialize)
            {
                throw new InvalidOperationException();
            }
            IsInitialize = true;
            Name = name;
            ClassType = type;
            Block = block;
            This = new ThisSymbol(this);
            Block.Append(This);
            AppendChild(Block);
            _Attribute = attr;
            _Generics = gnr;
            _Inherit = inherit;
        }

        internal override void Prepare()
        {
            InitInitializers();
            InitAliasCalls();
        }

        private void InitInitializers()
        {
            var i = new List<RoutineSymbol>();
            var newFlag = false;
            foreach (var e in Block)
            {
                var r = e as RoutineSymbol;
                if (r == null)
                {
                    continue;
                }
                if (r.IsConstructor)
                {
                    i.Add(r);
                    newFlag = true;
                }
            }
            if (!newFlag)
            {
                var def = new DefaultSymbol(RoutineSymbol.ConstructorIdentifier, this);
                Block.Append(def);
                i.Add(def);
            }
            Initializers = i;
        }

        private void InitAliasCalls()
        {
            var i = new List<RoutineSymbol>();
            var getFlag = false;
            var serFlag = false;
            foreach (var e in Block)
            {
                var r = e as RoutineSymbol;
                if (r == null)
                {
                    continue;
                }
                if (r.IsAliasCall)
                {
                    i.Add(r);
                    //if ()
                    //{
                    //    getFlag = true;
                    //}
                    //if ()
                    //{
                    //    serFlag = true;
                    //}
                }
            }
            if (!getFlag)
            {
                var def = new PropertySymbol(RoutineSymbol.AliasCallIdentifier, this, false);
                Block.Append(def);
                i.Add(def);
            }
            if (!serFlag)
            {
                var def = new PropertySymbol(RoutineSymbol.AliasCallIdentifier, this, true);
                Block.Append(def);
                i.Add(def);
            }
            AliasCalls = i;
        }

        public override IReadOnlyList<AttributeSymbol> Attribute
        {
            get { return _Attribute ?? new List<AttributeSymbol>(); }
        }

        public override IReadOnlyList<GenericSymbol> Generics
        {
            get { return _Generics ?? new List<GenericSymbol>(); }
        }

        public override IReadOnlyList<TypeSymbol> Inherit
        {
            get { return _Inherit ?? new List<TypeSymbol>(); }
        }

        public Scope InheritClass
        {
            get
            {
                var obj = NameResolution("Object").FindDataType().Type as ClassSymbol;
                if (this == obj)
                {
                    return null;
                }
                return Inherit.FirstOrDefault(v => !HasTrait(v)) ?? obj; 
            }
        }

        public IReadOnlyList<Scope> InheritTraits
        {
            get { return Inherit.Where(v => HasTrait(v)).ToList(); }
        }

        private bool HasTrait(Scope scope)
        {
            var c = scope as ClassSymbol;
            if(c != null)
            {
                return c.ClassType == ClassType.Trait;
            }
            return false;
        }

        protected override string ElementInfo
        {
            get 
            { 
                if(Generics.Count == 0)
                {
                    return string.Format("{0}", Name);
                }
                else
                {
                    return string.Format("{0}!({1})", Name, Generics.ToNames());
                }
            }
        }

        public bool IsDefaultConstructor
        {
            get { return Initializers.Any(v => v is DefaultSymbol); }
        }

        public RoutineSymbol ZeroArgInitializer
        {
            get { return Initializers.FirstOrDefault(v => v.Arguments.Count == 0); }
        }

        public bool IsTrait
        {
            get { return ClassType == ClassType.Trait; }
        }

        internal override IEnumerable<OverLoadCallMatch> GetTypeMatch(IReadOnlyList<GenericsInstance> inst, IReadOnlyList<TypeSymbol> pars, IReadOnlyList<TypeSymbol> args)
        {
            var newinst = GenericsInstance.MakeGenericInstance(Generics, pars);
            var newpars = new List<TypeSymbol>();
            foreach(var a in Initializers)
            {
                foreach (var b in a.GetTypeMatch(newinst, newpars, args))
                {
                    yield return b;
                }
            }
            foreach (var a in Root.ConvManager.GetAllInitializer(this))
            {
                foreach (var b in a.GetTypeMatch(newinst, newpars, args))
                {
                    yield return b;
                }
            }
        }

        internal override IEnumerable<OverLoadCallMatch> GetInstanceMatch(IReadOnlyList<GenericsInstance> inst, IReadOnlyList<TypeSymbol> pars, IReadOnlyList<TypeSymbol> args)
        {
            foreach (var a in AliasCalls)
            {
                foreach (var b in a.GetTypeMatch(inst, pars, args))
                {
                    yield return b;
                }
            }
        }

        internal override IEnumerable<TypeSymbol> EnumSubType()
        {
            yield return this;
            foreach(var a in Inherit)
            {
                foreach (var b in a.EnumSubType())
                {
                    yield return b;
                }
            }
        }
    }
}
