namespace Crowbar
{
    using UnityEngine;
    using Mirror;
    using Crowbar.Item;
    using System.Collections;

    /// <summary>
    /// Using controller for player character
    /// </summary>
    public class UsingComponent : NetworkBehaviour, ICharacterComponent
    {
        #region Variables
        [Header("Using properties")]
        [Tooltip("Enable on server")]
        public bool enableOnServer = false;
        [Tooltip("Distance for use")]
        public float distanceUse = 3f;
        [Tooltip("Key for using")]
        public KeyCode keyUsing = KeyCode.E;
        [Tooltip("Key for drop")]
        public KeyCode keyDrop = KeyCode.Q;

        public float cooldownItemGrab = 0.3f;
        public bool canItemGrab = true;
        #endregion

        #region Fuctions
        /// <summary>
        /// Set state component
        /// </summary>
        /// <param name="isActive">State component</param>
        public void SetComponentActive(bool isActive)
        {
            enabled = isActive;
        }

        /// <summary>
        /// Send command use on server
        /// </summary>
        [Command]
        public void CmdUse(NetworkIdentity useObject)
        {
            GetUse(useObject)?.Use(netIdentity);
        }

        /// <summary>
        /// Send command drop on server
        /// </summary>
        [Command]
        public void CmdDrop()
        {
            ItemObject item = GetComponent<Character>().hand.itemObject;

            if (item != null)
                item.Drop(netIdentity);
        }

        [Server]
        public void SetCooldownGrab()
        {
            StartCoroutine(CooldownItemGrab());
        }

        /// <summary>
        /// Check enable component for server
        /// </summary>
        public bool IsEnableComponentOnServer()
        {
            return enableOnServer;
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                if (!GameUI.lockInput)
                {
                    if (Input.GetKeyDown(keyUsing))
                        CmdUse(GetUseObject());

                    if (Input.GetKeyDown(keyDrop))
                        CmdDrop();
                }               
            }            
        }

        [Server]
        private IUse GetUse(NetworkIdentity useObject)
        {
            if (useObject != null)
                return useObject.GetComponent<IUse>();

            return null;
        }

        [Client]
        private NetworkIdentity GetUseObject()
        {
            if (PickManager.pickObject != null)
                if (Vector2.Distance(transform.position, PickManager.pickObject.transform.position) <= distanceUse)
                    return PickManager.pickObject.GetComponent<NetworkIdentity>();

            return null;
        }

        private IEnumerator CooldownItemGrab()
        {
            canItemGrab = false;

            yield return new WaitForSeconds(cooldownItemGrab);

            canItemGrab = true;
        } 
        #endregion
    }
}