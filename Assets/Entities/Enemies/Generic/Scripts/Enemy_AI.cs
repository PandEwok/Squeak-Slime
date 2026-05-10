using UnityEngine;

public class Enemy_AI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void playTurn(GameObject target) {
        Debug.Log($"{this.gameObject.name} played their turn, but no action was defined.");
    }
}
