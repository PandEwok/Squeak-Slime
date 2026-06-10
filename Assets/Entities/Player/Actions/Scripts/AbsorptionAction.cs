using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "AbsorptionAction", menuName = "PlayerAction/AbsorptionAction")]
public class AbsorptionAction : PlayerAction
{
    public override void Execute(Player player)
    {
        player.GetComponent<Stats_System>().ActivateAbsorption();
        player.SwitchingTurn();
    }
}
