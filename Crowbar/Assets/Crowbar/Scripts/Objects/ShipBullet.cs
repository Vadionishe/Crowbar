namespace Crowbar
{
    using UnityEngine;
    using Mirror;

    public class ShipBullet : WorldObject
    {
        public float damage = 1f;
        public float gravity = 0.06f;

        public string layerName = "GroundCollision";

        private Vector2 velocity;

        [Server]
        public void Push(Vector2 force)
        {
            velocity = force;
        }

        private void Start()
        {
            if (!isServer)
                enabled = false;
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
                if (LayerMask.LayerToName(collision.gameObject.layer) == layerName)
                {
                    NetworkServer.Destroy(gameObject);
                }
            }
        }
    }
}
