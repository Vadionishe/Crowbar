using UnityEngine;
using Mirror;

namespace Crowbar.Enemy
{
    public abstract class Enemy : WorldObject
    {
        public Animator animator;
        public NetworkAnimator networkAnimator;

        public bool isDied;

        public float maxHealth;
        public float health;

        public float timerCheckDestroy = 10f;
        public float distanceCheckDestroy = 50f;

        [Server]
        public void ChangeHealth(float value)
        {
            health = Mathf.Clamp(health + value, 0, maxHealth);

            if (health <= 0)
                Died();
        }

        [Server]
        [ContextMenu("Died")]
        public void Died()
        {
            isDied = true;
            animator.SetBool("isDeath", isDied);

            RpcDied();
        }
        
        [ClientRpc]
        public void RpcDied()
        {
            isDied = true;
            animator.SetBool("isDeath", isDied);
        }

        [Server]
        public void CheckToDestroy()
        {
            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, distanceCheckDestroy, LayerMask.GetMask("Player"));

            if (players.Length == 0 && isDied)
                NetworkServer.Destroy(gameObject);

            Collider2D[] _players = Physics2D.OverlapCircleAll(transform.position, distanceCheckDestroy * 5, LayerMask.GetMask("Player"));

            if (_players.Length == 0)
                NetworkServer.Destroy(gameObject);
        }

        [Server]
        public void CheckValidSpawn()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, LayerMask.GetMask("GroundCollision"));

            if (new Vector2(hit.point.x, hit.point.y) == new Vector2(transform.position.x, transform.position.y))
                NetworkServer.Destroy(gameObject);
        }
    }
}
