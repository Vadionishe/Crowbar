using UnityEngine;
using Mirror;
using Crowbar.Item;

namespace Crowbar.Ship
{
    public class FuelStorage : NetworkBehaviour, IShipModule, IPickInfo
    {
        public TextMesh textMesh;

        [SyncVar]
        public float maxFuel;
        [SyncVar(hook = nameof(SyncValue))]
        public float fuel;

        public Color PickColor = Color.green;

        private Color m_colorMain = Color.white;

        public void Pick()
        {
            GetComponent<SpriteRenderer>().color = PickColor;

            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                renderer.color = PickColor;
        }

        public void UnPick()
        {
            GetComponent<SpriteRenderer>().color = m_colorMain;

            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                renderer.color = m_colorMain;
        }

        [Server]
        public void ChangeFuel(float value)
        {
            fuel = Mathf.Clamp(fuel + value, 0, maxFuel);
        }

        public void Use(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            ItemObject itemObject = character.hand.itemObject;

            if (itemObject != null)
            {
                if (itemObject as FuelBox)
                {
                    ChangeFuel((itemObject as FuelBox).fuelValue);
                    itemObject.Drop(usingCharacter, 0, Vector2.zero, Vector2.zero);
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
                SyncValue(0, fuel);
        }
    }
}