using TMPro;
using UnityEngine;

namespace BunnyCoffee
{
    [RequireComponent(typeof(Animator))]
    public class UIController : MonoBehaviour
    {
        private Animator animator;

        [SerializeField] GameManager game;
        [SerializeField] TMP_Text textMoney;
        [SerializeField] UIButtonController buttonEmployee;
        [SerializeField] UIButtonController buttonAppliance;
        [SerializeField] UIButtonController buttonUpgradeAppliance;
        [SerializeField] UIButtonController buttonDecoration;

        int lastMoney = 0;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void SetShowIntro(bool value)
        {
            animator.SetBool("Intro", value);
        }

        public void SetShowBackdrop(bool value)
        {
            animator.SetBool("Backdrop", value);
        }

        public void SetShowHeader(bool value)
        {
            animator.SetBool("Header", value);
        }

        public void SetMoney(int money)
        {
            textMoney.text = money.ToString();

            if (money > lastMoney)
            {
                animator.SetTrigger("Money");
            }

            lastMoney = money;
        }

        public void ToggleManagementPanel()
        {
            animator.SetBool("Management", !animator.GetBool("Management"));
        }

        public void UpdateEmployeeButton(int quantity, string price, bool isDisabled)
        {
            buttonEmployee.UpdatePrice(price);
            buttonEmployee.UpdateDisabled(isDisabled);
            buttonEmployee.UpdateTooltip(quantity.ToString());
        }

        public void UpdateBuyApplianceButton(int quantity, string price, bool isDisabled)
        {
            buttonAppliance.UpdatePrice(price);
            buttonAppliance.UpdateDisabled(isDisabled);
            buttonAppliance.UpdateTooltip(quantity.ToString());
        }

        public void UpdateUpgradeApplianceButton(int quantity, string price, bool isDisabled)
        {
            buttonUpgradeAppliance.UpdatePrice(price);
            buttonUpgradeAppliance.UpdateDisabled(isDisabled);
            buttonUpgradeAppliance.UpdateTooltip(quantity.ToString());
        }

        public void UpdateDecorationButton(int quantity, string price, bool isDisabled)
        {
            buttonDecoration.UpdatePrice(price);
            buttonDecoration.UpdateDisabled(isDisabled);
            buttonDecoration.UpdateTooltip(quantity.ToString());
        }
    }
}