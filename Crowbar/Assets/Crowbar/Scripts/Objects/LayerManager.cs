using UnityEngine;

namespace Crowbar
{
    public class LayerManager : MonoBehaviour
    {
        public static LayerManager instance;

        public float mainCameraLayer = -10;
        public float mainObjectLayer = 0;
        public float mainGroundLayer = 1;
        public float mainWaterLayer = -1;
        public float mainBackwardGroundLayer = 10;
        public float shipLayerIn = -2;
        public float shipLayerOut = 0;
        
        private void Awake()
        {
            instance = this;
        }
    }
}
