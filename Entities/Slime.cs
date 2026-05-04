using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Managers;
using MonoGameLibrary.Misc;
using MonoGameLibrary.Shapes;
using System;

namespace EscapeShip.Entities;

public class Slime : Entity
{
    // Speed multiplier when moving.
    private const float MOVEMENT_SPEED = 5.0f;

    // The sound effect to play when the slime eats a bat.
    private SoundEffect _collectSoundEffect;

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
            (int)(_sprite.Width),
            (int)(_sprite.Height)
        );

        AssignRandomBatVelocity();
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);

        TextureAtlas _atlas2 = RessourceManager.Instance.GetOrAddTextureAtlas("images/atlas-definition2.xml");

        _sprite = RessourceManager.Instance.GetOrAddSprite("tile", _atlas2);

        // Load the collect sound effect.
        _collectSoundEffect = RessourceManager.Instance.GetOrAddSoundEffect("audio/collect");
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        //Rectangle _roomBounds = SceneManager.Instance.ActiveScene.RoomBounds;

        Velocity = Vector2.Zero;

        CheckKeyboardInput();
        CheckGamePadInput();

        /*if (_collider.Left < _roomBounds.Left)
        {
            SetPosition(new Vector2(_roomBounds.Left, _position.Y));
        }
        else if (_collider.Right > _roomBounds.Right)
        {
            SetPosition(new Vector2(_roomBounds.Right - _sprite.Width, _position.Y));
        }

        if (_collider.Top < _roomBounds.Top)
        {
            SetPosition(new Vector2(_position.X, _roomBounds.Top));
        }
        else if (_collider.Bottom > _roomBounds.Bottom)
        {
            SetPosition(new Vector2(_position.X, _roomBounds.Bottom - _sprite.Height));
        }*/
    }

    private void CheckKeyboardInput()
    {
        // Get a reference to the keyboard inof
        KeyboardInfo keyboard = InputManager.Instance.Keyboard;

        // If the space key is held down, the movement speed increases by 1.5
        float speed = MOVEMENT_SPEED;
        if (keyboard.IsKeyDown(Keys.Space))
        {
            speed *= 1.5f;
        }

        Vector2 delta = Vector2.Zero;
        // If the W or Up keys are down, move the slime up on the screen.
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
        {
            Velocity.Y -= speed;
        }

        // if the S or Down keys are down, move the slime down on the screen.
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
        {
            Velocity.Y += speed;
        }

        // If the A or Left keys are down, move the slime left on the screen.
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
        {
            Velocity.X -= speed;
        }

        // If the D or Right keys are down, move the slime right on the screen.
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
        {
            Velocity.X += speed;
        }

        //SetPosition(_position + delta);
    }

    private void CheckGamePadInput()
    {
        // Get the gamepad info for gamepad one.
        GamePadInfo gamePadOne = InputManager.Instance.GamePads[(int)PlayerIndex.One];

        // If the A button is held down, the movement speed increases by 1.5
        // and the gamepad vibrates as feedback to the player.
        float speed = MOVEMENT_SPEED;
        if (gamePadOne.IsButtonDown(Buttons.A))
        {
            speed *= 1.5f;
            GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
        }
        else
        {
            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
        }

        Vector2 delta = Vector2.Zero;
        // Check thumbstick first since it has priority over which gamepad input
        // is movement.  It has priority since the thumbstick values provide a
        // more granular analog value that can be used for movement.
        if (gamePadOne.LeftThumbStick != Vector2.Zero)
        {
            delta.X += gamePadOne.LeftThumbStick.X * speed;
            delta.Y -= gamePadOne.LeftThumbStick.Y * speed;
            //SetPosition(_position +  delta);
        }
        else
        {
            // If DPadUp is down, move the slime up on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadUp))
            {
                delta.Y -= speed;
            }

            // If DPadDown is down, move the slime down on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadDown))
            {
                delta.Y += speed;
            }

            // If DPapLeft is down, move the slime left on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
            {
                delta.X -= speed;
            }

            // If DPadRight is down, move the slime right on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadRight))
            {
                delta.X += speed;
            }

            //SetPosition(_position + delta);
        }
    }

    private void AssignRandomBatVelocity()
    {
        // Generate a random angle.
        float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

        // Convert angle to a direction vector.
        float x = (float)Math.Cos(angle);
        float y = (float)Math.Sin(angle);
        Vector2 direction = new Vector2(x, y);

        // Multiply the direction vector by the movement speed
        _velocity = direction * MOVEMENT_SPEED;
    }

    public override void OnCollide(Entity other)
    {
    }
}
