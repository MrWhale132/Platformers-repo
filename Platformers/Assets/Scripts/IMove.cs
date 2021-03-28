
public interface IMove
{
    void Move(Platform platform);
    void TryMove(Platform targetPlatform, out int resultCode);
    void MoveOn(Platform targetPlatform, int resultCode);
}
