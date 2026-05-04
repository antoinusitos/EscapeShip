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

namespace EscapeShip.Scenes;

public class GameScene : Scene
{
    // The SpriteFont Description used to draw text
    private SpriteFont _font;

    // Defines the position to draw the score text at.
    private Vector2 _scoreTextPosition;

    // Defines the origin used when drawing the score text.
    private Vector2 _scoreTextOrigin;

    // Defines the position to draw the score text at.
    private Vector2 _timeTextPosition;

    // Defines the origin used when drawing the score text.
    private Vector2 _timeTextOrigin;

    // Cached display strings rebuilt only when values change.
    private string _scoreText = "Score: 0";
    private string _timeText = "Time: 10:0";
    private int _cachedScore = -1;
    private int _cachedTimeSeconds = -1;

    // A reference to the pause panel UI element so we can set its visibility
    // when the game is paused.
    private Panel _pausePanel;

    // A reference to the resume button UI element so we can focus it
    // when the game is paused.
    private AnimatedButton _resumeButton;

    // The UI sound effect to play when a UI event is triggered.
    private SoundEffect _uiSoundEffect;

    // Reference to the texture atlas that we can pass to UI elements when they
    // are created.
    private TextureAtlas _atlas;

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

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

        Slime slime = new Slime();
        slime.LoadContent(Content);
        slime.Initialize();
        slime.SetPosition(new Vector2(centerColumn * _tilemap.TileWidth, centerRow * _tilemap.TileHeight));
        slime.SetScale(4);
        slime.Register();

        Bat bat = new Bat();
        bat.LoadContent(Content);
        bat.Initialize();
        bat.SetPosition(new Vector2(RoomBounds.Left, RoomBounds.Top));
        bat.SetScale(4);
        bat.Register();

        for (int i = 0; i < 16; i++)
        {
            WallTest wallTest = new WallTest();
            wallTest.LoadContent(Content);
            wallTest.Initialize();
            wallTest.SetPosition(new Vector2(16 * i * 4, 0));
            wallTest.SetScale(4);
            wallTest.Register();
        }
        for (int i = 1; i < 9; i++)
        {
            WallTest wallTest = new WallTest();
            wallTest.LoadContent(Content);
            wallTest.Initialize();
            wallTest.SetPosition(new Vector2(0, 16 * i * 4));
            wallTest.SetScale(4);
            wallTest.Register();
        }
        for (int i = 1; i < 16; i++)
        {
            WallTest wallTest = new WallTest();
            wallTest.LoadContent(Content);
            wallTest.Initialize();
            wallTest.SetPosition(new Vector2(16 * i * 4, 16 * 8 * 4));
            wallTest.SetScale(4);
            wallTest.Register();
        }
        for (int i = 1; i < 8; i++)
        {
            WallTest wallTest = new WallTest();
            wallTest.LoadContent(Content);
            wallTest.Initialize();
            wallTest.SetPosition(new Vector2(16 * 15 * 4, 16 * i * 4));
            wallTest.SetScale(4);
            wallTest.Register();
        }

        // Set the position of the score text to align to the left edge of the
        // room bounds, and to vertically be at the center of the first tile.
        _scoreTextPosition = new Vector2(RoomBounds.Left, _tilemap.TileHeight * 0.5f);

        _timeTextPosition = new Vector2(RoomBounds.Center.X, _tilemap.TileHeight * 0.5f);

        // Set the origin of the text so it is left-centered.
        float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
        _scoreTextOrigin = new Vector2(0, scoreTextYOrigin);

        // Set the origin of the text so it is left-centered.
        float timeTextOrigin = _font.MeasureString("Time").Y * 0.5f;
        _timeTextOrigin = new Vector2(0, timeTextOrigin);

        EscapeShipGameManager.Instance.time = 600;

        InitializeUI();
    }

    public override void LoadContent()
    {
        // Create the texture atlas from the XML configuration file
        _atlas = RessourceManager.Instance.GetOrAddTextureAtlas("images/atlas-definition.xml");

        // Create the tilemap from the XML configuration file.
        _tilemap = RessourceManager.Instance.GetOrAddTilemap("images/tilemap-definition2.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        // Load the font.
        _font = RessourceManager.Instance.GetOrAddSpriteFont("fonts/04B_30");

        // Load the sound effect to play when ui actions occur.
        _uiSoundEffect = RessourceManager.Instance.GetOrAddSoundEffect("audio/ui");
    }

    public override void Update(GameTime gameTime)
    {
        // Ensure the UI is always updated
        GumService.Default.Update(gameTime);

        // If the game is paused, do not continue
        if (_pausePanel.IsVisible)
        {
            return;
        }

        EscapeShipGameManager.Instance.time -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Check for keyboard input and handle it.
        CheckKeyboardInput();

        // Check for gamepad input and handle it.
        CheckGamePadInput();
    }

    private void CheckKeyboardInput()
    {
        // Get a reference to the keyboard inof
        KeyboardInfo keyboard = InputManager.Instance.Keyboard;

        // If the escape key is pressed, pause the game.
        if (keyboard.WasKeyJustPressed(Keys.Escape))
        {
            PauseGame();
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
            PauseGame();
            return;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Draw the tilemap
        _tilemap.Draw(Core.SpriteBatch);
    }

    public override void DrawUI(GameTime gameTime)
    {
        // Begin the sprite batch to prepare for rendering.
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the tilemap
        //_tilemap.Draw(Core.SpriteBatch);

        int score = EscapeShipGameManager.Instance.score;
        if (score != _cachedScore)
        {
            _cachedScore = score;
            _scoreText = "Score: " + score.ToString();
        }

        // Draw the score.
        Core.SpriteBatch.DrawString(
            _font,              // spriteFont
            _scoreText,         // text
            _scoreTextPosition, // position
            Color.White,        // color
            0.0f,               // rotation
            _scoreTextOrigin,   // origin
            1.0f,               // scale
            SpriteEffects.None, // effects
            0.0f                // layerDepth
        );

        float sec = EscapeShipGameManager.Instance.time % 60;
        float min = (EscapeShipGameManager.Instance.time - sec) / 60;
        int timeSeconds = (int)sec;
        if (timeSeconds != _cachedTimeSeconds)
        {
            _cachedTimeSeconds = timeSeconds;
            _timeText = "Time: " + (int)min + ":" + timeSeconds.ToString();
        }

        // Draw the time.
        Core.SpriteBatch.DrawString(
            _font,              // spriteFont
            _timeText,          // text
            _timeTextPosition,  // position
            Color.White,        // color
            0.0f,               // rotation
            _timeTextOrigin,    // origin
            1.0f,               // scale
            SpriteEffects.None, // effects
            0.0f                // layerDepth
        );

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();

        GumService.Default.Draw();
    }

    private void PauseGame()
    {
        // Make the pause panel UI element visible.
        _pausePanel.IsVisible = true;

        // Set the resume button to have focus
        _resumeButton.IsFocused = true;
    }

    private void CreatePausePanel()
    {
        _pausePanel = new Panel();
        _pausePanel.Anchor(Anchor.Center);
        _pausePanel.WidthUnits = DimensionUnitType.Absolute;
        _pausePanel.HeightUnits = DimensionUnitType.Absolute;
        _pausePanel.Height = 70;
        _pausePanel.Width = 264;
        _pausePanel.IsVisible = false;
        _pausePanel.AddToRoot();

        TextureRegion backgroundRegion = _atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureWidth = backgroundRegion.Width;
        _pausePanel.AddChild(background);

        TextRuntime textInstance = new TextRuntime();
        textInstance.Text = "PAUSED";
        textInstance.CustomFontFile = @"fonts/04b_30.fnt";
        textInstance.UseCustomFont = true;
        textInstance.FontScale = 0.5f;
        textInstance.X = 10f;
        textInstance.Y = 10f;
        _pausePanel.AddChild(textInstance);

        _resumeButton = new AnimatedButton(_atlas);
        _resumeButton.Text = "RESUME";
        _resumeButton.Anchor(Anchor.BottomLeft);
        _resumeButton.X = 9f;
        _resumeButton.Y = -9f;
        _resumeButton.Width = 80;
        _resumeButton.Click += HandleResumeButtonClicked;
        _pausePanel.AddChild(_resumeButton);

        AnimatedButton quitButton = new AnimatedButton(_atlas);
        quitButton.Text = "QUIT";
        quitButton.Anchor(Anchor.BottomRight);
        quitButton.X = -9f;
        quitButton.Y = -9f;
        quitButton.Width = 80;
        quitButton.Click += HandleQuitButtonClicked;

        _pausePanel.AddChild(quitButton);
    }

    private void HandleResumeButtonClicked(object sender, EventArgs e)
    {
        // A UI interaction occurred, play the sound effect
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        // Make the pause panel invisible to resume the game.
        _pausePanel.IsVisible = false;
    }

    private void HandleQuitButtonClicked(object sender, EventArgs e)
    {
        // A UI interaction occurred, play the sound effect
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        // Go back to the title scene.
        SceneManager.Instance.ChangeScene(new TitleScene());
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        CreatePausePanel();
    }

}
