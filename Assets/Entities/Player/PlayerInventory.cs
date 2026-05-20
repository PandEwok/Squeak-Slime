using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class PlayerInventory : MonoBehaviour
{
    public int cheeseInv = 0;
    public int bananaInv = 0;
    public int pepperAttInv = 0;
    public int pepperDefInv = 0;

    public void addCheese(int amount)
    {
        cheeseInv += amount;
        if (cheeseInv > 99)
        {
            cheeseInv = 99;
        }
    }
    public void removeCheese(int amount)
    {
        cheeseInv -= amount;
        if (cheeseInv < 0)
        {
            cheeseInv = 0;
        }
    }
    public void addBanana(int amount)
    {
        bananaInv += amount;
        if (bananaInv > 99)
        {
            bananaInv = 99;
        }
    }
    public void removeBanana(int amount)
    {
        bananaInv -= amount;
        if (bananaInv < 0)
        {
            bananaInv = 0;
        }
    }
    public void addPepperAtt(int amount)
    {
        pepperAttInv += amount;
        if (pepperAttInv > 99)
        {
            pepperAttInv = 99;
        }
    }

    public void removePepperAtt(int amount)
    {
        pepperAttInv -= amount;
        if (pepperAttInv < 0)
        {
            pepperAttInv = 0;
        }
    }
    public void addPepperDef(int amount)
    {
        pepperDefInv += amount;
        if (pepperDefInv > 99)
        {
            pepperDefInv = 99;
        }
    }

    public void removePepperDef(int amount)
    {
        pepperDefInv -= amount;
        if (pepperDefInv < 0)
        {
            pepperDefInv = 0;
        }
    }

}
