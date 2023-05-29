using UnityEditor;
using System;

namespace Proto.Data
{
    [CustomEditor(typeof(Tasks))]
    public class TasksEditor : DataScriptEditor
    {
        public override string FileID => "1GNLk0aYcw_gxd59eQ_LUK3zcaS1HYfv6tJwfVPzcJIA";
        public override string SheetName => "Task";
        public override DataScript.DataType DataType => DataScript.DataType.Table;
        public override Type SubClassType => typeof(Task);

        public override void SetAssetData(string json)
        {
            var obj = target as Tasks;
            obj.Values = DataScript.MakeJsonListObjectString<Task>(json).Values;
        }
    }
}

