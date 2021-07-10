using Mirror;
using System.Collections;
using UnityEngine;
using Crowbar.Ship;

namespace Crowbar.Item
{
    public class Fish : ItemObject
    {
        public static int count;

        public float foodValue;

        public int maxValue;
        public int minValue;

        public bool isDied;
        public float speedWalk;
        public float speedRun;
        public float speed;

        public Rigidbody2D rigidbodyFish;

        public void SetResource(float value)
        {
            foodValue = value;
        }

        public override void Initialize()
        {
            foodValue = Random.Range(minValue, maxValue + 1);
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

            stats.ChangeFood(foodValue);
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

        [Server]
        public void SetAngle()
        {
            if (!isDied) 
                transform.eulerAngles = new Vector3(0,0, Random.Range(0, 360));
        }

        [Server]
        public void SetAngle(float angle)
        {
            if (!isDied)
                transform.eulerAngles = new Vector3(0, 0, angle);
        }

        [Server]
        public void Death()
        {
            isDied = true;
            canParenting = true;
            transform.eulerAngles = new Vector3(0, 0, 180);
            rigidbodyFish.velocity = Vector2.zero;

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
            rigidbodyFish = GetComponent<Rigidbody2D>();
            rigidbodyFish.gravityScale = 0;

            if (isServer)
            {
                canParenting = false;
                speed = speedWalk;

                InvokeRepeating(nameof(CheckToDestroy), 30f, 30f);
                InvokeRepeating(nameof(SetAngle), 2f, 2f);

                CheckValidSpawn();
            }
            else
            {
                colliderItem.isTrigger = true;
            }
        }

        private void FixedUpdate()
        {
            if (!isDied)
                rigidbodyFish.velocity = transform.right * speed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isServer)
            {
                Hammer hammer = collision.GetComponent<Hammer>();
                Water water = collision.GetComponent<Water>();
                UnderwaterShip ship = collision.GetComponent<UnderwaterShip>();

                if (ship != null)
                    SetAngle(transform.eulerAngles.z + 180);

                if (hammer != null)
                    if (!hammer.onCooldown && hammer.isAttacking && !isDied)
                        Death();

                if (water != null)
                {
                    if (isDied)
                    {
                        rigidbodyFish.gravityScale = -0.2f;
                        rigidbodyFish.drag = 10f;
                    }
                    else
                    {
                        rigidbodyFish.gravityScale = 0;
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
                    rigidbodyFish.drag = 0.2f;
                    rigidbodyFish.gravityScale = 1f;

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
