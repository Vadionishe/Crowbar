namespace Crowbar.Ship
{
    using UnityEngine;
    using Mirror;
    using Crowbar.Item;

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
        [Tooltip("Camera look size")]
        public float lookSize = 7f;

        [SyncVar]
        public int maxBullet;
        [SyncVar(hook = nameof(SyncValue))]
        public int bullet;
        public SpriteRenderer[] bulletSprites;
        public AudioSource audioSource;

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

        [Client]
        private void SyncValue(int oldValue, int newValue)
        {
            if (oldValue < newValue)
            {
                bulletSprites[newValue - 1].enabled = true;

                audioSource.Play();
            }
            else
            {
                bulletSprites[newValue].enabled = false;
            }
        }

        [Server]
        public void ChangeBullet(int value)
        {
            bullet = Mathf.Clamp(bullet + value, 0, maxBullet);
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
            ItemObject itemObject = character.hand.itemObject;

            if (itemObject != null)
            {
                if (itemObject as Bullet && bullet < maxBullet)
                {
                    ChangeBullet(1);
                    itemObject.Drop(usingCharacter);
                    NetworkServer.Destroy(itemObject.gameObject);
                }
            }
            else
            {
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
                    m_playerInput.onClickLeft.AddListener(shipGun.Shot);
                    m_playerInput.onPushQ.AddListener(DropControl);
                    usingCharacter.GetComponent<CharacterStats>().onDied.AddListener(DropControl);

                    TargetUsing(usingCharacter.connectionToClient, usingCharacter, false);
                }
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
            CameraComponent cameraComponent = usingCharacter.GetComponent<CameraComponent>();
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
            m_playerInput.onClickLeft.RemoveListener(shipGun.Shot);
            m_usingCharacter = null;
            m_playerInput = null;
        }

        private void Start()
        {
            audioSource.volume = Settings.volume;
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
