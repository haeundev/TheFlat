using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.CameraSystem;
using Proto.Enums;
using Proto.SoundSystem;
using Proto.UISystem;
using UnityEngine;
using UnityEngine.Assertions;

namespace Proto.GameModeSystem
{
    public class GameModeService : MonoBehaviour, IGameModeService
    {
        private GameModeService Instance { get; set; }
        private const float DefaultFadeTime = 0.3f;
        private static readonly List<GameMode> BaseGameModes = new() { GameMode.InGame };
        private readonly Stack<GameMode> _gameModeHistoryStack = new();
        private readonly IUiServiceFactory _uiServiceFactory;
        private GameMode _gameMode;
        private GameMode _nextGameMode;

        public GameModeService(IUiServiceFactory uiServiceFactory)
        {
            _uiServiceFactory = uiServiceFactory;
        }

        private void Awake()
        {
            Instance = this;
        }

        public static float FadeTime { get; set; } = DefaultFadeTime;
        public static bool IsReadyToFadeIn { get; set; }
        public event Action<GameMode> OnGameModeEnter = delegate { };
        public event Action<GameMode> OnGameModeExit = delegate { };

        public GameMode GetGameMode()
        {
            return _gameMode;
        }

        public GameMode GetNextGameMode()
        {
            return _nextGameMode;
        }

        public void EnterLoadingGameMode(GameMode gameMode)
        {
            throw new NotImplementedException();
        }

        public void EnterGameMode(GameMode gameMode)
        {
            if (_gameMode == gameMode)
                return;
            _gameModeHistoryStack.Push(_gameMode);

            SetGameMode(gameMode);
        }

        public void LeaveGameMode(GameMode gameMode)
        {
            Assert.AreEqual(gameMode, _gameMode, $"Your game mode is not {gameMode} but {_gameMode}.");
            Assert.IsTrue(_gameModeHistoryStack.Count > 0,
                "The game mode history is empty. Make sure you entered a game mode.");
            var lastGameMode = _gameModeHistoryStack.Pop();
            if (lastGameMode == GameMode.None)
            {
                lastGameMode = GameMode.InGame;
                _gameModeHistoryStack.Push(GameMode.None);
            }

            SetGameMode(gameMode);
        }

        public void ResetGameMode()
        {
            while (_gameModeHistoryStack.Count > 0)
            {
                var stackGameMode = _gameModeHistoryStack.Pop();

                OnGameModeExit?.Invoke(stackGameMode);
                if (_uiServiceFactory.GetUiService(stackGameMode) != null)
                    _uiServiceFactory.GetUiService(stackGameMode).Exit();
                SoundService.Instance.OnGameModeExit(stackGameMode);
            }

            CameraManager.Reset();
            _gameModeHistoryStack.Push(GameMode.InGame);
        }

        public void ForceSetGameMode(GameMode gameMode)
        {
            _gameModeHistoryStack.Clear();
            SetGameMode(gameMode);
        }

        public bool IsCurrentlyInBaseGameMode(GameMode mode)
        {
            return (from gameMode in _gameModeHistoryStack
                where IsClassifiedAsBaseGameMode(gameMode)
                select mode == gameMode).FirstOrDefault();
        }

        public bool IsClassifiedAsBaseGameMode(GameMode mode)
        {
            return BaseGameModes.Contains(mode);
        }

        private void SetGameMode(GameMode gameMode)
        {
            _nextGameMode = gameMode;
            Debug.Log($"SetGameMode : {gameMode}");
            CameraManager.Reset();
            // Leave game mode
            OnGameModeExit?.Invoke(_gameMode);
            if (_uiServiceFactory.GetUiService(_gameMode) != null)
                _uiServiceFactory.GetUiService(_gameMode).Exit();
            SoundService.Instance.OnGameModeExit(_gameMode);

            _gameMode = gameMode;
            if (_uiServiceFactory.GetUiService(_gameMode) != null)
                _uiServiceFactory.GetUiService(_gameMode).Enter();
            SoundService.Instance.OnGameModeEnter(_gameMode);

            OnGameModeEnter?.Invoke(_gameMode);
        }

        private IEnumerator ChangeWithFade(GameMode gameMode)
        {
            CameraFadeService.Out(FadeTime);
            yield return new WaitForSeconds(FadeTime);
            yield return null;
            while (!IsReadyToFadeIn)
                yield return null;

            SetGameMode(gameMode);
            yield return null;
            CameraFadeService.In(FadeTime);
        }

        private IEnumerator ChangeWithFadeOut(GameMode gameMode)
        {
            CameraFadeService.Out(DefaultFadeTime);
            yield return new WaitForSeconds(DefaultFadeTime);
            SetGameMode(gameMode);
            yield return null;
            CameraFadeService.In(0);
        }

        private IEnumerator ChangeWithFadeIn(GameMode gameMode)
        {
            CameraFadeService.Out(0);
            yield return null;
            SetGameMode(gameMode);
            yield return new WaitForSeconds(DefaultFadeTime);
            CameraFadeService.In(DefaultFadeTime);
        }
    }
}