using UnityEngine;
using UnityEngine.Events;
using System;

namespace Crowbar 
{ 
    /// <summary>
    /// Move controller for player character
    /// </summary>
    public class MoveComponent : MonoBehaviour, ICharacterComponent
    {
        #region Variables
        public bool isMove; 
        public float curSpeedMove = 4f; 
        public float modificatorCrunch = 1f; 

        [Header("Move settings")]
        [Tooltip("Enable on server")]
        public bool enableOnServer = false;
        [Tooltip("Can moved character")]
        public bool canMove = true;
        [Tooltip("Maximum speed move character")]
        public float defSpeedMove = 4f;
        [Tooltip("Maximum speed move character in water")]
        public float defSpeedMoveInWater = 2f;
        [Tooltip("Smooth velocity movement")]
        public float movementSmoothing = 0.1f;

        [Tooltip("Animator")]
        public Animator animator;

        [Header("Events move")]
        public UnityEvent onStartMove;
        public UnityEvent onStopMove;
        public UnityEvent onFlip;

        [Header("Ground check settings")]
        [Tooltip("Maximum angle normal ground, when can move"), SerializeField]
        private float maxAngleCanMove = 0.7f;
        [Tooltip("Ground checker"), SerializeField]
        private GroundChecker groundChecker;
        [Tooltip("Water checker"), SerializeField]
        private WaterChecker waterChecker;
        [Tooltip("Character"), SerializeField]
        private Character character;

        private bool m_isMoved;
        private float m_xMove;
        private Vector3 m_velocity;
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
            character = GetComponent<Character>();

            curSpeedMove = defSpeedMove;
        }

        /// <summary>
        /// Checked player inputs for control move
        /// </summary>
        public virtual void InputPlayer()
        {
            m_xMove = Input.GetAxis("Horizontal");
        }

        /// <summary>
        /// Condition move control 
        /// </summary>
        public virtual void Move()
        {
            curSpeedMove = (waterChecker.CheckCollision()) ? defSpeedMoveInWater: defSpeedMove;

            if (canMove && !GameUI.lockInput)
            {
                if (m_xMove == 0)
                {
                    if (m_rigidBody.velocity.x != 0)
                    {
                        SetVelocity(0, m_rigidBody.velocity.y);
                        isMove = false;                        
                    }

                    if (m_isMoved)
                    {
                        InvokeOnStopMove();
                        m_isMoved = false;
                    }
                }
                else
                {
                    float x = m_xMove * curSpeedMove * modificatorCrunch * GetNormalModificator();

                    SetVelocity(x, m_rigidBody.velocity.y);                   

                    isMove = true;

                    if (!m_isMoved)
                    {
                        InvokeOnStartMove();
                        m_isMoved = true;
                    }
                }
            }

            if (GameUI.lockInput)
                SetVelocity(0, m_rigidBody.velocity.y);
        }

        /// <summary>
        /// Set speed animation
        /// </summary>
        public virtual void SetAnimation()
        {
            if (canMove)
            {
                animator.SetFloat("speedRun", Math.Sign(character.avatar.transform.localScale.x) == Math.Sign(m_xMove) ? 1f : -1f);
                animator.SetBool("inWater", waterChecker.CheckCollision());
                animator.SetBool("isRun", m_xMove != 0);

                if (GameUI.lockInput)
                    animator.SetBool("isRun", false);
            }
        }

        /// <summary>
        /// Set new velocity this character
        /// </summary>
        /// <param name="x">x velocity character</param>
        /// <param name="y">y velocity character</param>
        public virtual void SetVelocity(float x, float y)
        {
            Vector3 targetVelocity = new Vector2(x, y);

            m_rigidBody.velocity = Vector3.SmoothDamp(m_rigidBody.velocity, targetVelocity, ref m_velocity, movementSmoothing);
        }

        /// <summary>
        /// Set new velocity this character (forced)
        /// </summary>
        /// <param name="x">x velocity character</param>
        /// <param name="y">y velocity character</param>
        public virtual void SetVelocityForced(float x, float y)
        {
            m_rigidBody.velocity = new Vector2(x, y);
        }

        /// <summary>
        /// Invoke event OnStartMove
        /// </summary>
        public virtual void InvokeOnStartMove()
        {
            onStartMove.Invoke();
        }

        /// <summary>
        /// Invoke event OnStopMove
        /// </summary>
        public virtual void InvokeOnStopMove()
        {
            onStopMove.Invoke();
        }

        /// <summary>
        /// Invoke event OnFlip
        /// </summary>
        public virtual void InvokeOnFlip()
        {
            onFlip.Invoke();
        }

        /// <summary>
        /// Get normal modificator
        /// </summary>
        /// <returns>normal modificator</returns>
        public virtual float GetNormalModificator()
        {
            if (!groundChecker.CheckCollision())
                return 1f;

            if (groundChecker.GetNormalGround() > 0)
            {
                if (m_xMove > 0)
                {
                    return 1f;
                }
                else
                {
                    if (Mathf.Abs(groundChecker.GetNormalGround()) < maxAngleCanMove)
                    {
                        return 1f - Mathf.Abs(groundChecker.GetNormalGround());
                    }
                    else
                    {
                        return 0f;
                    }
                }
            }

            if (groundChecker.GetNormalGround() < 0)
            {
                if (m_xMove > 0)
                {
                    if (Mathf.Abs(groundChecker.GetNormalGround()) < maxAngleCanMove)
                    {
                        return 1f - Mathf.Abs(groundChecker.GetNormalGround());
                    }
                    else
                    {
                        return 0f;
                    }
                }
                else
                {
                    return 1f;
                }
            }

            return 1f;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            InputPlayer();
            SetAnimation();  
        }

        private void FixedUpdate()
        {
            Move();
        }
        #endregion
    }
}