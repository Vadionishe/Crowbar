using UnityEngine;

namespace Crowbar.Item
{
    public class Bullet : ItemObject
    {
        private void Start()
        {
            if (isServer)
            {
                InvokeRepeating(nameof(CheckToDestroy), 30f, 30f);
                InvokeRepeating(nameof(CheckToSleep), 2f, 2f);
            }
            else
            {
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                colliderItem.isTrigger = true;
            }
        }

        private void Update()
        {
            if (isServer)
                Fall();
        }
    }
}