using Crowbar.Server;
using UnityEngine;
using UnityEngine.UI;

namespace Crowbar
{
    public class SkinHat : MonoBehaviour
    {
        public int id;
        public bool selected;
        public bool isBought;

        public int price;

        public Color selectColor;
        public Color buyColor;
        public Color defaultColor;

        public GameObject buttonBuy;
        public GameObject buttonSelect;

        public void Buy()
        {
            ClientMenu.localPlayerInstance.BuyHat(id, Account.Name, price);
        }

        public void Select()
        {
            FindObjectOfType<SkinShop>().AllDeselect();

            GetComponent<Image>().color = selectColor;

            selected = true;

            buttonBuy.SetActive(false);
            buttonSelect.SetActive(true);

            Account.idCurrentHat = id;

            ClientMenu.localPlayerInstance.SetHat(id, Account.Name);
        }

        public void Deselect()
        {
            if (isBought)
                GetComponent<Image>().color = buyColor;

            selected = false;
        }

        public void SetBuy()
        {
            isBought = true;

            Deselect();

            buttonSelect.SetActive(true);
            buttonBuy.SetActive(false);
        }

        public void NoBuy()
        {
            isBought = false;

            buttonSelect.SetActive(false);
            buttonBuy.SetActive(true);

            GetComponent<Image>().color = defaultColor;
        }
    }
}
