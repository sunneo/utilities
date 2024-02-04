/*
* Copyright (c) 2019-2020 [Open Source Developer, Sunneo].
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the [Open Source Developer, Sunneo] nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE [Open Source Developer, Sunneo] AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE [Open Source Developer, Sunneo] AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Utilities
{
    public class ReflectionHelper
    {
        Object Target;
        public class MethodCollectionHelper
        {
            public ReflectionHelper Parent;
            String MethodName;
            public List<MethodInfo> List = new List<MethodInfo>();
            public object Invoke(params object[] parms)
            {
                if (List.Count == 0) return null;
                MethodInfo firstOrDefault = List.FirstOrDefault();
                if (firstOrDefault == null) return null;
                if (firstOrDefault.ReturnType == typeof(void))
                {
                    if (parms == null && firstOrDefault.GetParameters().Length > 0)
                    {
                        firstOrDefault.Invoke(Parent.Target, new object[]{null});
                    }
                    else
                    {
                        firstOrDefault.Invoke(Parent.Target, parms);
                    }
                    return null;
                }
                else
                {
                    if (parms == null && firstOrDefault.GetParameters().Length > 0)
                    {
                        return firstOrDefault.Invoke(Parent.Target, new object[] { null });
                    }
                    else
                    {
                        return firstOrDefault.Invoke(Parent.Target, parms);
                    }
                }
            }
        }
        public class PropertyHelper
        {
            public ReflectionHelper Parent;
            public PropertyInfo Member;
            public void Set(object val)
            {
                Member.SetValue(Parent.Target, val);
            }
            public object Get()
            {
                return Member.GetValue(Parent.Target);
            }
        }
        public class FieldHelper
        {
            public ReflectionHelper Parent;
            public FieldInfo Member;
            public void Set(object val)
            {
                Member.SetValue(Parent.Target, val);
            }
            public object Get()
            {
                return Member.GetValue(Parent.Target);
            }
        }
        public class EventHelper
        {
            public ReflectionHelper Parent;
            public EventInfo Member;
            public void Invoke(params object[] parms)
            {
                MethodInfo method = Member.GetRaiseMethod();
                if (method != null)
                {
                    method.Invoke(Parent.Target, parms);
                }
            }
        }
        public Dictionary<String, MethodCollectionHelper> Methods = new Dictionary<string, MethodCollectionHelper>();
        public Dictionary<String, PropertyHelper> Properties = new Dictionary<string, PropertyHelper>();
        public Dictionary<String, FieldHelper> Fields = new Dictionary<string, FieldHelper>();
        public Dictionary<String, EventHelper> Events = new Dictionary<string, EventHelper>();
        public ReflectionHelper(Object Target)
        {
            this.Target = Target;
            Type type = this.Target.GetType();
            InitializeMembers(type, false);

        }
        protected virtual void InitializeMembers(Type type, bool isStatic)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
            if(isStatic)
            {
                flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static;
            }
            foreach (MethodInfo member in type.GetMethods(flags))
            {
                if (member.IsSpecialName) continue;
                MethodCollectionHelper collection = null;
                if (!Methods.ContainsKey(member.Name))
                {
                    collection = new MethodCollectionHelper();
                    collection.Parent = this;
                    Methods[member.Name] = collection;
                }
                else
                {
                    collection = Methods[member.Name];
                }
                collection.List.Add(member);
            }
            flags = BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance;
            if (isStatic)
            {
                flags = BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static;
            }
            foreach (MethodInfo member in type.GetMethods(flags))
            {
                if (member.IsSpecialName) continue;
                MethodCollectionHelper collection = null;
                if (!Methods.ContainsKey(member.Name))
                {
                    collection = new MethodCollectionHelper();
                    collection.Parent = this;
                    Methods[member.Name] = collection;
                }
                else
                {
                    collection = Methods[member.Name];
                }
                collection.List.Add(member);
            }

            foreach (FieldInfo member in type.GetFields())
            {
                if (member.IsSpecialName) continue;
                Fields[member.Name] = new FieldHelper()
                {
                    Parent = this,
                    Member = member
                };
            }
            foreach (PropertyInfo member in type.GetProperties())
            {
                if (member.IsSpecialName) continue;
                Properties[member.Name] = new PropertyHelper()
                {
                    Parent = this,
                    Member = member
                };
            }
            foreach (EventInfo member in type.GetEvents())
            {
                if (member.IsSpecialName) continue;
                Events[member.Name] = new EventHelper()
                {
                    Parent = this,
                    Member = member
                };
            }
        }
        public ReflectionHelper(Type type)
        {
            this.Target = null;

            InitializeMembers(type,true);

        }
    }
}
