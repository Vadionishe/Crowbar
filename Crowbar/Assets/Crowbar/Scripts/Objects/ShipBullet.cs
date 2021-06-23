namespace Crowbar
{
    using UnityEngine;
    using Mirror;

    public class ShipBullet : WorldObject
    {
        public float damage = 1f;
        public float gravity = 0.06f;

        private Vector2 velocity;

        [Server]
        public void Push(Vector2 force)
        {
            velocity = force;
        }

        private void Start()
        {
            if (!isServer)
                enabled = true;
        }

        private void Update()
        {
            transform.Translate(velocity * Time.deltaTime);
            velocity += new Vector2(0, -gravity);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isServer)
            {
                //Отнимание хп у IHealth (ещё не реализован)

                //NetworkServer.Destroy(gameObject);
            }
        }
    }
}
