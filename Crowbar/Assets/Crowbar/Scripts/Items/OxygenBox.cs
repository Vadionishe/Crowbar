using UnityEngine;

namespace Crowbar.Item
{
    public class OxygenBox : ItemObject
    {
        public float oxygenValue;

        public int maxValue;
        public int minValue;

        private void Start()
        {
            if (isServer)
                oxygenValue = Random.Range(minValue, maxValue + 1);
        }
    }
}
