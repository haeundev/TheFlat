using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Util;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using AudioType = Proto.Enums.AudioType;

namespace Proto.SoundSystem
{
    public enum VolumeType
    {
        GameSound,
        VoiceSound
    }

    public class SoundService : MonoBehaviour
    {
        private const string AudioMixerName = "MasterAudioMixer";
        private const string DefaultSnapshotName = "OutGame";
        private const string DefaultAudioClipOnError = "Sound/UI/UI_Button2.ogg";

        private static SoundServiceSettings _soundServiceSettings;

        private static SoundService _instance;
        private static bool _destroyed;

        private static int _key;
        private static readonly Dictionary<int, Audio> AudioDict = new();
        private static readonly Dictionary<string, Audio> BGMDictionary = new();
        private static readonly Dictionary<string, Audio> EnvironmentDictionary = new();

        private const float DeltaVolumeSpeed = 0.5f;

        private bool _applicationPaused;

        private AudioMixer _audioMixer;
        private Audio _currentBGM;

        private GameMode _currentGameMode;
        private string _currentZoneBGMPath = "BGM/Field_BGM";
        private AudioMixerSnapshot _defaultSnapshot;

        private Audio _helperSound;
        private IEnumerator _volumeDownCoroutine;
        private IEnumerator _volumeUpCoroutine;
        public static string CurrentBGM { get; private set; }

        public static SoundService Instance
        {
            get
            {
                _instance ??= FindObjectOfType<SoundService>();
                return _instance;
            }
        }

        public string CurrentSnapshotName { get; private set; }

        private void Update()
        {
            AudioDict
                .Select(pair => pair.Value).ToList()
                .ForEach(audio => audio.Update());
        }

        private void OnDestroy()
        {
            _destroyed = true;
        }

        private void LoadAudioMixer()
        {
            _audioMixer = Resources.Load<AudioMixer>(AudioMixerName);
            _defaultSnapshot = _audioMixer.FindSnapshot(DefaultSnapshotName);
        }
        
        public static IEnumerator PlaySimple(string path, AudioSource audioSource)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(path);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
                audioSource.clip = handle.Result;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
            Addressables.Release(handle);
        }

        public static IEnumerator Load(string path, AudioSource audioSource, bool playImmediately = false)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(path);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
                audioSource.clip = handle.Result;
            else
                Debug.LogError($"Sound does not exist: {path}");

            if (playImmediately)
            {
                audioSource.Play();
                yield return new WaitWhile(() => audioSource.isPlaying);
                Addressables.Release(handle);
            }
            else
            {
                Addressables.Release(handle);
            }
        }

        public static Audio Play(AudioType type, string name, Vector3 position, bool loop)
        {
            if (_destroyed)
                return null;

            if (type == AudioType.Invalid)
            {
                GenerateAudioError(name);
                return null;
            }

            var path = $"{type.ToString()}/{name}";
            var audio = new Audio(Instance, _key, type, path, position, loop);
            AudioDict.Add(_key++, audio);
            ApplySoundSettings(path, type, audio);
            audio.Play();
            return audio;
        }

        public static void PlayLoop(float time, string name, Vector3 pos = default, float minRadius = 0f,
            float maxRadius = 0f)
        {
            var audio = Play(name, pos, true, minRadius, maxRadius);
            _instance.DoWaitForSeconds(time, audio.Stop);
        }

        public static Audio Play(string name, Vector3 pos = default, bool loop = false, float minRadius = 0f,
            float maxRadius = 0f)
        {
            if (_destroyed)
                return null;
            
            var slashIndex = name.IndexOf('/');
            if (slashIndex < 0)
            {
                GenerateAudioError(name);
                return null;
            }

            var type = ParseAudioType(name.Substring(0, slashIndex));
            if (type == AudioType.Invalid)
            {
                GenerateAudioError(name);
                return null;
            }

            var audio = new Audio(Instance, _key, type, name, pos, loop);
            AudioDict.Add(_key++, audio);
            ApplySoundSettings(name, type, audio);
            audio.Play();
            return audio;
        }
        
        public static Audio PlayAudioClip(AudioType type, AudioClip clip, bool autoReleased)
        {
            var audio = Audio.Create(Instance, _key, type, clip, autoReleased);
            AudioDict.Add(_key++, audio);
            ApplySoundSettings(null, type, audio);
            audio.Play();
            return audio;
        }

        private static AudioType ParseAudioType(string target)
        {
            return Enum.TryParse(target, out AudioType result) ? result : AudioType.Invalid;
        }

        private static void GenerateAudioError(string message)
        {
            Debug.LogError($"Invalid audio error! : {message}");
            Addressables.LoadAssetAsync<AudioClip>(DefaultAudioClipOnError).Completed += handle =>
            {
                var instance = new GameObject("Invalid audio error");
                var audioSource = instance.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.clip = handle.Result;
                audioSource.outputAudioMixerGroup =
                    Instance.GetAudioMixer().FindMatchingGroups(AudioType.UI.ToString())[0];
                audioSource.Play();

                if (!instance)
                    return;

                Destroy(instance, audioSource.clip.length);
                Observable.IntervalFrame(10).TakeUntilDestroy(instance)
                    .Subscribe(_ => { }, () => Addressables.Release(handle));
            };
        }

        public static void PlaySfx(string name, Vector3 position)
        {
            var path = $"{AudioType.SFX.ToString()}/{name}";
            var audio = new Audio(Instance, _key, AudioType.SFX, path, position, false);
            ApplySoundSettings(path, AudioType.SFX, audio);
            AudioDict.Add(_key++, audio);
            audio.Play();
        }

        private static void ApplySoundSettings(string path, AudioType type, Audio audio)
        {
            switch (type)
            {
                case AudioType.Environment:
                    if (path.Contains("3D"))
                        Set3DEnvironmentSoundSettings(audio);
                    else
                        Set2DEnvironmentSoundSettings(audio);
                    break;
                case AudioType.SFX:
                    SetSfxSoundSettings(audio);
                    break;
            }
        }

        private static void Set2DEnvironmentSoundSettings(Audio audio)
        {
            audio.AudioSource.spatialBlend = 0f;
        }

        private static void Set3DEnvironmentSoundSettings(Audio audio)
        {
            audio.AudioSource.spatialBlend = 1.0f;
            audio.AudioSource.spread = 360.0f;
            audio.AudioSource.rolloffMode = AudioRolloffMode.Custom;
            audio.AudioSource.minDistance = 4.0f; // will be controlled by the curve.
            audio.AudioSource.maxDistance = 15.0f;
            audio.AudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff,
                _soundServiceSettings.RolloffCurveFor3DEnvironmentSound);
        }

        private static void SetSfxSoundSettings(Audio audio)
        {
            audio.AudioSource.spatialBlend = 1.0f;
            audio.AudioSource.spread = 180.0f;
            audio.AudioSource.rolloffMode = AudioRolloffMode.Linear;
            audio.AudioSource.minDistance = 50.0f;
            audio.AudioSource.maxDistance = 100.0f;
        }

        private static void SetAmbientSoundSettings(Audio audio, float minRadius, float maxRadius)
        {
            audio.AudioSource.spatialBlend = 1.0f; // this must be 3d sound
            audio.AudioSource.spread = 180.0f;
            audio.AudioSource.rolloffMode = AudioRolloffMode.Linear;
            audio.AudioSource.minDistance = minRadius;
            audio.AudioSource.maxDistance = maxRadius;
        }

        public static void PlayBGM(string name, Vector3 pos = default, bool isBlend = false)
        {
            if (Instance == default || string.IsNullOrEmpty(name))
                return;

            if (BGMDictionary != null && BGMDictionary.Count > 0)
                foreach (var bgm in BGMDictionary.Keys.ToList())
                    StopBGM(bgm, isBlend);

            var audio = new Audio(Instance, _key, AudioType.BGM, name, pos, true);
            audio.Play();
            if (isBlend)
                Instance.StartVolumeUpCoroutine(audio);

            AudioDict.Add(_key++, audio);
            BGMDictionary?.Add(name, audio);
            _instance._currentBGM = audio;
            CurrentBGM = name;
        }

        public static Audio GetBGM(string name)
        {
            return BGMDictionary.ContainsKey(name) ? BGMDictionary?[name] : null;
        }

        private void StartResumeCoroutine(Audio audio, float volumeSpeed = -1)
        {
            if (audio == null) return;
            audio.Resume();
            StartVolumeUpCoroutine(audio, volumeSpeed);
        }

        private void StartVolumeUpCoroutine(Audio audio, float volumeSpeed = -1)
        {
            if (_volumeUpCoroutine != null)
                StopCoroutine(_volumeUpCoroutine);
            _volumeUpCoroutine = VolumeUpCoroutine(audio, volumeSpeed);
            StartCoroutine(_volumeUpCoroutine);
        }

        private IEnumerator VolumeUpCoroutine(Audio audio, float volumeSpeed)
        {
            float volume = 0;
            if (volumeSpeed < 0)
                volumeSpeed = DeltaVolumeSpeed;
            while (audio != null && audio.AudioSource != null && volume < 1)
            {
                volume += Time.deltaTime * volumeSpeed;
                if (volume > 1)
                    volume = 1;
                audio.AudioSource.volume = volume;
                yield return null;
            }
        }

        private void StartVolumeDownCoroutine(Audio audio, float volumeSpeed = -1)
        {
            if (_volumeDownCoroutine != null)
            {
                audio.Stop();
            }
            else
            {
                _volumeDownCoroutine = VolumeDownCoroutine(audio, volumeSpeed, false);
                StartCoroutine(_volumeDownCoroutine);
            }
        }

        private void StartPauseCoroutine(Audio audio, float volumeSpeed = -1)
        {
            if (_volumeDownCoroutine != null)
            {
                audio.Pause();
            }
            else
            {
                _volumeDownCoroutine = VolumeDownCoroutine(audio, volumeSpeed, true);
                StartCoroutine(_volumeDownCoroutine);
            }
        }

        private IEnumerator VolumeDownCoroutine(Audio audio, float volumeSpeed, bool isPause)
        {
            if (audio != null && audio.AudioSource != null)
            {
                if (volumeSpeed < 0)
                    volumeSpeed = DeltaVolumeSpeed;
                while (audio.AudioSource.volume > 0)
                {
                    yield return null;
                    if (audio == null || audio.AudioSource == null) yield break;
                    audio.AudioSource.volume -= Time.deltaTime * volumeSpeed;
                }

                if (isPause)
                    audio.Pause();
                else
                    audio.Stop();
            }

            _volumeDownCoroutine = null;
        }

        public static void PauseBGM()
        {
            Instance.StartPauseCoroutine(Instance._currentBGM, 1f);
        }

        public static void ResumeBGM()
        {
            Instance.StartResumeCoroutine(Instance._currentBGM, 1f);
        }

        public static void StopBGM(string name, bool isBlend = false)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (BGMDictionary != null && BGMDictionary.TryGetValue(name, out var audio))
            {
                if (isBlend)
                    Instance.StartVolumeDownCoroutine(audio);
                else
                    audio.Stop();

                BGMDictionary.Remove(name);
            }
        }

        public static void StopAllBGM(bool isBlend = false)
        {
            if (BGMDictionary == null)
                return;

            foreach (var audio in BGMDictionary.Values)
            {
                if (audio == null)
                    continue;

                if (isBlend)
                    Instance.StartVolumeDownCoroutine(audio);
                else
                    audio.Stop();
            }

            BGMDictionary.Clear();
        }

        public static Audio PlayEnvironment(string name, Vector3 pos = default, bool loop = true)
        {
            Audio audio = null;
            audio = Play(name, pos, loop);
            if (EnvironmentDictionary != null && EnvironmentDictionary.ContainsKey(name))
            {
                EnvironmentDictionary[name].Stop();
                EnvironmentDictionary.Remove(name);
            }

            if (audio == null)
            {
                Debug.LogError($"Invalid sound: {name}");
                return null;
            }

            AudioDict.Add(_key++, audio);
            EnvironmentDictionary?.Add(name, audio);
            return audio;
        }

        public static void StopEnvironmentSound(string name)
        {
            if (EnvironmentDictionary != null && EnvironmentDictionary.ContainsKey(name))
            {
                EnvironmentDictionary[name].Stop();
                EnvironmentDictionary.Remove(name);
            }
        }

        public static void StopAllEnvironmentSounds()
        {
            if (EnvironmentDictionary == null)
                return;

            foreach (var audio in EnvironmentDictionary.Values)
            {
                if (audio == null)
                    continue;

                audio.Stop();
            }

            EnvironmentDictionary.Clear();
        }

        public static void ChangeBGMSpeed(string name, float speed)
        {
            if (BGMDictionary != null && BGMDictionary.ContainsKey(name)) BGMDictionary[name].AudioSource.pitch = speed;
        }

        public AudioMixer GetAudioMixer()
        {
            return _audioMixer;
        }

        public void SetAudioMixerVolume(VolumeType volumeType, float volume)
        {
            volume -= 80; //슬라이더는 0~100 기준이고 음량은 -80db ~ 20db 기준이라서 -80 함

            switch (volumeType)
            {
                case VolumeType.GameSound:
                    _audioMixer.SetFloat("BGMVolume", volume);
                    _audioMixer.SetFloat("SFXVolume", volume);
                    break;
                case VolumeType.VoiceSound:
                    _audioMixer.SetFloat("VoiceVolume", volume);
                    break;
            }
        }

        public void Remove(Audio audio)
        {
            var keyValuePair = AudioDict.ToList().Find(pair => pair.Value.Equals(audio));
            AudioDict.Remove(keyValuePair.Key);
        }

        public void TransitToSnapshot(string snapshotName, float time = 0.5f)
        {
            if (snapshotName == default)
                return;
            if (_audioMixer.FindSnapshot(snapshotName) != null)
            {
                Debug.Log($"Change audio snapshot to {snapshotName}.");
                _audioMixer.FindSnapshot(snapshotName).TransitionTo(time);
                CurrentSnapshotName = snapshotName;
            }
            else
            {
                Debug.Log("Change audio snapshot to default.");
                _defaultSnapshot.TransitionTo(time);
                CurrentSnapshotName = DefaultSnapshotName;
            }
        }

        public void OnGameModeEnter(GameMode gameMode)
        {
            var lastBgm = GetBgmName(_currentGameMode);
            _currentGameMode = gameMode;
            _currentZoneBGMPath = "BGM/Field_BGM";
            TransitToSnapshot(gameMode.ToString());
            var bgm = GetBgmName(_currentGameMode);
            if (BGMDictionary.Count == 0)
            {
                PlayBGM(bgm, default, true);
            }
            else if (bgm != lastBgm)
            {
                StopBGM(lastBgm, true);
                PlayBGM(bgm, default, true);
            }
        }

        public void OnGameModeExit(GameMode gameMode)
        {
            
        }

        private string GetBgmName(GameMode gameMode)
        {
            switch (gameMode)
            {
                default:
                    return _currentZoneBGMPath;
            }
        }
    }
}