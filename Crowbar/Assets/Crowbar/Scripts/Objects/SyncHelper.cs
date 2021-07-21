using UnityEngine;
using Mirror;

namespace Crowbar
{
    public class SyncHelper : NetworkBehaviour
    {
        public static SyncHelper instance;

        public static void SetParent(GameObject syncObject, NetworkIdentity parent)
        {
            if (syncObject != null)
            {
                if (parent != null && parent.isActiveAndEnabled)
                    syncObject.transform.SetParent(parent.transform);
                else
                    syncObject.transform.SetParent(null);
            }
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
