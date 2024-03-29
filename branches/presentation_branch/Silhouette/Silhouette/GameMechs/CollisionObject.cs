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
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;

namespace Silhouette.GameMechs
{
    [Serializable]
    public abstract class CollisionObject : LevelObject
    {
        [NonSerialized]
        public Fixture fixture;

        private bool _isPervious;
        [DisplayName("Pervious"), Category("Fixture Data")]
        [Description("Defines if the player can jump through the collision object, but stand on it.")]
        public bool isPervious { get { return _isPervious; } set { _isPervious = value; } }

        private float _density;
        [DisplayName("Mass"), Category("Physical Behavior")]
        [Description("The mass of the object to calculate physical interaction.")]
        public float density { get { return _density; } set { _density = value; } }

        private BodyType _bodyType;
        [DisplayName("BodyType"), Category("Physical Behavior")]
        [Description("The BodyType defines the behavior of an object. A static object never changes position or rotation, like the dynamic ones do.")]
        public BodyType bodyType { get { return _bodyType; } set { _bodyType = value; } }

        public abstract void ToFixture();

        public override void Update(GameTime gameTime) { }
    }

    #region Rectangle
        [Serializable]
        public class RectangleCollisionObject : CollisionObject
        {
            public Microsoft.Xna.Framework.Rectangle rectangle;

            private float _width;
            [DisplayName("Width"), Category("Fixture Data")]
            [Description("The width of the rectangle.")]
            public float width { get { return _width; } set { _width = value; transformed(); } }
            private float _height;
            [DisplayName("Height"), Category("Fixture Data")]
            [Description("The height of the rectangle.")]
            public float height { get { return _height; } set { _height = value; transformed(); } }

            private bool _isClimbable;
            [DisplayName("Climbable"), Category("Fixture Data")]
            [Description("Defines if the player can climb up the collision object.")]
            public bool isClimbable { get { return _isClimbable; } set { _isClimbable = value; } }

            public RectangleCollisionObject(Microsoft.Xna.Framework.Rectangle rectangle)
            {
                this.rectangle = rectangle;
                position = rectangle.Location.ToVector2();
                width = rectangle.Width;
                height = rectangle.Height;
            }

            public override void Initialise() { }
            public override void LoadContent() { ToFixture(); }

            public override string getPrefix()
            {
                return "RectangleCollisionObject_";
            }

            public override bool canScale() { return true; }
            public override Vector2 getScale() { return new Vector2(width, height); }
            public override void setScale(Vector2 scale)
            {
                width = (float)Math.Round(scale.X);
                height = (float)Math.Round(scale.Y);
                transformed();
            }

            public override bool canRotate() { return false; }
            public override float getRotation() { return 0; }
            public override void setRotation(float rotate) { }

            public override LevelObject clone()
            {
                RectangleCollisionObject result = (RectangleCollisionObject)this.MemberwiseClone();
                result.mouseOn = false;
                return result;
            }

            public override void transformed()
            {
                rectangle.Location = position.ToPoint();
                rectangle.Width = (int)width;
                rectangle.Height = (int)height;
            }

            public override bool contains(Vector2 worldPosition)
            {
                return rectangle.Contains(new Microsoft.Xna.Framework.Point((int)worldPosition.X, (int)worldPosition.Y));
            }

            public override void drawSelectionFrame(SpriteBatch spriteBatch, Matrix matrix)
            {
                Primitives.Instance.drawBox(spriteBatch, this.rectangle, Color.Yellow, 2);

                Vector2[] poly = rectangle.ToPolygon();

                foreach (Vector2 p in poly)
                {
                    Primitives.Instance.drawCircleFilled(spriteBatch, p, 4, Color.Yellow);
                }
            }

            public override void ToFixture()
            {
                fixture = FixtureManager.CreateRectangle(width, height, position, bodyType, density);
                fixture.isClimbable = isClimbable;
                fixture.isHalfTransparent = isPervious;
            }
        }
    #endregion

    #region Circle
        [Serializable]
        public class CircleCollisionObject : CollisionObject
        {
            private float _radius;
            [DisplayName("Radius"), Category("Fixture Data")]
            [Description("The radius of the circle fixture.")]
            public float radius { get { return _radius; } set { _radius = value; } }

            private bool _isClimbable;
            [DisplayName("Climbable"), Category("Fixture Data")]
            [Description("Defines if the player can climb up the collision object.")]
            public bool isClimbable { get { return _isClimbable; } set { _isClimbable = value; } }

            public CircleCollisionObject(Vector2 position, float radius)
            {
                this.position = position;
                this.radius = radius;
            }

            public override void Initialise() { }
            public override void LoadContent() { ToFixture(); }

            public override string getPrefix()
            {
                return "CircleCollisionObject_";
            }

            public override bool canScale() { return true; }
            public override Vector2 getScale() { return new Vector2(radius, radius); }
            public override void setScale(Vector2 scale) { radius = (float)Math.Round(scale.X); }

            public override bool canRotate() { return false; }
            public override float getRotation() { return 0; }
            public override void setRotation(float rotate) { }

            public override LevelObject clone()
            {
                CircleCollisionObject result = (CircleCollisionObject)this.MemberwiseClone();
                result.mouseOn = false;
                return result;
            }

            public override void transformed() { }

            public override bool contains(Vector2 worldPosition)
            {
                return (worldPosition - position).Length() <= radius;
            }

            public override void drawSelectionFrame(SpriteBatch spriteBatch, Matrix matrix)
            {
                Vector2 transformedRadius = Vector2.UnitX * radius;
                Primitives.Instance.drawCircle(spriteBatch, position, transformedRadius.Length(), Color.Yellow, 2);

                Vector2[] extents = new Vector2[4];
                extents[0] = position + Vector2.UnitX * transformedRadius.Length();
                extents[1] = position + Vector2.UnitY * transformedRadius.Length();
                extents[2] = position - Vector2.UnitX * transformedRadius.Length();
                extents[3] = position - Vector2.UnitY * transformedRadius.Length();

                foreach (Vector2 p in extents)
                {
                    Primitives.Instance.drawCircleFilled(spriteBatch, p, 4, Color.Yellow);
                }
            }

            public override void ToFixture()
            {
                fixture = FixtureManager.CreateCircle(radius, position, bodyType, density);
                fixture.isClimbable = isClimbable;
                fixture.isHalfTransparent = isPervious;
            }
        }
    #endregion

    #region Path
        [Serializable]
        public class PathCollisionObject : CollisionObject
        {
            public int lineWidth;

            public Vector2[] LocalPoints;
            public Vector2[] WorldPoints;

            public override void Initialise() { }
            public override void LoadContent() { ToFixture(); }

            public PathCollisionObject(Vector2[] Points)
            {
                WorldPoints = Points;
                LocalPoints = (Vector2[])Points.Clone();
                position = Points[0];
                for (int i = 0; i < LocalPoints.Length; i++) LocalPoints[i] -= position;
                lineWidth = 4;
                transformed();
            }

            public override bool contains(Vector2 worldPosition)
            {
                for (int i = 1; i < WorldPoints.Length; i++)
                {
                    if (worldPosition.DistanceToLineSegment(WorldPoints[i], WorldPoints[i - 1]) <= lineWidth) return true;
                }

                if (worldPosition.DistanceToLineSegment(WorldPoints[0], WorldPoints[WorldPoints.Length - 1]) <= lineWidth) return true;

                return false;
            }

            public override string getPrefix()
            {
                return "PathCollisionObject_";
            }

            public override bool canScale() { return true; }
            public override Vector2 getScale()
            {
                float length = (LocalPoints[1] - LocalPoints[0]).Length();
                return new Vector2(length, length);
            }
            public override void setScale(Vector2 scale)
            {
                float factor = scale.X / (LocalPoints[1] - LocalPoints[0]).Length();
                for (int i = 1; i < LocalPoints.Length; i++)
                {
                    Vector2 olddistance = LocalPoints[i] - LocalPoints[0];
                    LocalPoints[i] = LocalPoints[0] + olddistance * factor;
                }
                transformed();
            }

            public override bool canRotate() { return false; }
            public override float getRotation() { return 0; }
            public override void setRotation(float rotate) { }

            public override LevelObject clone()
            {
                PathCollisionObject result = (PathCollisionObject)this.MemberwiseClone();
                result.LocalPoints = (Vector2[])this.LocalPoints.Clone();
                result.WorldPoints = (Vector2[])this.WorldPoints.Clone();
                result.mouseOn = false;
                return result;
            }

            public override void transformed()
            {
                for (int i = 0; i < WorldPoints.Length; i++) WorldPoints[i] = LocalPoints[i] + position;
            }

            public override void drawSelectionFrame(SpriteBatch spriteBatch, Matrix matrix)
            {
                Primitives.Instance.drawPolygon(spriteBatch, WorldPoints, Color.Yellow, 2);

                foreach (Vector2 p in WorldPoints)
                {
                    Primitives.Instance.drawCircleFilled(spriteBatch, p, 4, Color.Yellow);
                }
            }

            public override void ToFixture()
            {
                Vertices vertices = new Vertices();
                foreach (Vector2 v in WorldPoints)
                { 
                    vertices.Add(FixtureManager.ToMeter(v));
                }

                fixture = FixtureFactory.CreateLoopShape(Level.Physics, vertices, this.density);
                fixture.Body.BodyType = this.bodyType;
                fixture.isHalfTransparent = this.isPervious;
            }
        }
    #endregion
}
