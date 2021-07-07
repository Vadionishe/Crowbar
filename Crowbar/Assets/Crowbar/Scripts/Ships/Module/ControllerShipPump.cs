using UnityEngine;
using Mirror;

namespace Crowbar.Ship
{
    public class ControllerShipPump : NetworkBehaviour, IShipModule, IPickInfo
    {
        #region Variables
        [Header("Module properties")]
        [Tooltip("Water in ship")]
        public Water waterShip;
        [Tooltip("Electric storage")]
        public ElectricStorage electricStorage;
        public AudioSource audioSource;

        public float maxPumpHeight = 100f;
        public float minPumpHeight = 0;
        [SyncVar]
        public float pumpHeight = 0;
        public float speedPumpMove = 0.1f;
        public float valuePumpedWater = 2f;
        public float electricDown = 0.1f;
        public bool canWaterPumped;

        public float maxScalePump;
        public float minScalePump;
        public GameObject pump;
        public GameObject pumpUp;
        public GameObject targetUp;

        public Color PickColor = Color.green;

        private bool isSoundedPump;
        private Color m_colorMain = Color.white;
        private NetworkIdentity m_usingCharacter;
        private PlayerInputForServer m_playerInput;
        #endregion

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
                ClearBusyModule();
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
        /// Use control pump
        /// </summary>
        /// <param name="usingCharacter">Character using</param>
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
        public void Pump()
        {
            if (electricStorage.electric >= electricDown)
            {
                electricStorage.ChangeElectric(-electricDown);
                waterShip.ChangeHeight(-valuePumpedWater);
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

        private void PumpAnimation()
        {
            float scaleY = minScalePump + (maxScalePump - minScalePump) * (pumpHeight / maxPumpHeight);

            pumpUp.transform.position = new Vector3(targetUp.transform.position.x, targetUp.transform.position.y, targetUp.transform.position.z - 0.0001f);
            pump.transform.localScale = new Vector3(pump.transform.localScale.x, scaleY, pump.transform.localScale.z);

            if (!isSoundedPump && pumpHeight == maxPumpHeight)
            {
                audioSource.Play();
                isSoundedPump = true;
            }

            if (isSoundedPump && pumpHeight == minPumpHeight)
            {
                audioSource.Play();
                isSoundedPump = false;
            }
        }

        private void Start()
        {
            audioSource.volume = Settings.volume;
        }

        private void Update()
        {
            if (!isServer)
                PumpAnimation();
        }

        private void FixedUpdate()
        {           
            if (isServer)
            {
                if (m_playerInput != null)
                    pumpHeight = Mathf.Clamp(pumpHeight + m_playerInput.vertical, 0, maxPumpHeight);

                if (pumpHeight <= minPumpHeight)
                    canWaterPumped = true;

                if (pumpHeight >= maxPumpHeight && canWaterPumped)
                {
                    Pump();

                    canWaterPumped = false;
                }
            }
        }
    }
}
