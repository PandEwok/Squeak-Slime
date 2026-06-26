using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PotionProjectile : EnemyProjectile
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    protected override void ProjectileEffect()
    {
        if (sentBack && parent.GetComponent<Alchemist_AI>() != null)
        {
            parent.GetComponent<Stats_System>().MakeDizzy();
            parent.GetComponent<Alchemist_AI>().getDizzy();
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!sentBack)
        {
            if (target.GetComponent<PlayerStats>() != null && parent.GetComponent<Alchemist_AI>() != null)
            {
                if (parent.GetComponent<Alchemist_AI>().GetCurrentPhase() == 2 && target.GetComponent<PlayerStats>().blocking)
                {
                    timer = 0;
                    targetPos = parent.transform.position;
                    parentPos = transform.position;
                    sentBack = true;
                }
            }
        }
        base.Update();
    }
}
