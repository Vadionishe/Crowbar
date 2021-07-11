using Crowbar.Ship;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Crowbar.Enemy
{
    public class Psycho : Enemy
    {
        public static int count;

        public bool canAttack = true;
        public float timeAttack = 5f;
        public float cooldown = 8f;

        public float damage = 4f;

        public MoveShark moveShark;
        public AudioSource audioSource;

        [Server]
        public void Attack()
        {
            if (canAttack && !isDied)
            {
                CharacterStats[] characterStats = FindObjectsOfType<CharacterStats>();
                int statsRandom = Random.Range(0, characterStats.Length);

                characterStats[statsRandom].ChangeHealth(-damage);

                StartCoroutine(Cooldown());
                StartCoroutine(Attacking());
            }
        }

        [Server]
        private void CheckBullet(Collider2D collision)
        {
            ShipBullet bullet = collision.GetComponent<ShipBullet>();

            if (bullet != null)
            {
                if (!isDied)
                {
                    ChangeHealth(-bullet.damage);

                    NetworkServer.Destroy(bullet.gameObject);
                }
            }
        }

        private IEnumerator Cooldown()
        {
            canAttack = false;

            yield return new WaitForSeconds(cooldown);

            canAttack = true;
        }

        private IEnumerator Attacking()
        {
            animator.SetBool("isAttack", true);

            yield return new WaitForSeconds(timeAttack);

            animator.SetBool("isAttack", false);
        }

        private void Awake()
        {
            networkAnimator.Initialize();
        }

        private void Start()
        {
            List<PhysicShip> underwaterShips = FindObjectsOfType<PhysicShip>().ToList();

            foreach (PhysicShip ship in underwaterShips)
            {
                Collider2D colliderShip = ship.GetComponent<Collider2D>();
                Collider2D sharkCollider = GetComponent<Collider2D>();
                List<Collider2D> collidersShip = new List<Collider2D>();

                foreach (Collider2D collider in ship.underwaterShip.GetComponentsInChildren<Collider2D>())
                    if (LayerMask.LayerToName(collider.gameObject.layer) == "GroundCollision")
                        Physics2D.IgnoreCollision(collider, sharkCollider, true);

                Physics2D.IgnoreCollision(colliderShip, sharkCollider, true);
            }

            if (isServer)
            {
                InvokeRepeating(nameof(CheckToDestroy), timerCheckDestroy, timerCheckDestroy);

                CheckValidSpawn();
            }
            else
            {
                audioSource.volume = Settings.volume;
                GetComponent<Collider2D>().isTrigger = true;
            }
        }

        private void Update()
        {
            if (isServer)
                Attack();

            if (!isServer && isDied && audioSource.isPlaying)
            {
                audioSource.loop = false;

                audioSource.Stop();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isServer)
                CheckBullet(collision);
        }

        private void OnDestroy()
        {
            if (isServer)
                count--;
        }
    }
}
