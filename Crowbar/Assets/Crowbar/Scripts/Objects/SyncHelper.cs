using UnityEngine;
using Mirror;

namespace Crowbar
{
    public class SyncHelper : NetworkBehaviour
    {
        public static SyncHelper instance;

        public static void SetParent(GameObject syncObject, NetworkIdentity parent)
        {
            syncObject.transform.SetParent((parent != null) ? parent.transform : null);
        }

        [Server]
        public void SyncCharacter()
        {
            foreach (Character character in FindObjectsOfType<Character>())
            {
                character.RpcSetName(character.nameCharacter);
                character.RpcSetSkin(character.avatarController.idHat);
            }
        }

        private void Start()
        {
            instance = this;
        }
    }
}
