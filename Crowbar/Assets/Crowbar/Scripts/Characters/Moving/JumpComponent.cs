namespace Crowbar
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Jump controller for player character
    /// </summary>
    public class JumpComponent : MonoBehaviour, ICharacterComponent
    {
        #region Variables
        public float curJumpForce;
        public float curSwimUpForce;
        public float curMaxTimeJump;
        public float curDeltaUpJumpForce;

        [Header("Jump settings")]
        [Tooltip("Enable on server")]
        public bool enableOnServer = false;
        [Tooltip("Can jumping character")]
        public bool canJump = true;
        [Tooltip("Default maximum jump forced")]
        public float defJumpForce = 10f;
        [Tooltip("Default maximum swim up forced")]
        public float defSwimUpForce = 3f;
        [Tooltip("Default maximum time for jumping")]
        public float defMaxTimeJump = 0.35f;
        [Tooltip("Default time up to velocity Y, when jumping")]
        public float defDeltaUpJumpForce = 0.6f;
        [Tooltip("Count jump in air")]
        public float countJumpInAir = 1;
        [Tooltip("KeyCode for jumping")]
        public KeyCode jumpKey = KeyCode.C;

        [Header("Events jump")]
        public UnityEvent onStartJump;
        [HideInInspector]
        public float timeJump;

        [Header("Ground check settings")]
        [Tooltip("Maximum angle normal ground, when can jump"), SerializeField]
        private float maxAngleCanJump = 0.7f;
        [Tooltip("Head checker"), SerializeField]
        private GroundChecker headChecker;
        [Tooltip("Ground checker"), SerializeField]
        private GroundChecker groundChecker;
        [Tooltip("Water checker"), SerializeField]
        private WaterChecker waterChecker;

        private float m_countJump;
        private bool m_lastStateInWater;
        private GravityComponent m_gravityComponent;
        private Rigidbody2D m_rigidBody;
        #endregion

        #region Functions     
        /// <summary>
        /// Set state component
        /// </summary>
        /// <param name="isActive">State component</param>
        public void SetComponentActive(bool isActive)
        {
            enabled = isActive;
        }

        /// <summary>
        /// Check enable component for server
        /// </summary>
        public bool IsEnableComponentOnServer()
        {
            return enableOnServer;
        }

        /// <summary>
        /// Initialization this component
        /// </summary>
        public virtual void Initialize()
        {
            m_rigidBody = GetComponent<Rigidbody2D>();

            if (GetComponent<GravityComponent>())
                m_gravityComponent = GetComponent<GravityComponent>();

            curJumpForce = defJumpForce;
            curMaxTimeJump = defMaxTimeJump;
            curDeltaUpJumpForce = defDeltaUpJumpForce;
            curSwimUpForce = defSwimUpForce;
        }

        /// <summary>
        /// Checked player inputs for control jump
        /// </summary>
        public virtual void InputPlayer()
        {
            if (Input.GetKeyDown(jumpKey))
                Jump();

            if (Input.GetAxis("Vertical") < 0)
            {
                if (groundChecker.CheckCollision(out string tagGround))
                    if (tagGround == "OneWayPlatform")
                        StartCoroutine(JumpDown());
            }
        }

        /// <summary>
        /// If can, start jump once
        /// </summary>
        public virtual void Jump()
        {
            if (canJump && !GameUI.lockInput)
            {
                if (groundChecker.IsGroundRaycasting())
                {
                    SetVelocity(m_rigidBody.velocity.x, (waterChecker.CheckCollision()) ? curSwimUpForce : curJumpForce);
                    InvokeOnStartJump();
                }
                else
                {
                    if (m_countJump < countJumpInAir)
                    {
                        SetVelocity(m_rigidBody.velocity.x, curJumpForce);
                        timeJump = 0;

                        if (m_gravityComponent != null)
                        {
                            m_gravityComponent.m_isFall = false;
                            m_gravityComponent.m_timeFall = 0f;
                        }

                        m_countJump++;
                    }
                }
            }
        }

        /// <summary>
        /// If can, jumping hold
        /// </summary>
        public virtual void HoldJumping()
        {
            if (canJump && !GameUI.lockInput)
            {
                if (Input.GetKey(jumpKey))
                {
                    if (!groundChecker.IsGroundRaycasting() && timeJump < curMaxTimeJump)
                    {
                        if (m_rigidBody.velocity.y < curJumpForce)
                            SetVelocity(m_rigidBody.velocity.x, m_rigidBody.velocity.y + curDeltaUpJumpForce);

                        timeJump += Time.fixedDeltaTime;
                    }

                    if (waterChecker.CheckCollision())
                    {
                        SetVelocity(m_rigidBody.velocity.x, curSwimUpForce);
                    }
                    else if (m_lastStateInWater)
                    {
                        SetVelocity(m_rigidBody.velocity.x, curJumpForce);
                    }
                }

                if (Input.GetKeyUp(jumpKey))
                {
                    if (!groundChecker.IsGroundRaycasting())
                    {
                        timeJump = curMaxTimeJump;
                    }
                }

                m_lastStateInWater = waterChecker.CheckCollision();
            }
        }

        /// <summary>
        /// Set new velocity this character
        /// </summary>
        /// <param name="x">x velocity character</param>
        /// <param name="y">y velocity character</param>
        public virtual void SetVelocity(float x, float y)
        {
            Vector2 velocity = new Vector2(x, y);
            m_rigidBody.velocity = velocity;
        }

        /// <summary>
        /// Check update state jump variable
        /// </summary>
        public virtual void UpdateJump()
        {
            if (groundChecker.CheckCollision())
            {
                timeJump = 0;
                m_countJump = 0;
            }
          
            if (headChecker.CheckCollision(out string tagCollision))
            {
                if (tagCollision != "OneWayPlatform")
                    timeJump = curMaxTimeJump;
            }
        }

        /// <summary>
        /// Invoke event StartJump
        /// </summary>
        public virtual void InvokeOnStartJump()
        {
            onStartJump.Invoke();
        }

        private IEnumerator JumpDown()
        {
            groundChecker.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.1f);

            groundChecker.gameObject.SetActive(true);
        }

        private void Awake()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            HoldJumping();
        }

        private void Update()
        {
            InputPlayer();
            UpdateJump();
        }

        #endregion
    }
}
