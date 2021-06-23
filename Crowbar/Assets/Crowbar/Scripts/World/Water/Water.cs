namespace Crowbar
{
    using UnityEngine;
    using Mirror;

    public class Water : NetworkBehaviour
    {
        public Transform waterParent;

        public bool isSync;
        public float maxWaterY;
        public float minWaterY = 0;
        public float smoothSync = 0.1f;

        private float targetHeight;
        private float veloсityHeight;

        [Server]
        public void ChangeHeight(float addHeight)
        {
            float newHeight = Mathf.Clamp(transform.localScale.y + addHeight, minWaterY, maxWaterY);

            transform.localScale = new Vector3(transform.localScale.x, newHeight, transform.localScale.z);

            RpcSyncHeight(newHeight);
        }

        [ClientRpc]
        public void RpcSyncHeight(float height)
        {
            targetHeight = height;           
        }

        private void Update()
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);

            if (!isServer && isSync) 
            {
                float newHeight = Mathf.SmoothDamp(transform.localScale.y, targetHeight, ref veloсityHeight, smoothSync);

                transform.localScale = new Vector3(transform.localScale.x, newHeight, transform.localScale.z);
            }          
        }
    }
}