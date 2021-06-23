namespace Crowbar
{
    /// <summary>
    /// Functionality for managing character components
    /// </summary>
    public interface ICharacterComponent
    {
        void SetComponentActive(bool isActive);
        bool IsEnableComponentOnServer();
    }
}
