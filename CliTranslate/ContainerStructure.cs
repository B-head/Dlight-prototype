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
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CliTranslate
{
    [Serializable]
    public abstract class ContainerStructure : BuilderStructure
    {
        internal virtual bool IsDataTypeContext
        {
            get { return false; }
        }

        internal virtual CodeGenerator GainGenerator()
        {
            throw new NotSupportedException();
        }

        internal virtual TypeBuilder CreateType(string name, TypeAttributes attr)
        {
            throw new NotSupportedException();
        }

        internal virtual MethodBuilder CreateMethod(string name, MethodAttributes attr)
        {
            throw new NotSupportedException();
        }

        internal virtual ConstructorBuilder CreateConstructor(MethodAttributes attr, Type[] pt)
        {
            throw new NotSupportedException();
        }

        internal virtual FieldBuilder CreateField(string name, Type dt, FieldAttributes attr)
        {
            throw new NotSupportedException();
        }
    }
}
