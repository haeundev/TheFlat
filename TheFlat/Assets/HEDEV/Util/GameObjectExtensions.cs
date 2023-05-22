using System.Collections.Generic;
using UnityEngine;

namespace Proto.Util
{
    public static class GameObjectExtensions
    {
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
                child.gameObject.SetLayerRecursively(layer);
        }

        public static List<GameObject> GetAllChildren(this GameObject parent)
        {
            var list = new List<GameObject>();
            if (parent == default)
                return list;
            for (var i = 0; i < parent.transform.childCount; i++)
                list.Add(parent.transform.GetChild(i).gameObject);
            return list;
        }
    
        public static void SetToParentTransform(this GameObject gameObject)
        {
            gameObject.transform.position = gameObject.transform.parent.position;
            gameObject.transform.rotation = gameObject.transform.parent.rotation;
        }

        public static void SetInactiveWithAnimatorValuesReset(this GameObject gameObject, Animator animator)
        {
            if (!gameObject.activeSelf)
                return;
        
            if (animator)
                animator.WriteDefaultValues();
        
            gameObject.SetActive(false);
        }
    
        public static void SetInactiveWithChildrenAnimatorsValuesReset(this GameObject gameObject)
        {
            if (!gameObject.activeSelf)
                return;
        
            var animators = gameObject.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
                animator.WriteDefaultValues();
        
            gameObject.SetActive(false);
        }
    }
}