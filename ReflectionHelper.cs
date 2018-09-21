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
            foreach (MethodInfo member in type.GetMethods(BindingFlags.Public | BindingFlags.FlattenHierarchy| BindingFlags.Instance))
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
            foreach (MethodInfo member in type.GetMethods(BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
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
    }
}
