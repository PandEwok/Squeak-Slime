using UnityEngine;

// This acts as a blueprint. 
public abstract class CustomEffectLogic : ScriptableObject
{
    public abstract void ExecuteEffect(GameObject user, GameObject target);
}