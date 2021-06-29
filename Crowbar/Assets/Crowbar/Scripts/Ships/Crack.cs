namespace Crowbar.Ship
{
    using Crowbar.Item;
    using UnityEngine;
    using Mirror;

    public class Crack : NetworkBehaviour
    {
        public static int count;

        public int maxHealth = 6;
        public int curHealth = 6;
        public float waterUp;
        public float[] scaleStates;
        public Water waterPlace;
        public ParticleSystem particle;

        public bool isCollideWaterShip;
        public bool isCollideWaterGlobal;

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
            {
                if (water.waterParent == null && waterPlace != null)
                {
                    if (isServer)
                        waterPlace.ChangeHeight(waterUp);

                    isCollideWaterGlobal = true;
                }
                else
                {
                    isCollideWaterGlobal = false;
                }
            }
            else
            {
                isCollideWaterGlobal = false;
            }
        }

        private void CheckSplash()
        {
            if (isCollideWaterGlobal)
            {
                if (isCollideWaterShip)
                {
                    if (!particle.isStopped)
                        particle.Stop();
                }
                else
                {
                    if (particle.isStopped)
                        particle.Play();
                }
            }
            else
            {
                if (!particle.isStopped)
                    particle.Stop();
            }
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

        private void Update()
        {
            if (!isServer)
                CheckSplash();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Water>() == waterPlace)
                isCollideWaterShip = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<Water>() == waterPlace)
                isCollideWaterShip = false;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (isServer)
                CheckRepair(collision);

            CheckWater(collision);
        }

        private void OnDestroy()
        {
            if (isServer)
                count--;
        }
    }
}
