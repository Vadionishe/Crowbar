namespace Crowbar.Ship
{
    using UnityEngine;
    using Mirror;

    /// <summary>
    /// Module for control ship
    /// </summary>
    public class ControllerShipModule : NetworkBehaviour, IShipModule, IPickInfo
    {
        #region Variables
        [Header("Module properties")]
        [Tooltip("Ship physics model")]
        public PhysicShip physicShip;
        
        public Color PickColor = Color.green;

        private Color m_colorMain = Color.white;

        private NetworkIdentity m_usingCharacter;
        private PlayerInputForServer m_playerInput;
        #endregion

        #region Fuctions
        public void Pick()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = m_colorMain;
        }

        /// <summary>
        /// Drop control module (check died)
        /// </summary>
        [Server]
        public void DropControl(bool isDied)
        {
            if (isDied)
                ClearBusyModule();
        }

        /// <summary>
        /// Drop control module
        /// </summary>
        [Server]
        public void DropControl(float value)
        {
            TargetUsing(m_usingCharacter.connectionToClient, m_usingCharacter, true);
            ClearBusyModule();
        }

        /// <summary>
        /// Use control module
        /// </summary>
        /// <param name="usingCharacter">Character using</param>
        [Server]
        public void Use(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();

            if (!character.isBusy && m_usingCharacter == null && character.hand.itemObject == null)
            {
                character.isBusy = true;
                m_usingCharacter = usingCharacter;
                m_playerInput = usingCharacter.GetComponent<PlayerInputForServer>();
                m_playerInput.onPushQ.AddListener(DropControl);
                usingCharacter.GetComponent<CharacterStats>().onDied.AddListener(DropControl);

                TargetUsing(usingCharacter.connectionToClient, usingCharacter, false);
            }
        }

        /// <summary>
        /// Callback result for use
        /// </summary>
        /// <param name="connection">Target player</param>
        /// <param name="usingCharacter">Character using</param>
        /// <param name="isActiveUse">Result use action</param>
        [TargetRpc]
        public void TargetUsing(NetworkConnection connection, NetworkIdentity usingCharacter, bool isActiveUse)
        {
            MoveComponent moveComponent = usingCharacter.GetComponent<MoveComponent>();
            JumpComponent jumpComponent = usingCharacter.GetComponent<JumpComponent>();
            Character character = usingCharacter.GetComponent<Character>();
            AvatarController avatar = character.avatar.GetComponent<AvatarController>();
            Animator animator = avatar.GetComponentInChildren<Animator>();

            avatar.mouth.SetActive(isActiveUse);
            avatar.glass.SetActive(isActiveUse);

            animator.SetBool("isBusy", !isActiveUse);

            moveComponent.enabled = isActiveUse;
            jumpComponent.enabled = isActiveUse;
            character.handController.enabled = isActiveUse;
            avatar.canFlip = isActiveUse;
            avatar.transform.localScale = new Vector3(1f, 1f, 1f);
            avatar.leftHand.SetActive(true);
            avatar.rightHand.SetActive(false);

            if (!isActiveUse)
                character.handController.transform.eulerAngles = new Vector3(0, 0, 150f);

            usingCharacter.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

            if (!isActiveUse)
            {
                Vector3 currentPosition = usingCharacter.transform.position;

                currentPosition.x = transform.position.x;
                currentPosition.y = transform.position.y;
                usingCharacter.transform.position = currentPosition;
            }
        }

        [Server]
        private void ClearBusyModule()
        {
            m_usingCharacter.GetComponent<Character>().isBusy = false;
            m_usingCharacter.GetComponent<CharacterStats>().onDied.RemoveListener(DropControl);
            m_playerInput.onPushQ.RemoveListener(DropControl);
            m_usingCharacter = null;
            m_playerInput = null;
        }

        private void FixedUpdate()
        {
            if (m_playerInput != null)
                physicShip.Move(new Vector2(m_playerInput.horizontal, m_playerInput.vertical));
        }
        #endregion
    }
}
