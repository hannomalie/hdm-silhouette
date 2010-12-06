﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Silhouette.GameMechs;
using System.IO;
using System.ComponentModel;

//Physik-Engine Klassen
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace Silhouette.GameMechs
{
    [Serializable]
    public class TextureObject : DrawableLevelObject
    {
        private string _assetName;
        [DisplayName("Filename"), Category("Texture Data")]
        [Description("The filename of the attached texture.")]
        public string assetName { get { return _assetName; } set { _assetName = value; } }

        private string _fullPath;
        [DisplayName("Path"), Category("Texture Data")]
        [Description("The full path of the texture.")]
        public string fullPath { get { return _fullPath; } set { _fullPath = value; } }

        private Vector2 _scale;
        [DisplayName("Scale"), Category("Texture Data")]
        [Description("The scale factor of the object.")]
        public Vector2 scale { get { return _scale; } set { _scale = value; transformed(); } }

        private float _rotation;
        [DisplayName("Rotation"), Category("Texture Data")]
        [Description("The rotation factor of the object.")]
        public float rotation { get { return _rotation; } set { _rotation = value; transformed(); } }

        private Vector2 _origin;
        [DisplayName("Origin"), Category("Texture Data")]
        [Description("The sprite origin. Default is (0,0), which is the upper left corner.")]
        public Vector2 origin { get { return _origin; } set { _origin = value; transformed(); } } 


        [NonSerialized]
        public Texture2D texture;

        Matrix transform;
        Rectangle boundingBox;
        Vector2[] polygon;

        public TextureObject(string path)
        {
            this.fullPath = path;
            this.assetName = Path.GetFileNameWithoutExtension(path);
            this.scale = Vector2.One;
            this.rotation = 0f;
            this.origin = Vector2.Zero;
            this.polygon = new Vector2[4];
        }

        public override void Initialise() {}

        public override void LoadContent()
        {
            try
            {
                texture = GameLoop.gameInstance.Content.Load<Texture2D>("Sprites/" + assetName);
            }
            catch (Exception e1)
            {
                try
                {
                    string p = Path.Combine(layer.level.contentPath, Path.GetFileName(fullPath));
                    texture = TextureManager.Instance.LoadFromFile(p);
                }
                catch (Exception e2)
                {
                    texture = TextureManager.Instance.LoadFromFile(fullPath);
                }
            }

            if(texture != null)
                origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / 2));
        }

        public override void Update(GameTime gameTime) {}

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White, rotation, origin, scale, SpriteEffects.None, 1);
        }

        //---> Editor-Funktionalität <---//

        public override void drawInEditor(SpriteBatch spriteBatch)
        {
            Color color = Color.White;
            if (mouseOn) color = Constants.onHover;
            origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / 2));
            spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, SpriteEffects.None, 1);
        }

        public override void loadContentInEditor(GraphicsDevice graphics)
        {
            if (texture == null)
            {
                try
                {
                    string p = Path.Combine(layer.level.contentPath, Path.GetFileName(fullPath));
                    FileStream file = FileManager.LoadConfigFile(p);
                    texture = Texture2D.FromStream(graphics, file);
                    this.fullPath = p;
                    file.Close();
                }
                catch (Exception e)
                {
                    FileStream file = FileManager.LoadConfigFile(fullPath);
                    texture = Texture2D.FromStream(graphics, file);
                    file.Close();
                }
            }

            transformed();
        }

        public override string getPrefix()
        {
            return "TextureObject_";
        }

        public override bool canScale() { return true; }
        public override Vector2 getScale() { return scale; }
        public override void setScale(Vector2 scale) { this.scale = scale; }
        public override bool canRotate() { return true; }
        public override float getRotation() { return rotation; }
        public override void setRotation(float rotate) { this.rotation = rotate; }

        public override LevelObject clone()
        {
            TextureObject result = (TextureObject)this.MemberwiseClone();
            result.polygon = (Vector2[])this.polygon.Clone();
            result.mouseOn = false;
            return result;
        }

        public override void transformed()
        {
            if (texture == null)
                return;

            transform =
                Matrix.CreateTranslation(new Vector3(origin.X, origin.Y, 0.0f)) *
                Matrix.CreateScale(scale.X, scale.Y, 1) *
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateTranslation(new Vector3(position, 0.0f));

            Vector2 leftTop = new Vector2(-texture.Width, -texture.Height);
            Vector2 rightTop = new Vector2(0, -texture.Height);
            Vector2 leftBottom = new Vector2(-texture.Width, 0);
            Vector2 rightBottom = new Vector2(0, 0);

            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            polygon[0] = leftTop;
            polygon[1] = rightTop;
            polygon[3] = leftBottom;
            polygon[2] = rightBottom;

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            boundingBox = new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        public override bool contains(Vector2 worldPosition)
        {
            return boundingBox.Contains((int)worldPosition.X, (int)worldPosition.Y);
        }

        public override void drawSelectionFrame(SpriteBatch spriteBatch, Matrix matrix)
        {
            Primitives.Instance.drawPolygon(spriteBatch, polygon, Color.Yellow, 2);
            foreach (Vector2 p in polygon)
            {
                Primitives.Instance.drawCircleFilled(spriteBatch, p, 4, Color.Yellow);
            }
        }
    }
}
