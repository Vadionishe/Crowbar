﻿using UnityEngine;
using Mirror;
using Crowbar.Item;

namespace Crowbar.Ship
{
    public class FoodStorage : NetworkBehaviour, IShipModule, IPickInfo
    {
        public TextMesh textMesh;

        [SyncVar]
        public float maxFood;
        [SyncVar(hook = nameof(SyncValue))]
        public float food;

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
        public void ChangeFood(float value)
        {
            food = Mathf.Clamp(food + value, 0, maxFood);
        }

        public void Use(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            CharacterStats stats = usingCharacter.GetComponent<CharacterStats>();
            ItemObject itemObject = character.hand.itemObject;           

            if (itemObject != null)
            {
                if (itemObject as FoodBox)
                {
                    ChangeFood((itemObject as FoodBox).foodValue);
                    itemObject.Drop(usingCharacter);
                    NetworkServer.Destroy(itemObject.gameObject);
                }
            }
            else
            {
                float needFood = stats.maxFood - stats.food;

                ChangeFood(needFood <= food ? -needFood : -food);
                stats.ChangeFood(needFood <= food ? needFood : food);
            }
        }

        [Client]
        private void SyncValue(float oldValue, float newValue)
        {
            textMesh.text = $"{newValue.ToString("0")} g";
        }

        private void Start()
        {
            if (!isServer)
                SyncValue(0, food);
        }
    }
}