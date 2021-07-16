using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace Crowbar
{
    public class ShipBullet : WorldObject
    {
        public float damage = 1f;
        public float gravity = 0.06f;

        public string layerName = "GroundCollision";

        public AudioSource audioSource;

        private Vector2 velocity;

        [Server]
        public void OffCollisionShip(NetworkIdentity shipIdentity)
        {
            GameObject ship = shipIdentity.gameObject;
            List<Collider2D> collidersShip = new List<Collider2D>();

            foreach (Collider2D collider in ship.GetComponentsInChildren<Collider2D>())
                if (LayerMask.LayerToName(collider.gameObject.layer) == "GroundCollision")
                    collidersShip.Add(collider);

            foreach (Collider2D collider in collidersShip)
                Physics2D.IgnoreCollision(collider, GetComponent<Collider2D>(), true);
        }

        [Server]
        public void Push(Vector2 force)
        {
            velocity = force;
        }

        private void Start()
        {
            audioSource.volume = Settings.volume;

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
