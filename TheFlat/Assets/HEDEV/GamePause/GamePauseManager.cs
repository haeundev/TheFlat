using System.Collections.Generic;
using System.Linq;
using Proto.Data;
using Proto.SoundSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Proto.GamePauseSystem
{
    public enum PauseType
    {
        UI,
        FullScreen
    }

    public enum PauseEvent
    {
        Paused,
        Resumed
    }

    public class GamePauseManager : MonoBehaviour
    {
        private readonly List<PauseType> _pauseRequests = new();
        private string _prevSnapshotName;

        private bool _isPaused;
        private static GamePauseManager Instance { get; set; }
        public static bool IsPaused => Instance._isPaused;

        public static bool IsFullScreenPause => Instance._pauseRequests.Contains(PauseType.FullScreen);

        private void Awake()
        {
            Instance = this;
        }
        
        public static bool CheckLastPauseType(PauseType type)
        {
            var lastPauseType = Instance._pauseRequests.LastOrDefault();
            if (lastPauseType == default)
                return false;
            if (lastPauseType == type)
                return true;
            return false;
        }

        public static void Pause(PauseType key, string audioSnapshot = "Pause")
        {
            if (Instance == null || Instance._pauseRequests.Contains(key))
                return;

            SavePrevSnapshot();

            Instance._pauseRequests.Add(key);
            SoundService.Instance.TransitToSnapshot(audioSnapshot);
            Instance._isPaused = true;
            Time.timeScale = 0;
            if (!Instance._eventTable.TryGetValue(PauseEvent.Paused, out var pauseEventList))
                return;

            foreach (var pauseEvent in pauseEventList)
                pauseEvent?.Invoke();
        }

        private static void SavePrevSnapshot()
        {
            if (Instance._pauseRequests.Count <= 0)
                Instance._prevSnapshotName = SoundService.Instance.CurrentSnapshotName;
        }

        public static void Resume(PauseType key)
        {
            if (Instance == null)
                return;

            if (Instance._pauseRequests.Contains(key))
                Instance._pauseRequests.Remove(key);

            if (Instance._eventTable.TryGetValue(PauseEvent.Resumed, out var resumeEventList))
                foreach (var resumeEvent in resumeEventList)
                    resumeEvent?.Invoke();

            if (Instance._pauseRequests.Count > 0)
                return;

            SoundService.Instance.TransitToSnapshot(Instance._prevSnapshotName);
            Instance._isPaused = false;
            Time.timeScale = 1f;
        }

        public static void AllResume()
        {
            Instance._pauseRequests.Clear();
            Instance.Resume();
        }

        private void Resume()
        {
            SoundService.Instance.TransitToSnapshot(_prevSnapshotName);
            _isPaused = false;
            Time.timeScale = 1f;
        }

        public static bool HasPause(PauseType pauseType)
        {
            return Instance._pauseRequests.Contains(pauseType);
        }

        #region Pause Events

        private readonly Dictionary<PauseEvent, List<UnityAction>> _eventTable = new();

        public static void RegisterEvent(PauseEvent pauseEvent, UnityAction handler)
        {
            if (Instance == null)
            {
                Debug.Log("Failed to register game pause events because GamePauseManager Instance is null");
                return;
            }

            if (!Instance._eventTable.ContainsKey(pauseEvent))
                Instance._eventTable.Add(pauseEvent, new List<UnityAction>());

            Instance._eventTable[pauseEvent].Add(handler);
        }

        public static void UnregisterEvent(PauseEvent pauseEvent, UnityAction handler)
        {
            if (Instance == null)
            {
                Debug.Log("Failed to unregister game pause events because GamePauseManager Instance is null");
                return;
            }

            if (!Instance._eventTable.ContainsKey(pauseEvent)) return;
            Instance._eventTable[pauseEvent].Remove(handler);
        }

        #endregion
    }
}