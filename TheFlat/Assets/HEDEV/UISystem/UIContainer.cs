using System.Collections.Generic;
using UnityEngine;

namespace Proto.UISystem
{
    [CreateAssetMenu(fileName = "UIContainer", menuName = "ScriptableObject/UIContainer")]
    public class UIContainer : ScriptableObject
    {
        public List<UIKeyValue> UIList;
    }
}