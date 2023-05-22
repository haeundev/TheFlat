using System;
using System.Reflection;

namespace Proto.Util
{
    public class Type
    {
        public static System.Type GetType(string typeName)
        {
            var type = System.Type.GetType(typeName);
            if (type != null)
                return type;

            type = GetCustomTypeIncludeNested(typeName);
            if (type != null)
                return type;

            type = GetTypeFromCurrentDomain(typeName);
            return type;
        }

        public static System.Type GetCustomType(string typeName)
        {
            var assemblyName = "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            var assembly = Assembly.Load(assemblyName);
            if (assembly != null)
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static System.Type GetFirstCustomTypeByClassName(string typeName)
        {
            var assemblyName = "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            var assembly = Assembly.Load(assemblyName);
            if (assembly != null)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                    if (type.Name.Equals(typeName))
                        return type;
            }

            return null;
        }

        public static System.Type GetCustomTypeIncludeNested(string typeName)
        {
            var type = GetCustomType(typeName);
            if (type != null)
                return type;

            var index = typeName.LastIndexOf('.');
            if (index == -1 || index == typeName.Length - 1)
                return null;

            var parentName = typeName.Substring(0, index);
            var childName = typeName.Substring(index + 1);

            var parentType = GetCustomTypeIncludeNested(parentName);
            if (parentType != null)
            {
                var childType = parentType.GetNestedType(childName);
                if (childType != null)
                    return childType;
            }

            return null;
        }

        public static System.Type GetTypeFromCurrentDomain(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var domain in assemblies)
            {
                var referencedAssemblies = domain.GetReferencedAssemblies();
                foreach (var assemblyName in referencedAssemblies)
                {
                    // Load the referenced assembly
                    var assembly = Assembly.Load(assemblyName);
                    if (assembly != null)
                    {
                        // See if that assembly defines the named type
                        var type = assembly.GetType(typeName);
                        if (type != null)
                            return type;
                    }
                }
            }

            return null;
        }
    }
}