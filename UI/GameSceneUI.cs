using EscapeShip.Misc;
using EscapeShip.Scenes;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Managers;
using MonoGameLibrary.UI;
using System;

namespace EscapeShip.UI;

public class GameSceneUI : UIEntity
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

    // Reference to the texture atlas that we can pass to UI elements when they
    // are created.
    private TextureAtlas _atlas;

    // The UI sound effect to play when a UI event is triggered.
    private SoundEffect _uiSoundEffect;

    public GameSceneUI()
    {
        Rectangle RoomBounds = SceneManager.Instance.ActiveScene.RoomBounds;

        // Set the position of the score text to align to the left edge of the
        // room bounds, and to vertically be at the center of the first tile.
        _scoreTextPosition = new Vector2(RoomBounds.Left, SceneManager.Instance.ActiveScene.Tilemap .TileHeight * 0.5f);

        _timeTextPosition = new Vector2(RoomBounds.Center.X, SceneManager.Instance.ActiveScene.Tilemap.TileHeight * 0.5f);
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);

        // Create the texture atlas from the XML configuration file
        _atlas = RessourceManager.Instance.GetOrAddTextureAtlas("images/atlas-definition.xml");

        // Load the font.
        _font = RessourceManager.Instance.GetOrAddSpriteFont("fonts/04B_30");

        // Load the sound effect to play when ui actions occur.
        _uiSoundEffect = RessourceManager.Instance.GetOrAddSoundEffect("audio/ui");

        // Set the origin of the text so it is left-centered.
        float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
        _scoreTextOrigin = new Vector2(0, scoreTextYOrigin);

        // Set the origin of the text so it is left-centered.
        float timeTextOrigin = _font.MeasureString("Time").Y * 0.5f;
        _timeTextOrigin = new Vector2(0, timeTextOrigin);
    }

    public override void Render(SpriteBatch spriteBatch)
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

    public void PauseGame()
    {
        // Make the pause panel UI element visible.
        _pausePanel.IsVisible = true;

        // Set the resume button to have focus
        _resumeButton.IsFocused = true;
    }

    public void CreatePausePanel()
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
}
