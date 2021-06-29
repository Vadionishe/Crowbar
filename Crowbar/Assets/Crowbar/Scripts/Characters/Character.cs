namespace Crowbar
{
    using System;
    using UnityEngine;
    using Mirror;
    using Crowbar.Item;
    using Crowbar.Server;

    /// <summary>
    /// Main component for player representation
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class Character : WorldObject
    {
        #region Variables
        [Serializable]
        public class Hand
        {
            [Header("Hand properties")]
            [Tooltip("Item in hand")]
            public ItemObject itemObject;
            [Tooltip("Hand parent ponint")]
            public Transform handParentPoint;
            [HideInInspector]
            public float velocityAngle;
            [HideInInspector]
            public float newAngle;
            [Tooltip("Rate send package for sync ([1 / sendRate] in sec)")]
            public int sendRate = 10;
            [Tooltip("Smooth speed for rotation")]
            public float smoothRotate = 0.1f; 
        }

        [Header("Character properties")]
        [Tooltip("Name this character")]
        public string nameCharacter;
        [Tooltip("Avatar prefab character")]
        public GameObject avatar;
        [Tooltip("Hand properties")]
        public Hand hand;
        [Tooltip("Hand controller")]
        public HandController handController;
        [Tooltip("Avatar controller")]
        public AvatarController avatarController;
        [Tooltip("Camera component")]
        public CameraComponent cameraComponent;

        [Tooltip("Player audio listener"), SerializeField]
        private AudioListener audioListener;

        [Tooltip("Physic colliders character"), SerializeField]
        private Collider2D[] physicCollider;
        [Tooltip("Trigger colliders character"), SerializeField]
        private Collider2D[] triggerCollider;
       
        private ICharacterComponent[] characterComponents;
        private GameObject m_avatarObject;      
        private Animator m_animator;
        #endregion

        #region Fuctions   
        public override void OnStartClient()
        {
            base.OnStartClient();

            GameUI.SetLoadScreen(false);
        }

        public override void OnStopServer()
        {
            if (hand.itemObject != null)
                hand.itemObject.Drop(netIdentity);

            base.OnStopServer(); 
        }

        [TargetRpc]
        public void TargetCameraShake(NetworkConnection connection, float duration, float magnitude)
        {
            cameraComponent.Shake(duration, magnitude);
        }

        [Command]
        public void CmdSetName(string name)
        {
            nameCharacter = name;

            avatarController.SetName(name);
            RpcSetName(name);
        }

        [ClientRpc]
        public void RpcSetName(string name)
        {
            nameCharacter = name;

            avatarController.SetName(name);
        }

        private void DisableComponents()
        {
            Destroy(GetComponent<Rigidbody>());

            audioListener.enabled = false;

            foreach (ICharacterComponent component in characterComponents) 
            {
                if (isServer)
                {
                    if (!component.IsEnableComponentOnServer())
                        component.SetComponentActive(false);
                }
                else
                {
                    component.SetComponentActive(false);
                }
            }

            foreach (Collider2D collider in physicCollider)
                collider.isTrigger = true;

            foreach (Collider2D collider in triggerCollider)
                collider.enabled = false;
        }

        private void SetPropCharacter(float newAngle, bool isRight, bool isAttack, bool isBusy, Vector3 mousePos)
        {
            handController.gameObject.SetActive(!m_animator.GetBool("isDeath"));

            handController.isAttack = isAttack;
            handController.isRight = isRight;
            handController.mousePosOther = mousePos;

            Vector3 newPos = handController.transform.localPosition;

            newPos.z = (isRight) ? -handController.zPos : handController.zPos;
            hand.newAngle = newAngle;
            avatar.transform.localScale = new Vector3((isRight) ? 1f : -1f, 1f, 1f);
            handController.transform.localPosition = newPos;

            avatarController.leftHand.SetActive(isRight);
            avatarController.rightHand.SetActive(!isRight);
            avatarController.glass.SetActive(!isBusy);
            avatarController.mouth.SetActive(!isBusy);

            if (isBusy)
            {
                avatarController.transform.localScale = new Vector3(1f, 1f, 1f);
                avatarController.leftHand.SetActive(true);
                avatarController.rightHand.SetActive(false);
  
                handController.transform.eulerAngles = new Vector3(0, 0, 150f);
            }
        }

        [ClientRpc]
        private void RpcSyncCharacter(float newAngle, bool isRight, bool isAttack, bool isBusy, Vector3 mousePos)
        {
            if (!isLocalPlayer && handController != null)
            {
                SetPropCharacter(newAngle, isRight, isAttack, isBusy, mousePos);
            }              
        }

        [Command]
        private void CmdSyncCharacter(float newAngle, bool isRight, bool isAttack, bool isBusy, Vector3 mousePos)
        {
            RpcSyncCharacter(newAngle, isRight, isAttack, isBusy, mousePos);
            SetPropCharacter(newAngle, isRight, isAttack, isBusy, mousePos);

            if (hand.itemObject != null)
                hand.itemObject.isAttacking = isAttack;
        }

        private void SyncCharacter()
        {
            float angleHand = handController.transform.eulerAngles.z;
            bool isRight = avatar.transform.localScale.x > 0;

            CmdSyncCharacter(angleHand, isRight, handController.isAttack, m_animator.GetBool("isBusy"), Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        private void Awake()
        {
            m_avatarObject = Instantiate(avatar);
            avatarController = m_avatarObject.GetComponent<AvatarController>();
            m_animator = avatarController.GetComponentInChildren<Animator>();

            GetComponent<MoveComponent>().animator = m_animator;
            GetComponent<GravityComponent>().animator = m_animator;
            GetComponent<NetworkAnimator>().animator = m_animator;

            GetComponent<NetworkAnimator>().Initialize();
        }

        private void Start()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, LayerManager.instance.mainObjectLayer);
            characterComponents = GetComponents<ICharacterComponent>();
            handController = m_avatarObject.GetComponentInChildren<HandController>();
            m_avatarObject.transform.position = transform.position;
            avatarController.target = transform;
            avatarController.isLocal = isLocalPlayer;
            handController.localHand = isLocalPlayer;
            avatar = m_avatarObject;
            hand.handParentPoint = handController.grabPosition;

            if (isServer || !isLocalPlayer)
                DisableComponents();

            if (isLocalPlayer)
            {
                GameUI.Initialize(this);

                CmdSetName(Account.Name);
                InvokeRepeating(nameof(SyncCharacter), 1f / hand.sendRate, 1f / hand.sendRate);
            }
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
            }
            else
            {
                if (handController.isAttack)
                {
                    handController.HandAttackOtherPlayer();
                }
                else
                {
                    float newAngle = Mathf.SmoothDampAngle(handController.transform.eulerAngles.z, hand.newAngle, ref hand.velocityAngle, hand.smoothRotate);
                    handController.transform.eulerAngles = new Vector3(0, 0, newAngle);
                }
            }
        }

        private void OnDestroy()
        {
            Destroy(avatar);

            if (isServer)
                GetComponent<CharacterStats>().onDied.Invoke(true);
        }
        #endregion
    }
}
