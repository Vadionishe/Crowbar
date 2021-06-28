using UnityEngine;
using Mirror;
using Crowbar.Ship;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Crowbar.Enemy
{
    public class Shark : Enemy
    {
        public static int count;

        public int maxCrackDamage = 2;
        public int minCrackDamage = 0;
        public float damagePlayers = 1000f;
        public float cooldownDamage = 2f;

        private bool canDamage = true;

        [Server]
        private void CheckShip(Collider2D collision)
        {
            UnderwaterShip ship = collision.GetComponent<UnderwaterShip>();

            if (ship != null && canDamage && !isDied)
            {
                StartCoroutine(WaitCooldownDamage());

                int countCracks = Random.Range(minCrackDamage, maxCrackDamage + 1);

                for (int i = 0; i < countCracks; i++)
                    ship.AddCrack();
            }
        }

        [Server]
        private void CheckCharacter(Collider2D collision)
        {
            CharacterStats stats = collision.GetComponent<CharacterStats>();

            if (stats != null)
            {
                if (stats.oxygenChecker.place == null && !stats.isDied && !isDied)
                {
                    stats.ChangeHealth(-damagePlayers);
                }
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
                }
            }
        }

        private IEnumerator WaitCooldownDamage()
        {
            canDamage = false;

            yield return new WaitForSeconds(cooldownDamage);

            canDamage = true;
        }

        private void Start()
        {
            if (isServer)
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
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isServer)
            {
                CheckCharacter(collision);
                CheckBullet(collision);
                CheckShip(collision);
            }
        }

        private void OnDestroy()
        {
            if (isServer)
                count--;
        }
    }
}
