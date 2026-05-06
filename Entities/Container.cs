using Microsoft.Xna.Framework.Content;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Managers;
using MonoGameLibrary.Misc;
using MonoGameLibrary.Shapes;

namespace EscapeShip.Entities;

public class Container : Entity
{
    public Container(string name) : base(name)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        _canCollide = true;
        _canRender = true;
        _collisionType = CollisionType.STATIC;

        _collider = new Box(
            _position.X,
            _position.Y,
            _sprite.Width,
            _sprite.Height
        );

        Trigger trigger = new Trigger(_entityName + " Trigger");
        trigger.LoadContent(Core.Content);
        trigger.Initialize();
        trigger.SetPosition(_position);
        trigger.SetScale(4);
        trigger.Register();
        trigger.onTriggerEvent += OnCollidePlayer;

        _children = trigger;
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);

        // Create the texture atlas from the XML configuration file
        TextureAtlas _atlas2 = RessourceManager.Instance.GetOrAddTextureAtlas("images/atlas-definition2.xml");

        _sprite = RessourceManager.Instance.GetOrAddSprite("container", _atlas2);
    }

    public void OnCollidePlayer(Entity other)
    {
        Debug.Log("collide player");
    }
}
