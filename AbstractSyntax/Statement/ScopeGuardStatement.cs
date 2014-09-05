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
using AbstractSyntax.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractSyntax.Statement
{
    public enum ScopeGuardType
    {
        Unknown,
        Exit,
        Success,
        Failure,
    }

    [Serializable]
    public class ScopeGuardStatement : Scope
    {
        public ScopeGuardType ScopeGuardType { get; private set; }
        public ProgramContext Block { get; private set; }

        public ScopeGuardStatement(TextPosition tp, ScopeGuardType type, ProgramContext block)
            :base(tp)
        {
            ScopeGuardType = type;
            Block = block;
            AppendChild(Block);
        }
    }
}
