using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Silhouette;
using Silhouette.Engine;
using Silhouette.Engine.Manager;
using SilhouetteEditor.Forms;

//Physik-Engine Klassen
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
namespace SilhouetteEditor
{
    public class EditorLoop : Microsoft.Xna.Framework.Game
    {
        /* Sascha:
         * Zentrale Klasse zur Kommunikation zwischen Windows Forms und dem XNA Framework.
        */
        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        static EditorLoop _editorLoopInstance;
        public Form winform;
        public static EditorLoop EditorLoopInstance { get { return _editorLoopInstance; } }
        public World Physics { get; set; }
        private IntPtr drawSurface;

        public EditorLoop(IntPtr drawSurface)
        {
            _editorLoopInstance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            GameSettings.Initialise();
            GameSettings.ApplyChanges(ref graphics);
            this.drawSurface = drawSurface;
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            winform = (Form)Form.FromHandle(Window.Handle);
            winform.VisibleChanged += new EventHandler(EditorLoop_VisibleChanged);
            winform.Size = new System.Drawing.Size(10, 10);
            Mouse.WindowHandle = drawSurface;
            resizebackbuffer(MainForm.Default.GameView.Width, MainForm.Default.GameView.Height);
            winform.Hide();
        }

        protected override void Initialize()
        {
            Primitives.Instance.Initialize(this.GraphicsDevice);
            Editor.Default.Initialize();
            Physics = new World(new Vector2(0.0f, 9.8f));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            DebugViewXNA.LoadContent(EditorLoop.EditorLoopInstance.GraphicsDevice, EditorLoop.EditorLoopInstance.Content);
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            if (!MainForm.Default.GameView.ContainsFocus) return;

            Editor.Default.Update(gameTime);
            TimerManager.UpdateInEditor(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            int fps = (int)(1 / gameTime.ElapsedGameTime.TotalSeconds);
            MainForm.Default.FPS.Text = "FPS: " + fps.ToString();
            Editor.Default.Draw(MainForm.Default.treeView1.Width);
            base.Draw(gameTime);
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = drawSurface;
        }
        private void EditorLoop_VisibleChanged(object sender, EventArgs e)
        {
            winform.Hide();
            winform.Size = new System.Drawing.Size(10, 10);
            winform.Visible = false;
        }
        public void resizebackbuffer(int width, int height)
        {
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.ApplyChanges();
        }
    }
}
