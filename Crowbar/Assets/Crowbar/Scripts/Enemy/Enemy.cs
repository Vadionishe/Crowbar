using UnityEngine;
using Mirror;

namespace Crowbar.Enemy
{
    public abstract class Enemy : WorldObject
    {
        public bool canDamaged;
        public bool isDied;

        public float maxHealth;
        public float health;

        [Server]
        public void ChangeHealth(float value)
        {
            health = Mathf.Clamp(health + value, 0, maxHealth);

            if (health <= 0)
                Died();
        }

        [Server]
        public void Died()
        {
            isDied = true;

            RpcDied();
        }
        
        [ClientRpc]
        public void RpcDied()
        {
            isDied = true;
        }
    }
}
