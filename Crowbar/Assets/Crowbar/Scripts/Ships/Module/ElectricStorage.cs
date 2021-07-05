using UnityEngine;
using Mirror;
using Crowbar.Item;

namespace Crowbar.Ship
{
    public class ElectricStorage : NetworkBehaviour, IShipModule, IPickInfo
    {
        public TextMesh textMesh;

        [SyncVar]
        public float maxElectric;
        [SyncVar(hook = nameof(SyncValue))]
        public float electric;

        public Color PickColor = Color.green;

        private Color m_colorMain = Color.white;

        public void Pick()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = m_colorMain;
        }

        [Server]
        public void ChangeElectric(float value)
        {
            electric = Mathf.Clamp(electric + value, 0, maxElectric);
        }

        public void Use(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            ItemObject itemObject = character.hand.itemObject;

            if (itemObject != null)
            {
                if (itemObject as ElectricBox)
                {
                    ChangeElectric((itemObject as ElectricBox).electricValue);
                    itemObject.Drop(usingCharacter);
                    NetworkServer.Destroy(itemObject.gameObject);
                }
            }
        }

        [Client]
        private void SyncValue(float oldValue, float newValue)
        {
            textMesh.text = $"{newValue.ToString("0")} mAh";
        }

        private void Start()
        {
            if (!isServer)
                SyncValue(0, electric);
        }
    }
}
