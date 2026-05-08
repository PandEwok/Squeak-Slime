using UnityEngine;

public class CheeseScript : BaseItem
{
    public int healthRestoreAmount = 20;

    public override string description => "A piece of cheese that restores " + healthRestoreAmount + " health when used.";

    public override void Use()
    {
        Debug.Log("Player used cheese.");
    }
}
