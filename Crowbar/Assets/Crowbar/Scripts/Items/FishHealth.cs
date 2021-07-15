using Mirror;
using System.Collections;
using UnityEngine;
using Crowbar.Ship;

namespace Crowbar.Item
{
    public class FishHealth : ItemObject
    {
        public static int count;

        public float healthValue;

        public int maxValue;
        public int minValue;

        public bool isDied;
        public float speedWalk;
        public float speedRun;
        public float speed;

        public void SetResource(float value)
        {
            healthValue = value;
        }

        public override void Initialize()
        {
            healthValue = Random.Range(minValue, maxValue + 1);
        }

        public override void CheckValidSpawn()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, LayerMask.GetMask("GroundCollision", "PhysicShip"));

            if (new Vector2(hit.point.x, hit.point.y) == new Vector2(transform.position.x, transform.position.y))
                NetworkServer.Destroy(gameObject);
        }

        [Server]
        public override void UseItem()
        {
            base.UseItem();

            Character character = handedCharacter.GetComponent<Character>();
            CharacterStats stats = handedCharacter.GetComponent<CharacterStats>();

            character.hand.itemObject = null;

            stats.ChangeHealth(healthValue);
            NetworkServer.Destroy(gameObject);
        }

        [Server]
        public override void Grab(NetworkIdentity usingCharacter)
        {
            if (isDied)
            {
                base.Grab(usingCharacter);
            }
            else
            {
                StartCoroutine(SpeedRun());
            }
        }

        public override void CheckToSleep()
        {
            if (isDied)
                base.CheckToSleep();
        }

        [Server]
        public void SetAngle()
        {
            if (!isDied)
            {
                transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
                rigidbodyItem.velocity = transform.right * speed;
            }
        }

        [Server]
        public void SetAngle(float angle)
        {
            if (!isDied)
            {
                transform.eulerAngles = new Vector3(0, 0, angle);
                rigidbodyItem.velocity = transform.right * speed;
            }
        }

        [Server]
        public void Death()
        {
            isDied = true;
            canParenting = true;
            transform.eulerAngles = new Vector3(0, 0, 180);
            rigidbodyItem.velocity = Vector2.zero;

            count--;
        }

        [Server]
        private IEnumerator SpeedRun()
        {
            speed = speedRun;

            yield return new WaitForSeconds(4f);

            speed = speedWalk;
        }

        private void Start()
        {
            rigidbodyItem = GetComponent<Rigidbody2D>();
            rigidbodyItem.gravityScale = 0;

            if (isServer)
            {
                canParenting = false;
                speed = speedWalk;
                rigidbodyItem.velocity = transform.right * speed;

                InvokeRepeating(nameof(CheckToDestroy), 30f, 30f);
                InvokeRepeating(nameof(SetAngle), 2f, 2f);
                InvokeRepeating(nameof(CheckToSleep), 2f, 2f);

                CheckValidSpawn();
            }
            else
            {
                colliderItem.isTrigger = true;
            }
        }

        private void Update()
        {
            if (isServer && isDied)
                Fall();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isServer)
            {
                Hammer hammer = collision.GetComponent<Hammer>();
                ShipBullet bullet = collision.GetComponent<ShipBullet>();
                Water water = collision.GetComponent<Water>();
                UnderwaterShip ship = collision.GetComponent<UnderwaterShip>();

                if (ship != null)
                    SetAngle(transform.eulerAngles.z + 180);

                if (bullet != null && !isDied)
                    Death();

                if (hammer != null && !isDied)
                    if (!hammer.onCooldown && hammer.isAttacking)
                        Death();

                if (water != null)
                {
                    if (isDied)
                    {
                        rigidbodyItem.gravityScale = -0.2f;
                        rigidbodyItem.drag = 10f;
                    }
                    else
                    {
                        rigidbodyItem.gravityScale = 0;
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (isServer)
            {
                Water water = collision.GetComponent<Water>();

                if (water != null)
                {
                    rigidbodyItem.drag = 0.2f;
                    rigidbodyItem.gravityScale = 1f;

                    if (!isDied)
                        SetAngle(270f);
                }
            }
        }

        private void OnDestroy()
        {
            if (isServer && !isDied)
                count--;
        }
    }
}
