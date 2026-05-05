using EscapeShip.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Managers;
using MonoGameLibrary.Misc;
using MonoGameLibrary.Shapes;
using System;

namespace EscapeShip.Entities;

public class Bat : Entity
{
    // The sound effect to play when the bat bounces off the edge of the screen.
    protected SoundEffect _bounceSoundEffect;

    // Speed multiplier when moving.
    private const float MOVEMENT_SPEED = 5.0f;

    public override void Initialize()
    {
        base.Initialize();

        _canUpdate = true;
        _canCollide = true;
        _canRender = true;
        _canMove = true;
        _collisionType = CollisionType.DYNAMIC;

        _collider = new Box(
            (int)(_position.X),
            (int)(_position.Y),
            (int)(_animatedSprite.Width),
            (int)(_animatedSprite.Height)
        );

        AssignRandomBatVelocity();
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);

        // Create the texture atlas from the XML configuration file
        TextureAtlas _atlas = RessourceManager.Instance.GetOrAddTextureAtlas("images/atlas-definition.xml");

        _animatedSprite = _atlas.CreateAnimatedSprite("bat-animation");

        _bounceSoundEffect = RessourceManager.Instance.GetOrAddSoundEffect("audio/bounce");
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Calculate the new position of the bat based on the velocity.
        Vector2 newBatPosition = _position + _velocity;

        Rectangle _roomBounds = SceneManager.Instance.ActiveScene.RoomBounds;

        Vector2 normal = Vector2.Zero;
        if (_collider.Left < _roomBounds.Left)
        {
            normal.X = Vector2.UnitX.X;
            newBatPosition.X = _roomBounds.Left;
        }
        else if (_collider.Right > _roomBounds.Right)
        {
            normal.X = -Vector2.UnitX.X;
            newBatPosition.X = _roomBounds.Right - _animatedSprite.Width;
        }

        if (_collider.Top < _roomBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y;
            newBatPosition.Y = _roomBounds.Top;
        }
        else if (_collider.Bottom > _roomBounds.Bottom)
        {
            normal.Y = -Vector2.UnitY.Y;
            newBatPosition.Y = _roomBounds.Bottom - _animatedSprite.Height;
        }

        // If the normal is anything but Vector2.Zero, this means the bat had
        // moved outside the screen edge so we should reflect it about the
        // normal.
        if (normal != Vector2.Zero)
        {
            normal.Normalize();
            Velocity = Vector2.Reflect(Velocity, normal);

            // Play the bounce sound effect.
            Core.Audio.PlaySoundEffect(_bounceSoundEffect);
        }

        //SetPosition(newBatPosition);
    }

    private void AssignRandomBatVelocity()
    {

    }

    public override void OnCollide(Entity other)
    {
        // Choose a random row and column based on the total number of each
        int column = Random.Shared.Next(1, SceneManager.Instance.ActiveScene.Tilemap.Columns - 1);
        int row = Random.Shared.Next(1, SceneManager.Instance.ActiveScene.Tilemap.Rows - 1);

        // Change the bat position by setting the x and y values equal to
        // the column and row multiplied by the width and height.
        SetPosition(new Vector2(column * _animatedSprite.Width, row * _animatedSprite.Height));

        // Assign a new random velocity to the bat.
        AssignRandomBatVelocity();

        // Increase the player's score.
        EscapeShipGameManager.Instance.score += 100;
    }
}
