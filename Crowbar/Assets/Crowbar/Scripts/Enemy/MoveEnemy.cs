using UnityEngine;
using Mirror;

namespace Crowbar.Enemy
{
    public abstract class MoveEnemy : NetworkBehaviour
    {
        public float speed;

        protected Rigidbody2D m_rigidbody;

        private void Start()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();
        }
    }
}
