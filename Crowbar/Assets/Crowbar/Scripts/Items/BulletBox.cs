using UnityEngine;

namespace Crowbar.Item
{
    public class BulletBox : ItemObject
    {
        public int bulletValue;

        public int maxValue;
        public int minValue;

        private void Start()
        {
            if (isServer)
                bulletValue = Random.Range(minValue, maxValue + 1);
        }
    }
}
