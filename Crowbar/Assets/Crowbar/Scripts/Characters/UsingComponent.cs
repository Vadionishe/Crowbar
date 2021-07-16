using UnityEngine;
using Mirror;
using Crowbar.Item;
using System.Collections;

namespace Crowbar
{
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

        public float force;
        public float forceUp = 2f;
        public float maxForce = 100f;
        public float cooldownUse = 0.3f;
        public bool canUse = true;

        private Character m_character;
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
        public void CmdDrop(float forceDrop, Vector2 direction)
        {
            ItemObject item = GetComponent<Character>().hand.itemObject;

            if (item != null)
                item.Drop(netIdentity, forceDrop, direction, transform.localPosition);
        }

        /// <summary>
        /// Check enable component for server
        /// </summary>
        public bool IsEnableComponentOnServer()
        {
            return enableOnServer;
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

        private IEnumerator CooldownUse()
        {
            canUse = false;

            yield return new WaitForSeconds(cooldownUse);

            canUse = true;
        }

        private void Start()
        {
            m_character = GetComponent<Character>();
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                if (!GameUI.lockInput)
                {
                    if (Input.GetKeyDown(keyUsing) && canUse)
                    {
                        StartCoroutine(CooldownUse());
                        CmdUse(GetUseObject());
                    }

                    if (Input.GetKey(keyDrop))
                        force = Mathf.Clamp(force + forceUp, 0, maxForce);

                    if (Input.GetKeyUp(keyDrop))
                    {
                        if (m_character != null)
                        {
                            Vector2 direction = m_character.avatarController.transform.localScale.x > 0 
                                ? m_character.handController.transform.right
                                : -m_character.handController.transform.right;

                            CmdDrop(force, direction);

                            force = 0;
                        }
                    }                        
                }               
            }            
        }
        #endregion
    }
}