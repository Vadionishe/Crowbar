using UnityEngine;

namespace Crowbar.Item
{
    public class FoodBox : ItemObject
    {
        public float foodValue;

        public int maxValue;
        public int minValue;

        private void Start()
        {
            if (isServer)
            {
                foodValue = Random.Range(minValue, maxValue + 1);

                InvokeRepeating(nameof(CheckToDestroy), 30f, 30f);
            }
            else
            {
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                colliderItem.isTrigger = true;
            }
        }
    }
}
