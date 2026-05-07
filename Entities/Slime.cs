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
    private const float MOVEMENT_SPEED = 400.0f;

    // The sound effect to play when the slime eats a bat.
    private SoundEffect _collectSoundEffect;

    public Slime(string name) : base(name)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        _canUpdate = true;
        _canCollide = true;
        _canRender = true;
        _canMove = true;
        _collisionType = CollisionType.DYNAMIC;

        _collider = new Box(
            _position.X,
            _position.Y,
            _sprite.Width,
            _sprite.Height
        );

        Trigger trigger = new Trigger(_entityName + " Trigger");
        trigger.LoadContent(Core.Content);
        trigger.Initialize();
        trigger.AttachTo(this);
        trigger.SetRelativePosition(-trigger.Collider.Width + 16, -trigger.Collider.Height + 16);
        //trigger.SetRelativePosition(-500, -trigger.Collider.Height / 2);
        trigger.SetPosition(_position);
        trigger.SetScale(4);
        trigger.Register();
        trigger.onTriggerEnter += OnTriggerEnter;
        trigger.onTriggerExit += onTriggerExit;
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);

        TextureAtlas _atlas2 = RessourceManager.Instance.GetOrAddTextureAtlas("images/atlas-definition2.xml");

        _sprite = RessourceManager.Instance.GetOrAddSprite("tile", _atlas2);

        // Load the collect sound effect.
        _collectSoundEffect = RessourceManager.Instance.GetOrAddSoundEffect("audio/collect");
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        //Rectangle _roomBounds = SceneManager.Instance.ActiveScene.RoomBounds;

        Velocity = Vector2.Zero;

        CheckKeyboardInput(deltaTime);
        CheckGamePadInput(deltaTime);
    }

    private void CheckKeyboardInput(float deltaTime)
    {
        // Get a reference to the keyboard inof
        KeyboardInfo keyboard = InputManager.Instance.Keyboard;

        // If the space key is held down, the movement speed increases by 1.5
        float speed = MOVEMENT_SPEED;
        if (keyboard.IsKeyDown(Keys.Space))
        {
            speed *= 1.5f;
        }

        // If the W or Up keys are down, move the slime up on the screen.
        if (keyboard.IsKeyDown(Keys.W))
        {
            Velocity.Y -= 1;
        }

        // if the S or Down keys are down, move the slime down on the screen.
        if (keyboard.IsKeyDown(Keys.S))
        {
            Velocity.Y += 1;
        }

        // If the A or Left keys are down, move the slime left on the screen.
        if (keyboard.IsKeyDown(Keys.A))
        {
            Velocity.X -= 1;
        }

        // If the D or Right keys are down, move the slime right on the screen.
        if (keyboard.IsKeyDown(Keys.D))
        {
            Velocity.X += 1;
        }

        if (Velocity != Vector2.Zero)
        {
            Velocity.Normalize();
            Velocity *= speed *deltaTime;
        }

        if (keyboard.WasKeyJustPressed(Keys.F)) 
        {
            SetWantToInteract(true);
        }

        //Debug.Log("vel " + Velocity.Y);
    }

    private void CheckGamePadInput(float deltaTime)
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

        // Check thumbstick first since it has priority over which gamepad input
        // is movement.  It has priority since the thumbstick values provide a
        // more granular analog value that can be used for movement.
        if (gamePadOne.LeftThumbStick != Vector2.Zero)
        {
            if (gamePadOne.LeftThumbStick.X >= 0.1f || gamePadOne.LeftThumbStick.X <= -0.1f)
            {
                Velocity.X += gamePadOne.LeftThumbStick.X * speed * deltaTime;
            }
            if (gamePadOne.LeftThumbStick.Y >= 0.1f || gamePadOne.LeftThumbStick.Y <= -0.1f)
            {
                Velocity.Y -= gamePadOne.LeftThumbStick.Y * speed * deltaTime;
            }
        }
        else
        {
            // If DPadUp is down, move the slime up on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadUp))
            {
                Velocity.Y -= speed * deltaTime;
            }

            // If DPadDown is down, move the slime down on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadDown))
            {
                Velocity.Y += speed * deltaTime;
            }

            // If DPapLeft is down, move the slime left on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
            {
                Velocity.X -= speed * deltaTime;
            }

            // If DPadRight is down, move the slime right on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadRight))
            {
                Velocity.X += speed * deltaTime;
            }
        }
    }

    public void OnTriggerEnter(Entity other)
    {
        if (other is Container)
        {
            Debug.Log("collide Container");
        }
    }

    public void onTriggerExit(Entity other)
    {
        if (other is Container)
        {
            Debug.Log("collide Container");
        }
    }
}
