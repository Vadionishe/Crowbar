using UnityEngine;
using UnityEngine.Events;

namespace Crowbar
{
    /// <summary>
    /// Custom component for gravity modeling
    /// </summary>
    public class GravityComponent : MonoBehaviour, ICharacterComponent
    {
        #region Variables
        public float curMaxGravity;
        public float curSpeedUpGravity;
        public float curTimeFallNoDamage;
        public float curModificatorFallDamage;

        [Header("Gravity settings")]
        [Tooltip("Enable on server")]
        public bool enableOnServer = false;
        [Tooltip("Use gravity for this character")]
        public bool onGravity = true;
        [Tooltip("Default maximum gravity (speed fall)")]
        public float defMaxGravity = 20f;
        [Tooltip("Default maximum gravity in water")]
        public float defMaxGravityInWater = 4f;
        [Tooltip("Default time down to velocity Y, when fallen")]
        public float defSpeedUpGravity = 0.8f;

        [Header("Fall settings")]
        [Tooltip("Use damage after fall for this character")]
        public bool canFallDamage = true;
        [Tooltip("Default time fall after which there will be no damage")]
        public float defTimeFallNoDamage = 2f;
        [Tooltip("Default modificator fall damage. [Time Fall * defModificatorFallDamage = Finaly Damage]")]
        public float defModificatorFallDamage = 1f;

        [Tooltip("Animator")]
        public Animator animator;

        [Header("Events gravity")]
        public UnityEvent onGround;
        public UnityEvent onFall;
        public UnityEvent onInWater;
        public UnityEvent onOutWater;

        [Header("Ground check settings")]
        [Tooltip("Radius for check normal grounded"), SerializeField]
        private float heightNormalGroundChecker = 0.32f;
        [Tooltip("Maximum angle normal ground, when gravity disable"), SerializeField]
        private float maxAngleNoGravity = 0.2f;
        [Tooltip("Center for check grounded"), SerializeField]
        private GroundChecker groundChecker;
        [Tooltip("Water checker"), SerializeField]
        private WaterChecker waterChecker;
        [Tooltip("Layer for check collision ground"), SerializeField]
        private LayerMask groundLayer;

        [HideInInspector]
        public bool m_isFall;
        [HideInInspector]
        public float m_timeFall;

        private bool m_inWater;
        private bool m_fallen;
        private bool m_grounded;
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

            m_rigidBody.gravityScale = 0;
            curMaxGravity = defMaxGravity;
            curSpeedUpGravity = defSpeedUpGravity;
            curTimeFallNoDamage = defTimeFallNoDamage;
            curModificatorFallDamage = defModificatorFallDamage;
        }

        /// <summary>
        /// Used gravity force in this character
        /// </summary>
        public virtual void GravityForce()
        {
            if (onGravity)
            {
                if (groundChecker.CheckCollision())
                {
                    if (CheckNormalGround())
                    {
                        if (m_rigidBody.velocity.y > -curMaxGravity)
                        {
                            SetVelocity(m_rigidBody.velocity.x, m_rigidBody.velocity.y - curSpeedUpGravity);
                        }
                    }
                    else
                    {
                        if (m_rigidBody.velocity.y < 0)
                        {
                            SetVelocity(m_rigidBody.velocity.x, 0f);
                        }
                    }
                }
                else
                {
                    if (m_rigidBody.velocity.y > -curMaxGravity)
                        SetVelocity(m_rigidBody.velocity.x, m_rigidBody.velocity.y - curSpeedUpGravity);
                    else
                        SetVelocity(m_rigidBody.velocity.x, -curMaxGravity);
                }
            }
        }

        /// <summary>
        /// Check getting damage after fall
        /// </summary>
        public virtual void CheckFallDamage()
        {
            if (canFallDamage)
            {
                if (m_timeFall >= curTimeFallNoDamage)
                {
                    //TODO: Realization logic damage for falling
                }
            }
        }

        /// <summary>
        /// Check water collision
        /// </summary>
        public virtual void CheckWater()
        {
            m_inWater = waterChecker.CheckCollision();
            curMaxGravity = (m_inWater) ? defMaxGravityInWater : defMaxGravity;
        }

        /// <summary>
        /// Set new velocity this character
        /// </summary>
        /// <param name="x">x velocity character</param>
        /// <param name="y">y velocity character</param>
        public virtual void SetVelocity(float x, float y)
        {
            m_rigidBody.velocity = new Vector2(x, y);
        }

        /// <summary>
        /// Check conditions for invoke events water
        /// </summary>
        public virtual void CheckEventWater()
        {
            if (m_inWater && !waterChecker.CheckCollision())
                onOutWater.Invoke();

            if (!m_inWater && waterChecker.CheckCollision())
                onInWater.Invoke();
        }

        /// <summary>
        /// Check conditions for invoke event fall
        /// </summary>
        public virtual void CheckEventFall()
        {
            if (m_rigidBody.velocity.y < 0)
            {
                if (!m_fallen)
                {
                    InvokeOnFall();
                    m_fallen = true;
                }
            }

            if (m_rigidBody.velocity.y >= 0)
            {
                m_fallen = false;
            }
        }

        /// <summary>
        /// Check conditions for invoke event grounded
        /// </summary>
        public virtual void CheckEventGrounded()
        {
            if (groundChecker.CheckCollision())
            {
                if (!m_grounded)
                {
                    InvokeOnGround();
                    m_grounded = true;
                }
            }
            else
            {
                if (m_grounded)
                {
                    m_grounded = false;
                }
            }
        }

        /// <summary>
        /// Invoke event OnGround
        /// </summary>
        public virtual void InvokeOnGround()
        {
            CheckFallDamage();

            m_isFall = false;
            m_timeFall = 0f;

            onGround.Invoke();
        }

        /// <summary>
        /// Invoke event OnFall
        /// </summary>
        public virtual void InvokeOnFall()
        {
            m_isFall = true;

            onFall.Invoke();
        }

        /// <summary>
        /// Check normal ground
        /// </summary>
        /// <returns>Is normal state</returns>
        public virtual bool CheckNormalGround()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, heightNormalGroundChecker, groundLayer);

            return Mathf.Abs(hit.normal.x) > maxAngleNoGravity;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            animator.SetBool("isGround", groundChecker.IsGroundRaycasting());
        }

        private void FixedUpdate()
        {
            CheckEventFall();
            CheckEventGrounded();
            CheckEventWater();

            GravityForce();
            CheckWater();
            TimerFallDamage();
        }

        private void TimerFallDamage()
        {
            if (m_isFall)
                m_timeFall += Time.fixedDeltaTime;
        }
        #endregion
    }
}
