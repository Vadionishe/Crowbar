using Mirror;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Crowbar.Item;

namespace Crowbar
{
    public class ResourcePoint : NetworkBehaviour
    {
        [Serializable]
        public class Resources
        {
            public GameObject prefab;
            public int maxCount;
            public int minCount;
            [Range(0, 1f)]
            public float chance;
        }

        public Resources[] resources;

        public float xRandom;
        public float yRandom;
        public bool isSpawned;

        [ContextMenu("Spawn")]
        public void Spawn()
        {
            foreach (Resources resources in resources)
            {
                float chance = Random.Range(0, 1f);
                int count = Random.Range(resources.minCount, resources.maxCount + 1);

                if (chance < resources.chance)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-xRandom, xRandom), Random.Range(-yRandom, yRandom), 0);
                        GameObject resource = Instantiate(resources.prefab, spawnPosition, Quaternion.identity, null);

                        NetworkServer.Spawn(resource);
                        resource.GetComponent<ItemObject>().Initialize();
                    }
                }
            }
        }

        private void CheckToSpawn()
        {
            if (isSpawned)
                return;

            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 300f, LayerMask.GetMask("Player"));

            if (players.Length > 0)
            {
                isSpawned = true;

                Spawn();
            }
        }

        private void Start()
        {
            if (isServer)
                InvokeRepeating(nameof(CheckToSpawn), 10f, 10f);
        }
    }
}
