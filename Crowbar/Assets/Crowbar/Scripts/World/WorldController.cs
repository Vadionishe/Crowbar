using UnityEngine;
using Mirror;
using Crowbar.Ship;
using Crowbar.Enemy;
using System;
using Random = UnityEngine.Random;

namespace Crowbar
{
    public class WorldController : NetworkBehaviour
    {
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

        private void CheckSpawnShark()
        {
            float chanceValue = Random.Range(0, 1f);

            if (chanceValue < spawnerShark.percentChance)
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
                    }
                }
            }
        }

        private void CheckSpawnSnake()
        {
            float chanceValue = Random.Range(0, 1f);

            if (chanceValue < spawnerWaterSnake.percentChance)
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
