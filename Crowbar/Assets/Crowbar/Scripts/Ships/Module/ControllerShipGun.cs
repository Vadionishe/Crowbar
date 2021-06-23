namespace Crowbar.Ship
{
    using UnityEngine;
    using Mirror;

    public class ControllerShipGun : NetworkBehaviour, IShipModule, IPickInfo
    {
        #region Variables
        [Header("Module properties")]
        [Tooltip("Gun for shit controller")]
        public ShipGun shipGun;
        [Tooltip("Gun for shit controller")]
        public UnderwaterShip ship;
        [Tooltip("Electric Storage for gun controller")]
        public ElectricStorage electricStorage;
        [Tooltip("Electric cost move gun")]
        public float electricDown = 0.01f;

        public Color PickColor = Color.green;

        private Color m_colorMain;

        private NetworkIdentity m_usingCharacter;
        private PlayerInputForServer m_playerInput;
        #endregion

        #region Fuctions
        public void Pick()
        {
            m_colorMain = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            GetComponent<SpriteRenderer>().color = m_colorMain;
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
            if (usingCharacter.GetComponent<Character>().hand.itemObject != null)
                return;

            if (m_usingCharacter != null)
            {
                if (m_usingCharacter == usingCharacter)
                {
                    DropControl();
                }
            }
            else
            {
                m_usingCharacter = usingCharacter;
                m_playerInput = usingCharacter.GetComponent<PlayerInputForServer>();
                m_playerInput.onClickLeft.AddListener(shipGun.Shot);
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

            ship.VisibleInterier(usingCharacter.gameObject.GetComponent<WorldObject>(), isActiveUse);

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
            m_usingCharacter.GetComponent<CharacterStats>().onDied.RemoveListener(DropControl);
            m_playerInput.onPushQ.RemoveListener(DropControl);
            m_playerInput.onClickLeft.RemoveListener(shipGun.Shot);
            m_usingCharacter = null;
            m_playerInput = null;
        }

        private void FixedUpdate()
        {
            if (m_playerInput != null)
            {
                if (electricStorage.electric > 0)
                {
                    if (m_playerInput.horizontal != 0)
                    {
                        electricStorage.ChangeElectric(-electricDown);

                        shipGun.Rotate(m_playerInput.horizontal);
                    }
                }
            }
        }
        #endregion
    }
}
