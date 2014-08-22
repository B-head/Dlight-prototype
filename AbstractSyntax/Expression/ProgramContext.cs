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
using AbstractSyntax.Declaration;
using AbstractSyntax.Statement;
using AbstractSyntax.Symbol;
using AbstractSyntax.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AbstractSyntax.Expression
{
    [Serializable]
    public class ProgramContext : Element
    {
        public bool IsInline { get; private set; }

        public ProgramContext()
        {

        }

        public ProgramContext(TextPosition tp, IReadOnlyList<Element> child, bool isInline)
            :base(tp)
        {
            IsInline = isInline;
            AppendChild(child);
        }

        public void Append(Element child)
        {
            AppendChild(child);
        }

        public override bool IsConstant
        {
            get
            {
                foreach (var v in this)
                {
                    if (!v.IsConstant)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool IsNoReturn
        {
            get 
            { 
                if(Parent is IfStatement || Parent is LoopStatement)
                {
                    return true;
                }
                return Parent is NameSpaceSymbol && !(Parent is ModuleDeclaration); 
            }
        }
    }
}
