using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BunnyCoffee
{
    [RequireComponent(typeof(Button))]
    public class UIButtonController : MonoBehaviour
    {
        private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMPro.TMP_Text textPrice;
        [SerializeField] private TMPro.TMP_Text textTooltip;

        void Start()
        {
            button = GetComponent<Button>();
        }

        public void UpdateDisabled(bool value)
        {
            button.interactable = !value;
            Color color = icon.color;
            color.a = value ? 0.2f : 1f;
            icon.color = color;
            textPrice.color = color;
        }

        public void UpdatePrice(string price)
        {
            textPrice.text = price;
        }

        public void UpdateTooltip(string tooltip)
        {
            textTooltip.text = tooltip;
        }
    }
}
