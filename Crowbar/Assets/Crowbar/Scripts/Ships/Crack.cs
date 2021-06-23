namespace Crowbar.Ship
{
    using Crowbar.Item;
    using UnityEngine;
    using Mirror;
    using System.Collections.Generic;

    public class Crack : NetworkBehaviour
    {
        public int maxHealth = 6;
        public int curHealth = 6;
        public float waterUp;
        public Sprite[] spriteStates;
        public Water waterPlace;

        private SpriteRenderer spriteRenderer;
        private Dictionary<int, int> statesInfo;

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
                SetState(statesInfo[curHealth]);
                RpcSyncState(statesInfo[curHealth]);
            }
        }

        private void SetState(int state)
        {
            if (state < spriteStates.Length || state > 0)
                spriteRenderer.sprite = spriteStates[state];
        }

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            statesInfo = new Dictionary<int, int>
                {
                    { 6, 5 },
                    { 5, 4 },
                    { 4, 3 },
                    { 3, 2 },
                    { 2, 1 },
                    { 1, 0 }
                };
        }

        private void FixedUpdate()
        {
            
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (isServer)
            {
                CheckRepair(collision);
                CheckWater(collision);
            }
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
    }
}
