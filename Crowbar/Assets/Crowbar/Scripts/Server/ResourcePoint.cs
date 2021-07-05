using Mirror;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

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

        public bool isSpawned;

        private void CheckToSpawn()
        {
            if (isSpawned)
                return;

            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 300f, LayerMask.GetMask("Player"));

            if (players.Length > 0)
            {
                isSpawned = true;

                foreach (Resources resources in resources)
                {
                    float chance = Random.Range(0, 1f);
                    int count = Random.Range(resources.minCount, resources.maxCount + 1);

                    if (chance < resources.chance)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            GameObject resource = Instantiate(resources.prefab, transform.position, Quaternion.identity, null);

                            NetworkServer.Spawn(resource);
                        }
                    }
                }    
            }
        }

        private void Start()
        {
            if (isServer)
                InvokeRepeating(nameof(CheckToSpawn), 10f, 10f);
        }
    }
}
