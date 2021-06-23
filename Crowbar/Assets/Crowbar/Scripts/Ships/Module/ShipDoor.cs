namespace Crowbar.Ship
{
    using UnityEngine;
    using Mirror;

    public class ShipDoor : NetworkBehaviour
    {
        public WaterCheckerShip waterChecker;
        public Water waterShip;

        public float waterUp = 2f;

        public Vector3 openPosition;
        public Vector3 closePosition;

        public float speedMove = 1f;
        public float smooth = 0.1f;
        public float stopDistance = 0.05f;
        public bool isOpen = true;

        private Vector3 velocity;

        [ClientRpc]
        public void RpcSetState(bool state)
        {
            isOpen = state;
        }

        [Server]
        public void SetState(bool state)
        {
            isOpen = state;

            RpcSetState(isOpen);
        }

        private void Update()
        {
            Vector3 targetPosition = isOpen ? openPosition : closePosition;

            if (Vector3.Distance(transform.localPosition, targetPosition) > stopDistance)
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref velocity, smooth, speedMove);

            if (isServer)
            {
                WaterFall();
            }        
        }

        [Server]
        private void WaterFall()
        {
            if (waterChecker.CheckCollision() && isOpen)
                waterShip.ChangeHeight(waterUp);
        }
    }
}