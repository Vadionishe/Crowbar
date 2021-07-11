using UnityEngine;
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

        private Color m_colorMain = Color.white;

        public void Pick()
        {
            if (GetComponent<SpriteRenderer>() != null)
                GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            if (GetComponent<SpriteRenderer>() != null)
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
                    itemObject.Drop(usingCharacter, 0, Vector2.zero);
                    NetworkServer.Destroy(itemObject.gameObject);
                }
            }
            else
            {
                float needFood = stats.maxFood - stats.food;

                stats.ChangeFood(needFood <= food ? needFood : food);
                ChangeFood(needFood <= food ? -needFood : -food);             
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
