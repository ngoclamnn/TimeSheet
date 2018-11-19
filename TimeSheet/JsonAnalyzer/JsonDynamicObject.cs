using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace TimeSheet.JsonAnalyzer
{

    public class JsonDynamicObject : DynamicObject
    {
        public Dictionary<string, object> dictionary;
        public JsonDynamicObject()
        {

            var comparer = StringComparer.OrdinalIgnoreCase;
            dictionary = new Dictionary<string, object>(comparer);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Type dictType = typeof(Dictionary<string, object>);
            try
            {
                result = dictType.InvokeMember(
                             binder.Name,
                             BindingFlags.InvokeMethod,
                             null, dictionary, args);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;
            if (!dictionary.TryGetValue(name, out result))
            {
                result = null;
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (dictionary.ContainsKey(binder.Name))
                dictionary[binder.Name] = value;
            else
                dictionary.Add(binder.Name, value);
            return true;
        }
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return dictionary.Select(x => x.Key).ToList();
        }

        public static explicit operator List<object>(JsonDynamicObject v)
        {
            throw new NotImplementedException();
        }
    }
}

