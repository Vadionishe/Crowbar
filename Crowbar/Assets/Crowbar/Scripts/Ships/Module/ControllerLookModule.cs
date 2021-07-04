namespace Crowbar.Ship
{
    using UnityEngine;
    using Mirror;

    public class ControllerLookModule : NetworkBehaviour, IShipModule, IPickInfo
    {
        #region Variables
        [Header("Module properties")]
        [Tooltip("Current gun for shit controller")]
        public UnderwaterShip ship;
        [Tooltip("Camera look size")]
        public float lookSize = 10f;

        public Color PickColor = Color.green;

        private Color m_colorMain = Color.white;
        private NetworkIdentity m_usingCharacter;
        private PlayerInputForServer m_playerInput;
        #endregion

        #region Fuctions
        public void Pick()
        {
            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                renderer.color = PickColor;
        }

        public void UnPick()
        {
            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                renderer.color = m_colorMain;
        }

        /// <summary>
        /// Drop control module (check died)
        /// </summary>
        [Server]
        public void DropControl(bool isDied)
        {
            if (isDied)
            {                
                TargetReturnCamera(m_usingCharacter.connectionToClient, m_usingCharacter);
                ClearBusyModule();
            }
        }

        [TargetRpc]
        public void TargetReturnCamera(NetworkConnection connection, NetworkIdentity usingCharacter)
        {
            CameraComponent cameraComponent = usingCharacter.GetComponent<CameraComponent>();

            ship.VisibleInterier(usingCharacter.gameObject.GetComponent<WorldObject>(), true);
            cameraComponent.SetDefaultLookSize();
        }

        /// <summary>
        /// Drop control module
        /// </summary>
        [Server]
        public void DropControl()
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

            if (character.hand.itemObject != null)
                return;

            if (m_usingCharacter != null)
            {
                if (m_usingCharacter == usingCharacter)
                {
                    DropControl();
                }
            }
            else if (!character.isBusy)
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
            CameraComponent cameraComponent = usingCharacter.GetComponent<CameraComponent>();
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

            ship.VisibleInterier(usingCharacter.gameObject.GetComponent<WorldObject>(), isActiveUse);

            if (!isActiveUse)
            {
                Vector3 currentPosition = usingCharacter.transform.position;

                currentPosition.x = transform.position.x;
                currentPosition.y = transform.position.y;
                usingCharacter.transform.position = currentPosition;

                cameraComponent.SetLookSize(lookSize, 1f);
            }
            else
            {
                cameraComponent.SetDefaultLookSize();
            }
        }

        [Server]
        private void ClearBusyModule()
        {
            m_usingCharacter.GetComponent<Character>().isBusy = false;
            m_usingCharacter.GetComponent<CharacterStats>().onDied.RemoveListener(DropControl);
            m_playerInput.onPushQ.RemoveListener(DropControl);
            m_usingCharacter = null;
        }
        #endregion
    }
}