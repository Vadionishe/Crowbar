using Mirror;
using UnityEngine;

namespace Crowbar.Ship
{
    public class ControllerLightModule : NetworkBehaviour, IShipModule, IPickInfo
    {
        public LightSprite2D light2D;
        public ElectricStorage electricStorage;

        public float electricDown = 0.01f;

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
        public void Use(NetworkIdentity usingCharacter)
        {
            if (electricStorage.electric > 0)
            {
                light2D.enabled = !light2D.enabled;

                RpcSetLight(light2D.enabled);
            }
        }

        [ClientRpc]
        private void RpcSetLight(bool enableLight)
        {
            light2D.enabled = enableLight;
        }

        private void Update()
        {
            if (isServer)
            {
                if (light2D.enabled)
                {
                    if (electricStorage.electric > 0)
                    {
                        electricStorage.ChangeElectric(-electricDown);
                    }
                    else
                    {
                        light2D.enabled = false;

                        RpcSetLight(false);
                    }
                }         
            }
        }
    }
}