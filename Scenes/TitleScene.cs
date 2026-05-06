using EscapeShip.UI;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Managers;
using MonoGameLibrary.Input;

namespace EscapeShip.Scenes;

public class TitleScene : Scene
{
    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        InitializeUI();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // If the user presses enter, switch to the game scene.
        if (InputManager.Instance.Keyboard.WasKeyJustPressed(Keys.Enter))
        {
            SceneManager.Instance.ChangeScene(new GameScene());
        }
    }

    private void InitializeUI()
    {
        // Clear out any previous UI in case we came here from
        // a different screen:
        GumService.Default.Root.Children.Clear();

        UIManager.Instance.currentUIEntity = new TitleSceneUI();

        UIManager.Instance.currentUIEntity.LoadContent(Content);

        ((TitleSceneUI)UIManager.Instance.currentUIEntity).CreateTitlePanel();
        ((TitleSceneUI)UIManager.Instance.currentUIEntity).CreateOptionsPanel();
    }

}
