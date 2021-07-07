using Mirror;
using System.Collections;
using UnityEngine;

namespace Crowbar.Item
{
    public class Fish : ItemObject
    {
        public float foodValue;

        public bool isDied;
        public float speedWalk;
        public float speedRun;
        public float speed;

        public Rigidbody2D rigidbodyFish;

        [Server]
        public override void UseItem()
        {
            base.UseItem();

            if (handedCharacter != null)
            {
                Character character = handedCharacter.GetComponent<Character>();
                CharacterStats stats = handedCharacter.GetComponent<CharacterStats>();

                character.hand.itemObject = null;

                stats.ChangeFood(foodValue);
                NetworkServer.Destroy(gameObject);
            }
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
            rigidbodyFish.bodyType = RigidbodyType2D.Dynamic;
            transform.eulerAngles = new Vector3(0, 0, 180);
            rigidbodyFish.velocity = Vector2.zero;
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
            rigidbodyFish.bodyType = RigidbodyType2D.Kinematic;

            if (isServer)
            {
                canParenting = false;
                speed = speedWalk;

                InvokeRepeating(nameof(CheckToDestroy), 30f, 30f);
                InvokeRepeating(nameof(SetAngle), 2f, 2f);
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

                if (hammer != null)
                    if (!hammer.onCooldown && hammer.isAttacking && !isDied)
                        Death();

                if (water != null)
                {
                    if (isDied)
                        rigidbodyFish.gravityScale = -0.2f;

                    rigidbodyFish.drag = 10f;
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
                    if (isDied)
                    {
                        rigidbodyFish.gravityScale = 1f;
                        rigidbodyFish.drag = 0.2f;
                    }
                    else
                    {
                        SetAngle(270f);
                    }
                }                    
            }
        }
    }
}
