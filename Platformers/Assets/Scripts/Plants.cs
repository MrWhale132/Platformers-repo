using UnityEngine;

public class Plants : MonoBehaviour, IDestroyable
{
    public int startHitpoints;

    [SerializeField]
    protected int hitpoints;
    protected bool destoryed;

    public Platform platform;


    protected virtual void Start()
    {
        hitpoints = startHitpoints;
        platform = MapGenerator.GetPlatformFromPosition(transform.position);
        platform.objAtPlatform = this;
        platform.walkable = false;
    }

    public virtual void TakeHit(int hit)
    {
        hitpoints -= hit;

        if (hitpoints <= 0 && !destoryed)
        {
            destoryed = true;
            Destroy();
        }
    }

    protected void Destroy()
    {
        print(GetType() + " destroyed.");
        Destroy(gameObject);
    }
}
