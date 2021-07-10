using UnityEngine;
using Mirror;
using Crowbar.Ship;
using Crowbar.Enemy;
using System;
using Random = UnityEngine.Random;
using Crowbar.Item;

namespace Crowbar
{
    public class WorldController : NetworkBehaviour
    {
        [Serializable]
        public class SpawnerPsycho
        {
            [Header("Spawn psycho properties")]
            public int maxCounts = 10;
            public int minSpawnCount = 2;
            public int maxSpawnCount = 5;
            [Range(0, 1f)]
            public float percentChance = 0.04f;
            public float spawnPositionY = -1000f;
            public float distanceSpawn = 50f;
            public Psycho prefab;
        }

        [Serializable]
        public class SpawnerFish
        {
            [Header("Spawn fish properties")]
            public int maxCounts = 10;
            [Range(0, 1f)]
            public float percentChance = 0.2f;
            public float spawnPositionY = -55f;
            public float distanceSpawn = 50f;
            public Fish prefab;
        }

        [Serializable]
        public class Pressure
        {
            [Header("Pressure properties")]
            public float criticalPressurePositionY = -300f;
            [Range(0, 1f)]
            public float percentChance = 0.1f;
        }

        [Serializable]
        public class SpawnerWaterSnake
        {
            [Header("Spawn Water Snake properties")]
            public int maxCounts = 3;
            [Range(0, 1f)]            
            public float percentChance = 0.1f;
            public float spawnPositionY = -100f;
            public float distanceSpawn = 50f;
            public WaterSnake prefab;        
        }

        [Serializable]
        public class SpawnerShark
        {
            [Header("Spawn Shark properties")]
            public int maxCounts = 5;
            [Range(0, 1f)]
            public float percentChance = 0.2f;
            public float spawnPositionY = -300f;
            public float distanceSpawn = 50f;
            public Shark prefab;
        }

        [Header("Main properties")]
        public float intevalTimeCheck = 5f;

        public Pressure pressure;
        public SpawnerWaterSnake spawnerWaterSnake;
        public SpawnerShark spawnerShark;
        public SpawnerFish spawnerFish;
        public SpawnerPsycho spawnerPsycho;

        private void CheckSpawnShark()
        {
            float chanceValue = Random.Range(0, 1f);

            if (chanceValue < spawnerShark.percentChance && Shark.count < spawnerShark.maxCounts)
            {
                foreach (UnderwaterShip ship in FindObjectsOfType<UnderwaterShip>())
                {
                    if (ship.transform.position.y <= spawnerShark.spawnPositionY)
                    {
                        float randAng = Random.Range(0, 360);
                        float x = ship.transform.position.x + (spawnerShark.distanceSpawn * Mathf.Cos(randAng / (180f / Mathf.PI)));
                        float y = ship.transform.position.y + (spawnerShark.distanceSpawn * Mathf.Sin(randAng / (180f / Mathf.PI)));
                        Vector2 spawnPosition = new Vector2(x, y);

                        Shark spawnedShark = Instantiate(spawnerShark.prefab, spawnPosition, Quaternion.identity, null);

                        NetworkServer.Spawn(spawnedShark.gameObject);

                        Shark.count++;
                    }
                }
            }
        }

        private void CheckSpawnSnake()
        {
            float chanceValue = Random.Range(0, 1f);

            if (chanceValue < spawnerWaterSnake.percentChance && WaterSnake.count < spawnerWaterSnake.maxCounts)
            {
                foreach (UnderwaterShip ship in FindObjectsOfType<UnderwaterShip>())
                {
                    if (ship.transform.position.y <= spawnerWaterSnake.spawnPositionY)
                    {
                        float randAng = Random.Range(0, 360);
                        float x = ship.transform.position.x + (spawnerWaterSnake.distanceSpawn * Mathf.Cos(randAng / (180f / Mathf.PI)));
                        float y = ship.transform.position.y + (spawnerWaterSnake.distanceSpawn * Mathf.Sin(randAng / (180f / Mathf.PI)));
                        Vector2 spawnPosition = new Vector2(x, y);

                        WaterSnake spawnedSnake = Instantiate(spawnerWaterSnake.prefab, spawnPosition, Quaternion.identity, null);

                        NetworkServer.Spawn(spawnedSnake.gameObject);

                        spawnedSnake.Initialize(ship);

                        WaterSnake.count++;
                    }
                }
            }
        }

        private void CheckSpawnFish()
        {
            float chanceValue = Random.Range(0, 1f);

            if (chanceValue < spawnerFish.percentChance && Fish.count < spawnerFish.maxCounts)
            {
                foreach (Character character in FindObjectsOfType<Character>())
                {
                    if (character.transform.position.y <= spawnerFish.spawnPositionY)
                    {
                        float randAng = Random.Range(0, 360);
                        float x = character.transform.position.x + (spawnerFish.distanceSpawn * Mathf.Cos(randAng / (180f / Mathf.PI)));
                        float y = character.transform.position.y + (spawnerFish.distanceSpawn * Mathf.Sin(randAng / (180f / Mathf.PI)));
                        Vector2 spawnPosition = new Vector2(x, y);

                        Fish spawnedFish = Instantiate(spawnerFish.prefab, spawnPosition, Quaternion.identity, null);

                        NetworkServer.Spawn(spawnedFish.gameObject);
                        spawnedFish.Initialize();

                        Fish.count++;
                    }
                }
            }
        }

        private void CheckSpawnPsycho()
        {
            float chanceValue = Random.Range(0, 1f);

            if (chanceValue < spawnerPsycho.percentChance && Psycho.count < spawnerPsycho.maxCounts)
            {
                for (int i = 0; i < Random.Range(spawnerPsycho.minSpawnCount, spawnerPsycho.maxSpawnCount + 1); i++)
                {
                    foreach (UnderwaterShip ship in FindObjectsOfType<UnderwaterShip>())
                    {
                        if (ship.transform.position.y <= spawnerPsycho.spawnPositionY)
                        {
                            float randAng = Random.Range(0, 360);
                            float x = ship.transform.position.x + (spawnerPsycho.distanceSpawn * Mathf.Cos(randAng / (180f / Mathf.PI)));
                            float y = ship.transform.position.y + (spawnerPsycho.distanceSpawn * Mathf.Sin(randAng / (180f / Mathf.PI)));
                            Vector2 spawnPosition = new Vector2(x, y);

                            Psycho spawnedFish = Instantiate(spawnerPsycho.prefab, spawnPosition, Quaternion.identity, null);

                            NetworkServer.Spawn(spawnedFish.gameObject);

                            Psycho.count++;
                        }
                    }
                }
            }
        }

        private void CheckCracks()
        {
            foreach (UnderwaterShip ship in FindObjectsOfType<UnderwaterShip>())
            {
                if (ship.transform.position.y <= pressure.criticalPressurePositionY)
                {
                    float chanceValue = Random.Range(0, 1f);

                    if (chanceValue < pressure.percentChance)
                    {
                        ship.AddCrack();
                    }
                }
            }
        }

        private void CheckInterval()
        {
            CheckCracks();
            CheckSpawnSnake();
            CheckSpawnShark();
            CheckSpawnFish();
            CheckSpawnPsycho();
        }

        private void Start()
        {
            if (isServer)
            {
                InvokeRepeating(nameof(CheckInterval), intevalTimeCheck, intevalTimeCheck);
            }
        }
    }
}
