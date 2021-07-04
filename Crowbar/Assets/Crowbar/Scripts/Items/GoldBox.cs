using UnityEngine;

namespace Crowbar.Item
{
    public class GoldBox : ItemObject
    {
        public int goldValue;

        public int maxValue;
        public int minValue;

        private void Start()
        {
            if (isServer)
                goldValue = Random.Range(minValue, maxValue + 1);
        }
    }
}
