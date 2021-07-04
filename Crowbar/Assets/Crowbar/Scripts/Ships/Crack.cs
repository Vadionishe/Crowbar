﻿using Crowbar.Item;
using UnityEngine;
using Mirror;

namespace Crowbar.Ship
{
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

        public AudioClip crashSound;
        public AudioClip repaireSound;
        public AudioSource audioSource;

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

            audioSource.PlayOneShot(repaireSound);
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

        private void CheckSplash()
        {
            if (isCollideWaterGlobal)
            {
                if (isCollideWaterShip)
                {
                    if (!particle.isStopped)
                    {
                        particle.Stop();
                        audioSource.Stop();
                    }
                }
                else
                {
                    if (particle.isStopped)
                    {
                        particle.Play();
                        audioSource.Play();
                    }
                }
            }
            else
            {
                if (!particle.isStopped)
                {
                    particle.Stop();
                    audioSource.Stop();
                }
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

        private void Start()
        {
            if (!isServer)
            {                
                waterPlace = GetComponentInParent<UnderwaterShip>().water;
                audioSource.volume = Settings.volume;

                audioSource.PlayOneShot(crashSound);
                particle.Play();
            }
        }

        private void Update()
        {
            if (!isServer)
                CheckSplash();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isServer)
            {
                if (collision.gameObject == waterPlace.gameObject)
                    isCollideWaterShip = true;

                if (collision.GetComponent<Water>() != null)
                    if (collision.GetComponent<Water>().waterParent == null)
                        isCollideWaterGlobal = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!isServer)
            {
                if (collision.gameObject == waterPlace.gameObject)
                    isCollideWaterShip = false;

                if (collision.GetComponent<Water>() != null)
                    if (collision.GetComponent<Water>().waterParent == null)
                        isCollideWaterGlobal = false;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (isServer)
            {
                CheckWater(collision);
                CheckRepair(collision);
            }
        }

        private void OnDestroy()
        {
            if (isServer)
                count--;
        }
    }
}
