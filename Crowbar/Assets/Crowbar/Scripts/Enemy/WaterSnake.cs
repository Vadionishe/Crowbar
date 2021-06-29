using Crowbar.Ship;
using Mirror;
using UnityEngine;

namespace Crowbar.Enemy
{
    public class WaterSnake : Enemy
    {
        public static int count;

        public int maxCrackDamage = 2;
        public int minCrackDamage = 0;
        public float damagePlayers = 1000f;
        public float timeToDestroy = 10f;

        private bool canDamage = true;

        public void Initialize(UnderwaterShip ship)
        {
            Vector3 direction = ship.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        [Server]
        private void CheckShip(Collider2D collision)
        {
            UnderwaterShip ship = collision.GetComponent<UnderwaterShip>();

            if (ship != null && canDamage && !isDied)
            {
                int countCracks = Random.Range(minCrackDamage, maxCrackDamage + 1);

                canDamage = false;

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

                    NetworkServer.Destroy(bullet.gameObject);
                }
            }
        }

        private void DestroySnake()
        {
            NetworkServer.Destroy(gameObject);
        }

        private void Awake()
        {
            networkAnimator.Initialize();
        }

        private void Start()
        {
            if (isServer)
            {                 
                Initialize(FindObjectOfType<UnderwaterShip>());
                Invoke(nameof(DestroySnake), timeToDestroy);
                InvokeRepeating(nameof(CheckToDestroy), timerCheckDestroy, timerCheckDestroy);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isServer)
            {
                CheckShip(collision);
                CheckCharacter(collision);
                CheckBullet(collision);
            }
        }

        private void OnDestroy()
        {
            if (isServer)
                count--;
        }
    }
}
