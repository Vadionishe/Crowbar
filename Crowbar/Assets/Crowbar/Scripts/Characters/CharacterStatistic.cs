using Mirror;

namespace Crowbar
{
    public class CharacterStatistic : NetworkBehaviour
    {
        public float maxDeep;
        public float zeroYMeter;
        public float deepMeter;
        public float oneMeter = 1f;

        private void Update()
        {
            if (isServer)
            {
                deepMeter = ((transform.position.y - zeroYMeter) / oneMeter) * -1;

                if (maxDeep < deepMeter)
                    maxDeep = deepMeter;
            }    
        }
    }
}
