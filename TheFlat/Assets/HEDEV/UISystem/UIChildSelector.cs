using System;
using System.Collections.Generic;
using UnityEngine;

namespace Proto.UISystem
{
    public class UIChildSelector : UnityEngine.EventSystems.UIBehaviour
    {
        [SerializeField] private List<Child> children;
        [SerializeField] private int defaultIndex;

        [Space] [Header("Data for run time ")] [SerializeField]
        private GameObject selected;

        [SerializeField] private bool keepSelected;

        protected override void Awake()
        {
            base.Awake();
            if (children == null || children.Count == 0)
                Collect();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (keepSelected && selected != null)
                return;

            if (children.Count > defaultIndex)
                Select(children[0].gameObject);
        }

        public void Collect()
        {
            children = new List<Child>();
            var count = transform.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = CreateChild(transform.GetChild(i).gameObject);
                children.Add(child);
            }
        }

        private Child CreateChild(GameObject obj)
        {
            if (obj == null)
                return null;

            var child = new Child
            {
                gameObject = obj
            };
            return child;
        }

        public void Select(GameObject obj)
        {
            children.ForEach(p =>
            {
                var isSelected = p.gameObject == obj;
                p.gameObject.SetActive(isSelected);
            });
            selected = obj;
        }

        public void Select(int index)
        {
            selected = null;
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var isSelected = i == index;
                child.gameObject.SetActive(isSelected);
                if (isSelected)
                    selected = child.gameObject;
            }
        }

        [Serializable]
        public class Child
        {
            public GameObject gameObject;
        }
    }
}