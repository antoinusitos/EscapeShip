using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Managers;
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
            (int)(16),
            (int)(16)
        );
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);

        // Create the texture atlas from the XML configuration file
        TextureAtlas _atlas = RessourceManager.Instance.GetOrAddTextureAtlas("images/atlas-definition.xml");

        _animatedSprite = _atlas.CreateAnimatedSprite("bat-animation");
    }
}
