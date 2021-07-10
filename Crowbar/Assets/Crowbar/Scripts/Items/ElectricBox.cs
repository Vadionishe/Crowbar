using UnityEngine;

namespace Crowbar.Item
{
    public class ElectricBox : ItemObject
    {
        public float electricValue;

        public int maxValue;
        public int minValue;

        public void SetResource(float value)
        {
            electricValue = value;
        }

        public override void Initialize()
        {
            electricValue = Random.Range(minValue, maxValue + 1);
        }

        private void Start()
        {
            if (isServer)
            {
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
