using UnityEditor;
using System;

namespace Proto.Data
{
    [CustomEditor(typeof(SoundClips))]
    public class SoundClipsEditor : DataScriptEditor
    {
        public override string FileID => "1GNLk0aYcw_gxd59eQ_LUK3zcaS1HYfv6tJwfVPzcJIA";
        public override string SheetName => "SoundClip";
        public override DataScript.DataType DataType => DataScript.DataType.Table;
        public override Type SubClassType => typeof(SoundClip);

        public override void SetAssetData(string json)
        {
            var obj = target as SoundClips;
            obj.Values = DataScript.MakeJsonListObjectString<SoundClip>(json).Values;
        }
    }
}

