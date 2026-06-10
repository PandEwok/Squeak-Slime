using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : ScriptableObject
{
    public int actionCost;
    protected float qteSuccessDamageBoost = 1.5f; //multiplicateur
    [SerializeField] protected string attackSoundName;
    [SerializeField] protected string slimeMovingSound;
    public virtual void Execute(Player player, GameObject target)
    {

    }

    public virtual void Execute(Player player, float windowDuration)
    {

    }

    public virtual void Execute(Player player, List<GameObject> targets)
    {

    }

    public virtual void Execute(Player player)
    {
    }
}
