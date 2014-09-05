﻿/*
Copyright 2014 B_head

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CliTranslate
{
    [Serializable]
    public class CallStructure : ExpressionStructure
    {
        public BuilderStructure Call { get; private set; }
        public ExpressionStructure Pre { get; private set; }
        public CilStructure Access { get; private set; }
        public CilStructure Variant { get; private set; }
        public IReadOnlyList<ExpressionStructure> Arguments { get; private set; }
        public IReadOnlyList<BuilderStructure> Converters { get; private set; }
        public bool IsVariadic { get; private set; }

        public CallStructure(TypeStructure rt, BuilderStructure call, ExpressionStructure pre, CilStructure variant)
            : base(rt)
        {
            Call = call;
            Pre = pre;
            Access = pre;
            Variant = variant;
            Arguments = new List<ExpressionStructure>();
            Converters = new List<BuilderStructure>();
            if (Access != null)
            {
                AppendChild(Access);
            }
            if (Arguments != null)
            {
                AppendChild(Arguments);
            }
        }

        public CallStructure(TypeStructure rt, BuilderStructure call, ExpressionStructure pre, CilStructure access, CilStructure variant, IReadOnlyList<ExpressionStructure> args, IReadOnlyList<BuilderStructure> convs, bool isVariadic = false)
            :base(rt)
        {
            Call = call;
            Pre = pre;
            Access = access;
            Variant = variant;
            Arguments = args;
            Converters = convs;
            IsVariadic = isVariadic;
            if (Access != null)
            {
                AppendChild(Access);
            }
            if (Arguments != null)
            {
                AppendChild(Arguments);
            }
        }

        internal override void BuildCode()
        {
            if(CurrentContainer.IsDataTypeContext)
            {
                return;
            }
            var cg = CurrentContainer.GainGenerator();
            if (Pre != null)
            {
                Pre.BuildCode();
                if (Call is MethodStructure && Pre.ResultType != null)
                {
                    cg.GenerateToAddress(Pre.ResultType);
                }
            }
            if (Call == null)
            {
                return;
            }
            if (IsVariadic)
            {
                var arr = new LocalStructure(GetVariadicType(Call), cg);
                cg.GenerateArray(GetVariadicLangth(Call), arr.DataType.GetBaseType());
                cg.GenerateStore(arr);
                var vi = GetVariadicIndex(Call);
                for (var i = 0; i < Arguments.Count; ++i)
                {
                    if (i < vi)
                    {
                        Arguments[i].BuildCode();
                        if (Converters[i] != null)
                        {
                            Converters[i].BuildCall(cg);
                        }
                    }
                    else
                    {
                        cg.GenerateLoad(arr);
                        cg.GeneratePrimitive(i - vi);
                        Arguments[i].BuildCode();
                        if (Converters[i] != null)
                        {
                            Converters[i].BuildCall(cg);
                        }
                        cg.GenerateToAddress(Arguments[i].ResultType, arr.DataType.GetBaseType());
                        cg.GenerateStoreElement(arr.DataType.GetBaseType());
                    }
                }
                cg.GenerateLoad(arr);
            }
            else
            {
                for (var i = 0; i < Arguments.Count; ++i)
                {
                    Arguments[i].BuildCode();
                    if(Converters[i] != null)
                    {
                        Converters[i].BuildCall(cg);
                    }
                }
            }
            var lss = Call as LoadStoreStructure;
            var gms = Call as GenericMethodStructure;
            if (lss != null)
            {
                lss.BuildCall(Variant, cg);
            }
            else if(gms != null)
            {
                gms.BuildCall(Variant, cg);
            }
            else
            {
                Call.BuildCall(cg);
            }
        }

        private TypeStructure GetVariadicType(BuilderStructure call)
        {
            var c = call as MethodBaseStructure;
            if (c == null)
            {
                return null;
            }
            return c.Arguments.Last().ParamType;
        }

        private int GetVariadicLangth(BuilderStructure call)
        {
            var c = call as MethodBaseStructure;
            if (c == null)
            {
                return -1;
            }
            return Arguments.Count - c.Arguments.Count + 1;
        }

        private int GetVariadicIndex(BuilderStructure call)
        {
            var c = call as MethodBaseStructure;
            if(c == null)
            {
                return -1;
            }
            return c.Arguments.Count - 1;
        }
    }
}
