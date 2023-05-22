using System;
using Proto.Enums;

namespace Proto.GameModeSystem
{
    public interface IGameModeService
    {
        event Action<GameMode> OnGameModeEnter;
        event Action<GameMode> OnGameModeExit;

        void EnterLoadingGameMode(GameMode gameMode);
        void EnterGameMode(GameMode gameMode);
        void LeaveGameMode(GameMode gameMode);
        void ResetGameMode();
        void ForceSetGameMode(GameMode gameMode);
        GameMode GetGameMode();
        GameMode GetNextGameMode();
        bool IsCurrentlyInBaseGameMode(GameMode mode);
        bool IsClassifiedAsBaseGameMode(GameMode mode);
    }
}