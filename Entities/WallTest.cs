using Microsoft.Xna.Framework;
using MonoGameLibrary.Misc;
using MonoGameLibrary.Shapes;

namespace EscapeShip.Entities;

public class WallTest : Entity
{
    public override void Initialize()
    {
        base.Initialize();

        _canCollide = true;
        _canRender = true;
        _collisionType = CollisionType.STATIC;

        _collider = new Box(
            (int)(_position.X),
            (int)(_position.Y),
            16,
            16
        );
    }
}
