namespace Crowbar.Ship
{
    using Crowbar.Item;
    using UnityEngine;
    using Mirror;
    using System.Collections.Generic;

    public class Crack : NetworkBehaviour
    {
        public static int count;

        public int maxHealth = 6;
        public int curHealth = 6;
        public float waterUp;
        public float[] scaleStates;
        public Water waterPlace;

        [ClientRpc]
        public void RpcSyncPosition(Vector3 pos, NetworkIdentity mainParent, string nameParent)
        {
            transform.SetParent(mainParent.transform.Find(nameParent));

            transform.localPosition = pos;
        }

        [ClientRpc]
        public void RpcSyncState(int state)
        {
            SetState(state);
        }

        [Server]
        public void RepairOne(int value)
        {
            curHealth -= value;

            if (curHealth <= 0)
            {
                NetworkServer.Destroy(gameObject);
            }
            else
            {
                SetState(curHealth);
                RpcSyncState(curHealth);
            }
        }

        private void SetState(int state)
        {
            transform.localScale = Vector3.one * scaleStates[state - 1];
        }

        private void CheckWater(Collider2D collision)
        {
            Water water = collision.GetComponent<Water>();

            if (water != null)
                if (water.waterParent == null && waterPlace != null)
                    waterPlace.ChangeHeight(waterUp);
        }

        private void CheckRepair(Collider2D collision)
        {
            Hammer hammer = collision.GetComponent<Hammer>();

            if (hammer != null)
            {
                if (!hammer.onCooldown && hammer.isAttacking)
                {
                    hammer.Attack();
                    RepairOne(hammer.damage);
                }
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (isServer)
            {
                CheckRepair(collision);
                CheckWater(collision);
            }
        }

        private void OnDestroy()
        {
            if (isServer)
                count--;
        }
    }
}
