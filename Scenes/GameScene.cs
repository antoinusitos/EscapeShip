using System;
using EscapeShip.UI;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using Gum.Forms.Controls;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Managers;
using EscapeShip.Entities;
using EscapeShip.Misc;
using MonoGameLibrary.Particles;

namespace EscapeShip.Scenes;

public class GameScene : Scene
{
    // Reference to the texture atlas that we can pass to UI elements when they
    // are created.
    private TextureAtlas _atlas;

    private ParticleEmitter _particleEmitter;

    private Slime _slime;

    float _cameraSpeed = 5;

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        float gameScale = 4.0f;

        // During the game scene, we want to disable exit on escape. Instead,
        // the escape key will be used to return back to the title screen
        Core.ExitOnEscape = false;

        Rectangle screenBounds = Core.GraphicsDevice.PresentationParameters.Bounds;

        RoomBounds = new Rectangle(
             (int)_tilemap.TileWidth,
             (int)_tilemap.TileHeight,
             screenBounds.Width - (int)_tilemap.TileWidth * 2,
             screenBounds.Height - (int)_tilemap.TileHeight * 2
         );

        // Initial slime position will be the center tile of the tile map.
        int centerRow = _tilemap.Rows / 2;
        int centerColumn = _tilemap.Columns / 2;

        _slime = new Slime("Slime");
        _slime.LoadContent(Content);
        _slime.Initialize();
        _slime.SetPosition(new Vector2(16 * gameScale));
        _slime.SetScale(4);
        _slime.Register();

        WallTest wallTest = new WallTest("wallTest");
        wallTest.LoadContent(Content);
        wallTest.Initialize();
        wallTest.Collider.Width = 16 * 50;
        wallTest.SetPosition(new Vector2(0, 0));
        wallTest.SetScale(4);
        wallTest.Register();

        WallTest wallTestLeft = new WallTest("wallTestLeft");
        wallTestLeft.LoadContent(Content);
        wallTestLeft.Initialize();
        wallTestLeft.Collider.Height = 16 * 20;
        wallTestLeft.SetPosition(new Vector2(0, 0));
        wallTestLeft.SetScale(4);
        wallTestLeft.Register();

        WallTest wallTestBottom = new WallTest("wallTestBottom");
        wallTestBottom.LoadContent(Content);
        wallTestBottom.Initialize();
        wallTestBottom.Collider.Width = 16 * 50;
        wallTestBottom.SetPosition(new Vector2(0, 16 * 19 * gameScale));
        wallTestBottom.SetScale(4);
        wallTestBottom.Register();

        WallTest wallTestRight = new WallTest("wallTestRight");
        wallTestRight.LoadContent(Content);
        wallTestRight.Initialize();
        wallTestRight.Collider.Height = 16 * 20;
        wallTestRight.SetPosition(new Vector2(16 * 50 * gameScale, 0));
        wallTestRight.SetScale(4);
        wallTestRight.Register();

        Container container = new Container("container");
        container.LoadContent(Content);
        container.Initialize();
        container.SetPosition(new Vector2(16 * 5 * gameScale, 16 * 5 * gameScale));
        container.SetScale(4);
        container.Register();

        _particleEmitter = new ParticleEmitter();
        _particleEmitter.SetPosition(new Vector2(0, 0));
        _particleEmitter.SetScale(10);
        _particleEmitter.SetSpawnRate(0.1f);
        _particleEmitter.SetVelocity(-Vector2.UnitY);
        _particleEmitter.SetLifeTime(1.5f);
        _particleEmitter.SetOffsetMin(-Vector2.UnitX * 20);
        _particleEmitter.SetOffsetMax(Vector2.UnitX * 20);
        _particleEmitter.Register();

        EscapeShipGameManager.Instance.time = 600;

        CameraManager.Instance.Camera.Move(new Vector2(Core.GraphicsDevice.Viewport.Width / 2, Core.GraphicsDevice.Viewport.Height / 2));

        InitializeUI();
    }

    public override void LoadContent()
    {
        // Create the texture atlas from the XML configuration file
        _atlas = RessourceManager.Instance.GetOrAddTextureAtlas("images/atlas-definition.xml");

        // Create the tilemap from the XML configuration file.
        _tilemap = RessourceManager.Instance.GetOrAddTilemap("images/tilemap-definition2.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);
    }

    public override void Update(float deltaTime)
    {
        // Ensure the UI is always updated
        GumService.Default.Update(TimeManager.Instance.gameTime);

        // If the game is paused, do not continue
        if (EscapeShipGameManager.Instance.paused)
        {
            return;
        }

        EscapeShipGameManager.Instance.time -= deltaTime;

        // Check for keyboard input and handle it.
        CheckKeyboardInput();

        // Check for gamepad input and handle it.
        CheckGamePadInput();

        Vector2 pos = Vector2.Zero;
        pos.X = MathHelper.Lerp(CameraManager.Instance.Camera.Position.X, _slime.Position.X + _slime.Velocity.X * 20, deltaTime * _cameraSpeed);
        pos.Y = MathHelper.Lerp(CameraManager.Instance.Camera.Position.Y, _slime.Position.Y + _slime.Velocity.Y * 20, deltaTime * _cameraSpeed);
        CameraManager.Instance.Camera.Position = pos;
    }

    private void CheckKeyboardInput()
    {
        // Get a reference to the keyboard inof
        KeyboardInfo keyboard = InputManager.Instance.Keyboard;

        // If the escape key is pressed, pause the game.
        if (keyboard.WasKeyJustPressed(Keys.Escape))
        {
            EscapeShipGameManager.Instance.PauseGame();
            return;
        }

        // If the escape key is pressed, return to the title screen.
        if (keyboard.WasKeyJustPressed(Keys.Escape))
        {
            SceneManager.Instance.ChangeScene(new TitleScene());
        }

        // If the M key is pressed, toggle mute state for audio.
        if (keyboard.WasKeyJustPressed(Keys.M))
        {
            Core.Audio.ToggleMute();
        }

        // If the + button is pressed, increase the volume.
        if (keyboard.WasKeyJustPressed(Keys.OemPlus))
        {
            Core.Audio.SongVolume += 0.1f;
            Core.Audio.SoundEffectVolume += 0.1f;
        }

        // If the - button was pressed, decrease the volume.
        if (keyboard.WasKeyJustPressed(Keys.OemMinus))
        {
            Core.Audio.SongVolume -= 0.1f;
            Core.Audio.SoundEffectVolume -= 0.1f;
        }
    }

    private void CheckGamePadInput()
    {
        // Get the gamepad info for gamepad one.
        GamePadInfo gamePadOne = InputManager.Instance.GamePads[(int)PlayerIndex.One];

        // If the start button is pressed, pause the game
        if (gamePadOne.WasButtonJustPressed(Buttons.Start))
        {
            EscapeShipGameManager.Instance.PauseGame();
            return;
        }
    }

    public override void Draw(float deltaTime)
    {
        // Draw the tilemap
        _tilemap.Draw(Core.SpriteBatch);
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        UIManager.Instance.currentUIEntity = new GameSceneUI();

        UIManager.Instance.currentUIEntity.LoadContent(Content);

        ((GameSceneUI)UIManager.Instance.currentUIEntity).CreatePausePanel();
    }

}
