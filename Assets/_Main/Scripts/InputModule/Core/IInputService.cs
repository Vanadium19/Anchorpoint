namespace InputModule
{
    public interface IInputService
    {
        void Enable();
        void Disable();
        void SetUIMode(bool isActive);
        void SetBuildMode(bool isActive);
        void Reset();
    }
}