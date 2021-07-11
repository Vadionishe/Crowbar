using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Crowbar.Enemy
{
    public class Gas : NetworkBehaviour
    {
        public float damage = 1f;
        public float tickDamage = 4f;

        public List<CharacterStats> statsInGas;

        private void Damage()
        {
            foreach (CharacterStats stats in statsInGas)
                stats.ChangeHealth(-damage);
        }

        private void Start()
        {
            if (isServer)
            {
                statsInGas = new List<CharacterStats>();

                InvokeRepeating(nameof(Damage), tickDamage, tickDamage);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isServer)
            {
                CharacterStats stats = collision.GetComponent<CharacterStats>();

                if (stats != null)
                    statsInGas.Add(stats);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (isServer)
            {
                CharacterStats stats = collision.GetComponent<CharacterStats>();

                if (stats != null)
                    statsInGas.Remove(stats);
            }
        }
    }
}
