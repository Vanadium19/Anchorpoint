namespace InputModule
{
    public interface IInputService
    {
        void Enable();
        void Disable();
        void SetUIMode(bool isActive);
    }
}