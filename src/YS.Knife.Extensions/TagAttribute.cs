using System.Reflection;

namespace YS.Knife
{
    /// <summary>
    /// 为程序集、类或成员附加自定义标签，可叠加多个；同一目标上同名 key 重复时抛 <see cref="InvalidOperationException"/>。
    /// 派生类型/重写成员上定义的同名 key 会覆盖基类上的定义。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TagAttribute : Attribute
    {
        public TagAttribute(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
        public string Key { get; }
        public object Value { get; }

        /// <summary>
        /// 获取程序集上定义的 tag 字典；无 tag 时返回 null。
        /// </summary>
        public static Dictionary<string, object> GetTags(Assembly assembly)
        {
            return BuildTags(assembly.GetCustomAttributes(typeof(TagAttribute), false).OfType<TagAttribute>());
        }

        /// <summary>
        /// 获取类型（含继承链）上定义的 tag 字典，派生类型的同名 key 覆盖基类；无 tag 时返回 null。
        /// </summary>
        public static Dictionary<string, object> GetTags(Type type)
        {
            var tags = new Dictionary<string, object>();
            for (var t = type; t != null && t != typeof(object); t = t.BaseType)
            {
                // 逐层只取本类型声明的 tag，已收集的 key（来自派生层）保留，实现派生覆盖基类
                foreach (var attr in GetDeclaredTags(t))
                {
                    if (!tags.ContainsKey(attr.Key))
                    {
                        tags[attr.Key] = attr.Value;
                    }
                }
            }
            return tags.Count > 0 ? tags : null;
        }

        /// <summary>
        /// 获取成员（属性/方法，含继承链）上定义的 tag 字典，重写成员的同名 key 覆盖基类；无 tag 时返回 null。
        /// </summary>
        public static Dictionary<string, object> GetTags(MemberInfo member)
        {
            var tags = new Dictionary<string, object>();
            switch (member)
            {
                case PropertyInfo property:
                    for (var current = property; current != null; current = GetBaseDefinition(current))
                    {
                        foreach (var attr in GetDeclaredTags(current))
                        {
                            if (!tags.ContainsKey(attr.Key))
                            {
                                tags[attr.Key] = attr.Value;
                            }
                        }
                    }
                    break;
                case MethodInfo method:
                    for (var current = method; current != null; current = GetBaseDefinition(current))
                    {
                        foreach (var attr in GetDeclaredTags(current))
                        {
                            if (!tags.ContainsKey(attr.Key))
                            {
                                tags[attr.Key] = attr.Value;
                            }
                        }
                    }
                    break;
                default:
                    foreach (var attr in GetDeclaredTags(member))
                    {
                        if (!tags.ContainsKey(attr.Key))
                        {
                            tags[attr.Key] = attr.Value;
                        }
                    }
                    break;
            }
            return tags.Count > 0 ? tags : null;
        }

        /// <summary>
        /// 获取成员上仅本层声明的 tags，校验同层内无重复 key。
        /// </summary>
        private static IEnumerable<TagAttribute> GetDeclaredTags(MemberInfo member)
        {
            var declared = member.GetCustomAttributes(typeof(TagAttribute), false).OfType<TagAttribute>().ToList();
            var keys = new HashSet<string>();
            foreach (var attr in declared)
            {
                if (!keys.Add(attr.Key))
                {
                    throw new InvalidOperationException($"Duplicate tag key '{attr.Key}'.");
                }
            }
            return declared;
        }

        private static PropertyInfo GetBaseDefinition(PropertyInfo property)
        {
            var accessor = property.GetGetMethod(true) ?? property.GetSetMethod(true);
            var baseAccessor = GetBaseDefinition(accessor);
            if (baseAccessor == null)
            {
                return null;
            }
            return baseAccessor.DeclaringType?.GetProperty(property.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static MethodInfo GetBaseDefinition(MethodInfo method)
        {
            if (method == null || method.DeclaringType?.BaseType == null)
            {
                return null;
            }
            var baseDefinition = method.GetBaseDefinition();
            return baseDefinition != method ? baseDefinition : null;
        }

        private static Dictionary<string, object> BuildTags(IEnumerable<TagAttribute> tagAttributes)
        {
            var tags = new Dictionary<string, object>();
            var keys = new HashSet<string>();
            foreach (var attr in tagAttributes)
            {
                if (!keys.Add(attr.Key))
                {
                    throw new InvalidOperationException($"Duplicate tag key '{attr.Key}'.");
                }
                tags[attr.Key] = attr.Value;
            }
            return tags.Count > 0 ? tags : null;
        }
    }
}
