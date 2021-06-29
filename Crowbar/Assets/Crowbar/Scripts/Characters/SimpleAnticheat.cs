using UnityEngine;
using Mirror;

namespace Crowbar
{
    public class SimpleAnticheat : NetworkBehaviour
    {
        public float timeInterval = 5f;
        public float timeTreshold = 1f;

        public float maxDistance = 10f;

        private float timeLastSend;
        private Vector2 oldPosition;

        [Command]
        public void CmdSendCheck()
        {
            if (timeLastSend < timeInterval - timeTreshold)
                TargetCheatDetected(netIdentity.connectionToClient);

            timeLastSend = 0;
        }

        [TargetRpc]
        public void TargetCheatDetected(NetworkConnection connection)
        {
            Application.Quit();
        }

        public void SendValidate()
        {
            CmdSendCheck();
        }

        private void Start()
        {
            if (isLocalPlayer)
                InvokeRepeating(nameof(SendValidate), timeInterval, timeInterval);
        }

        private void FixedUpdate()
        {
            if (isLocalPlayer)
            {
                if (Vector2.Distance(oldPosition, new Vector2(transform.position.x, transform.position.y)) > maxDistance)
                    Application.Quit();

                oldPosition.x = transform.position.x;
                oldPosition.y = transform.position.y;
            }

            if (isServer)
                timeLastSend += Time.fixedDeltaTime;
        }
    }
}