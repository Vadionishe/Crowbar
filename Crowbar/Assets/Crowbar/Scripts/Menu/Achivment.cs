using UnityEngine;
using UnityEngine.UI;

namespace Crowbar
{
    public class Achivment : MonoBehaviour
    {
        public int id;

        public Color activateColor;

        public void SetActive()
        {
            GetComponent<Image>().color = activateColor; 
        }
    }
}
