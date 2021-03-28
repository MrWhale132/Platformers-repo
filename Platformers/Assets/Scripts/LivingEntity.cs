using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    public int startingHealth;

    [SerializeField]
    protected int health;
    protected bool dead;

    public LivingEntity livingEntity { get; protected set; }


    protected virtual void Awake()
    {
        livingEntity = GetComponent<LivingEntity>();
    }

    protected virtual void Start()
    {
        health = startingHealth;
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
