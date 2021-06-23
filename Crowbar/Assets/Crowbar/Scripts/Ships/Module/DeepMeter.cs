using Mirror;
using UnityEngine;

namespace Crowbar.Ship
{
    public class DeepMeter : MonoBehaviour, IShipModule, IPickInfo
    {
        public TextMesh textMesh;

        public float zeroYMeter;
        public float deepMeter;
        public float oneMeter = 1f;

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

        public void Use(NetworkIdentity usingCharacter)
        {
            
        }

        private void Update()
        {
            deepMeter = ((transform.position.y - zeroYMeter) / oneMeter) * -1;

            textMesh.text = $"{deepMeter.ToString("0")} m";
        }
    }
}
