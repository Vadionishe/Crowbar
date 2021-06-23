namespace Crowbar
{
    using Mirror;

    /// <summary>
    /// Functionality for use object
    /// </summary>
    public interface IUse
    {
        [Server]
        void Use(NetworkIdentity usingCharacter);
    }
}