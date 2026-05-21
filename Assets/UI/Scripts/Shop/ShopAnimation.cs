using UnityEngine;

public class ShopAnimation : MonoBehaviour
{
    public Animator panelAnimator;

    // Changing the name slightly to 'isShopOpen' prevents future confusion!
    // Set this to true if your shop starts visible on screen, or false if it starts hidden.
    private bool isShopOpen = false;

    // Removed the parameter from the parentheses!
    public void ToggleShop()
    {
        // 1. Flip the state cleanly
        isShopOpen = !isShopOpen;

        // 2. Play the correct animation based on the true/false state
        if (isShopOpen)
        {
            panelAnimator.Play("Shop_SlideIn");
        }
        else
        {
            panelAnimator.Play("Shop_SlideOut");
        }
    }
}