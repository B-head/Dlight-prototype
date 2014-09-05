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
using System.Text;
using System.Threading.Tasks;

namespace CliTranslate
{
    [Serializable]
    public abstract class BuilderStructure : CilStructure
    {
        private bool IsPreBuilded;
        private bool IsPostBuilded;

        internal void RelayPreBuild()
        {
            if (IsPreBuilded)
            {
                return;
            }
            IsPreBuilded = true;
            PreBuild();
        }

        internal void RelayPostBuild()
        {
            if (IsPostBuilded)
            {
                return;
            }
            IsPostBuilded = true;
            PostBuild();
        }

        protected virtual void PreBuild()
        {
            return;
        }

        internal virtual void PostBuild()
        {
            return;
        }

        internal virtual void BuildCall(CodeGenerator cg)
        {
            throw new NotSupportedException();
        }

        internal virtual BuilderStructure RenewInstance(TypeStructure type)
        {
            throw new NotSupportedException();
        }
    }
}
