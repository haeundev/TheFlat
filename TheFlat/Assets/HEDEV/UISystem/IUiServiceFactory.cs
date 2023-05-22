using Proto.Enums;

namespace Proto.UISystem
{
    public interface IUiServiceFactory
    {
        IUiService GetUiService(GameMode gameMode);
        void RegistUiService(GameMode gameMode, IUiService uiService);
    }
}