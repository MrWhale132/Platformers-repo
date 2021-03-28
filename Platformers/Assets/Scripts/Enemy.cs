using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy : MonoBehaviour, IDamagable
{
    public enum EnemyState { Idle = 0, Chase = 1, Atack = 2 }
    [HideInInspector]
    public EnemyState enemyState;

    Platform platform;

    bool dead;

    [SerializeField]
    int startHelath;

    int health;


    public Platform Platform => platform;


    protected virtual void Start()
    {
        platform = MapGenerator.GetPlatformFromPosition(transform.position);
        platform.objAtPlatform = this;
        platform.walkable = false;
        health = startHelath;
    }

    protected virtual void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));    
    }
    
    public void CallOnMouseUpAsButton()
    {
        OnMouseUpAsButton();
    }


    public virtual void TakeDamage(int dmg)
    {
        health -= dmg;

        if (health <= 0 && !dead)
        {
            dead = true;
            Die();
        }
    }

    protected void Die()
    {
        print(GetType() + " is dead.");
        Destroy(gameObject);
    }
}