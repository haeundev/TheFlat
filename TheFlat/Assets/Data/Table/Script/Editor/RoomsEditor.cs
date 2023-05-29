using UnityEditor;
using System;

namespace Proto.Data
{
    [CustomEditor(typeof(Rooms))]
    public class RoomsEditor : DataScriptEditor
    {
        public override string FileID => "1GNLk0aYcw_gxd59eQ_LUK3zcaS1HYfv6tJwfVPzcJIA";
        public override string SheetName => "Room";
        public override DataScript.DataType DataType => DataScript.DataType.Table;
        public override Type SubClassType => typeof(Room);

        public override void SetAssetData(string json)
        {
            var obj = target as Rooms;
            obj.Values = DataScript.MakeJsonListObjectString<Room>(json).Values;
        }
    }
}

