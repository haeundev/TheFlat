﻿using System;
using System.Collections.Generic;
using System.IO;
using Proto.Util;

namespace Editors.Datas
{
    public class DataMember
    {
        public string Type;
        public string Name;
        public bool isArray;
        public bool isEnum;
        public bool isExternalEnum;
        public int Column;
        public string Separator;

        public string TypeName
        {
            get
            {
                if (!isEnum)
                    return Type;
                if (isExternalEnum)
                    return $"Enums.{Name}";
                return $"{ClassBuilder.MainClassName}.{Name}";
            }
        }
    }

    public class DataClass
    {
        public string Name;
        public List<DataMember> dataMembers = new List<DataMember>();
        public List<DataClass> dataClasses = new List<DataClass>();
        public DataClass Parent;
        public DataClass(string name, DataClass parent)
        {
            Name = name;
            Parent = parent;
        }
        public DataClass GetDataClass(string className)
        {
            var dataClass = dataClasses.Find(p => p.Name.Equals(className));
            if (dataClass == null)
            {
                dataClass = new DataClass(className, this);
                dataClasses.Add(dataClass);
            }
            return dataClass;
        }
    }

    public class ClassBuilder
    {
        public static string MainClassName;
        public string OutFileName;
        public DataClass rootClass;
        public DataScript.DataType DataType { get; private set; }
        public bool Build(IList<IList<Object>> values, string name, DataScript.DataType dataType)
        {
            if (dataType == DataScript.DataType.Table && values.Count < 2)
                return false;
            else if (values.Count < 1)
                return false;

            DataType = dataType;
            if (dataType == DataScript.DataType.Table)
            {
                MainClassName = name.ToPlural();
                OutFileName = name;
                rootClass = new DataClass(name, null);
            }
            else
            {
                MainClassName = name;
                OutFileName = name;
                rootClass = new DataClass("DataClass", null);
            }
            var count = values[0].Count;

            for (int i = 0; i < count; i++)
            {
                var memberType = values[0][i].ToString().Trim();
                var memberName = values[1][i].ToString().Trim();
                CreatetMember(rootClass, memberType, memberName, i);
            }
            return true;
        }

        private void CreatetMember(DataClass dataClass, string type, string name, int column)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(name))
                return;

            var className = GetClassName(ref name);
            if (string.IsNullOrEmpty(className))
            {
                var member = new DataMember();
                member.Name = name;
                member.Column = column;
                SetType(member, type);
                dataClass.dataMembers.Add(member);
            }
            else
            {
                var subDataClass = dataClass.GetDataClass(className);
                CreatetMember(subDataClass, type, name, column);
            }
        }

        private void SetType(DataMember member, string dataType)
        {
            if (dataType.EndsWith("]"))
            {
                var index = dataType.IndexOf('[');
                var lengthWithoutBrace = dataType.Length - 2;
                member.isArray = true;
                if (index < lengthWithoutBrace)
                    member.Separator = dataType.Substring(index + 1, lengthWithoutBrace - index);
                else
                    member.Separator = ",";
                dataType = dataType.Substring(0, index);
            }
            if (dataType == "enum")
            {
                member.isEnum = true;
                if (member.Name.StartsWith("e"))
                {
                    member.isExternalEnum = true;
                    member.Name = member.Name.Substring(1);
                }
            }
            member.Type = dataType;
        }

        private string GetClassName(ref string dataName)
        {
            var index = dataName.IndexOf('.');
            if (index < 1)
                return null;

            var className = dataName.Substring(0, index);
            dataName = dataName.Substring(index + 1);
            return className;
        }

        internal void GenerateClasses(string @namespace, string scriptDirectory)
        {
            var path = Path.Combine(scriptDirectory, $"{OutFileName}.cs");
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("// Generated by Data Class Generator");
                sw.WriteLine("");
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using Proto.Enums;");
                // sw.WriteLine("using Newtonsoft.Json;");
                // sw.WriteLine("using Newtonsoft.Json.Converters;");
                sw.WriteLine("using UnityEngine;");
                sw.WriteLine("");
                sw.WriteLine($"namespace {@namespace}");
                sw.WriteLine("{");                
                WirteRootClass(sw);
                sw.WriteLine("}");
                sw.WriteLine("");
            }
        }

        private void WirteClass(StreamWriter sw, DataClass dataClass, int indent)
        {
            var indentString = GetIndentString(indent);

            sw.WriteLine($"{indentString}[Serializable]");
            sw.WriteLine($"{indentString}public class {dataClass.Name}");
            sw.WriteLine($"{indentString}{{");
            if (dataClass.dataClasses != null)
            {
                var indentString2 = GetIndentString(indent + 1);
                dataClass.dataClasses.ForEach(p => WirteClass(sw, p, indent + 1));
                dataClass.dataClasses.ForEach(p => WriteMemberBySubClass(sw, p, indent + 1));
            }
            WriteMembers(sw, dataClass, indent + 1);
            sw.WriteLine($"{indentString}}}");
            sw.WriteLine("");
        }

        private void WirteRootClass(StreamWriter sw)
        {
            if (DataType == DataScript.DataType.Table)
            {
                WirteClass(sw, rootClass, 1);
            }
            else if (DataType == DataScript.DataType.Const)
            {
                var indentString = GetIndentString(1);
                sw.WriteLine($"{indentString}public class {MainClassName} : ScriptableObject");
                sw.WriteLine("{");
                WirteClass(sw, rootClass, 2);
                sw.WriteLine($"{indentString}public {rootClass.Name} Data;");
                sw.WriteLine("}");
            }
        }

        private bool WriteMembers(StreamWriter sw, DataClass dataClass, int indent)
        {
            bool hasValue = false;
            foreach (var member in dataClass.dataMembers)
            {
                WriteMember(sw, member, indent);
            }
            return hasValue;
        }

        private void WriteMember(StreamWriter sw, DataMember member, int indent)
        {
            var indentString = GetIndentString(indent);
            if (member.isArray)
            {
                if (member.isEnum) 
                    sw.WriteLine($"{indentString}[JsonProperty (ItemConverterType = typeof(StringEnumConverter))]");
                sw.WriteLine($"{indentString}public List<{member.TypeName}> {member.Name.ToPlural()};");
            }
            else
            {
                if (member.isEnum) 
                    sw.WriteLine($"{indentString}[JsonConverter(typeof(StringEnumConverter))]");
                sw.WriteLine($"{indentString}public {member.TypeName} {member.Name};");
            }
        }

        private void WriteMemberBySubClass(StreamWriter sw, DataClass dataClass, int indent)
        {
            var indentString = GetIndentString(indent);
            sw.WriteLine($"{indentString}public List<{dataClass.Name}> {dataClass.Name.ToPlural()};");
        }

        string GetIndentString(int indentLevel) { return new string(' ', indentLevel * 4); }

    }
}
