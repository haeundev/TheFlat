namespace Proto.UISystem
{
    public interface IUiService
    {
        void Enter();
        void Exit();
        void Register(IUiServiceFactory factory);
    }
}