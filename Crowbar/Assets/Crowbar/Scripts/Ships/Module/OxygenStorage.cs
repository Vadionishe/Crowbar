﻿using UnityEngine;
using Mirror;
using Crowbar.Item;

namespace Crowbar.Ship
{
    public class OxygenStorage : NetworkBehaviour, IShipModule, IPickInfo
    {
        public TextMesh textMesh;

        [SyncVar]
        public float maxOxygen;
        [SyncVar(hook = nameof(SyncValue))]
        public float oxygen;

        public Color PickColor = Color.green;

        private Color m_colorMain;

        public void Pick()
        {
            m_colorMain = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            GetComponent<SpriteRenderer>().color = m_colorMain;
        }

        [Server]
        public void ChangeOxygen(float value)
        {
            oxygen = Mathf.Clamp(oxygen + value, 0, maxOxygen);
        }

        public void Use(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            ItemObject itemObject = character.hand.itemObject;

            if (itemObject != null)
            {
                if (itemObject as OxygenBox)
                {
                    ChangeOxygen((itemObject as OxygenBox).oxygenValue);
                    itemObject.Drop(usingCharacter);
                    NetworkServer.Destroy(itemObject.gameObject);
                }
            }
        }

        [Client]
        private void SyncValue(float oldValue, float newValue)
        {
            textMesh.text = $"{newValue.ToString("0")} L";
        }

        private void Start()
        {
            if (!isServer)
                SyncValue(0, oxygen);
        }
    }
}