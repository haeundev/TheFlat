using UniRx;
using UnityEditor;
using UnityEngine;

namespace Proto.Data
{
    [ExecuteAlways]
    public class DataTableManager : MonoBehaviour
    {
        private static DataTableManager _instance;

        [SerializeField] private Tasks tasks;
        [SerializeField] private Rooms rooms;
        [SerializeField] private SoundClips clips;
        
        public static Tasks Tasks => _instance.tasks;
        public static Rooms Rooms => _instance.rooms;
        public static SoundClips SoundClips => _instance.clips;
        
        public static DataTableManager Instance
        {
            get
            {
#if UNITY_EDITOR
                if (_instance == null)
                    _instance = AssetDatabase.LoadAssetAtPath<DataTableManager>(
                        "Assets/Proto_System/Prefabs/DataTableManager.prefab");
#endif
                return _instance;
            }
            set => _instance = value;
        }

        public static readonly ReactiveProperty<bool> IsLoaded = new(false);

        private static bool _needToInit = true;

        private void Awake()
        {
            if (_needToInit)
                Init();
        }

        private void OnDestroy()
        {
            Debug.Log("DataTableManager OnDestroy!");

            Instance = null;
            _needToInit = true;
        }

        public void Init()
        {
            Debug.Log("DataTableManager Init!");

            _needToInit = false;
#if UNITY_EDITOR
            _instance = this;

            if (!Application.isPlaying)
            {
                Debug.Log("Application is not playing!");
            }
#endif
        }
    }
}