using UnityEngine;

public interface IAtack
{
    void Atack(Platform platform);
    void TryAtack(Platform platform, out int resultCode);
}
