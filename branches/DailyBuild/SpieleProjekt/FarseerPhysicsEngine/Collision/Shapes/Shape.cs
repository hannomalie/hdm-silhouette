﻿/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Collision.Shapes
{
    /// <summary>
    /// This holds the mass data computed for a shape.
    /// </summary>
    public struct MassData
    {
        /// <summary>
        /// The position of the shape's centroid relative to the shape's origin.
        /// </summary>
        public Vector2 Centroid;

        /// <summary>
        /// The rotational inertia of the shape about the local origin.
        /// </summary>
        public float Inertia;

        /// <summary>
        /// The mass of the shape, usually in kilograms.
        /// </summary>
        public float Mass;

        /// <summary>
        /// The area of the shape
        /// </summary>
        public float Area;
    }

    public enum ShapeType
    {
        Unknown = -1,
        Circle = 0,
        Edge = 1,
        Polygon = 2,
        Loop = 3,
        TypeCount = 4,
    }

    /// <summary>
    /// A shape is used for collision detection. You can create a shape however you like.
    /// Shapes used for simulation in World are created automatically when a Fixture
    /// is created. Shapes may encapsulate a one or more child shapes.
    /// </summary>
    public abstract class Shape
    {
        public MassData MassData;

        /// <summary>
        /// Radius of the Shape
        /// </summary>
        public float Radius;

        internal float _density;

        protected Shape(float density)
        {
            _density = density;
            ShapeType = ShapeType.Unknown;
        }

        /// <summary>
        /// Get the type of this shape. You can use this to down cast to the concrete shape.
        /// </summary>
        /// <value>The type of the shape.</value>
        public ShapeType ShapeType { get; internal set; }

        /// <summary>
        /// Get the number of child primitives.
        /// </summary>
        /// <value></value>
        public abstract int ChildCount { get; }

        public float Density
        {
            get { return _density; }
            set
            {
                _density = value;
                ComputeProperties();
            }
        }

        /// <summary>
        /// Clone the concrete shape
        /// </summary>
        /// <returns>A clone of the shape</returns>
        public abstract Shape Clone();

        /// <summary>
        /// Test a point for containment in this shape. This only works for convex shapes.
        /// </summary>
        /// <param name="transform">The shape world transform.</param>
        /// <param name="point">a point in world coordinates.</param>
        /// <returns>True if the point is inside the shape</returns>
        public abstract bool TestPoint(ref Transform transform, ref Vector2 point);

        /// <summary>
        /// Cast a ray against a child shape.
        /// </summary>
        /// <param name="output">The ray-cast results.</param>
        /// <param name="input">The ray-cast input parameters.</param>
        /// <param name="transform">The transform to be applied to the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        /// <returns>True if the ray-cast hits the shape</returns>
        public abstract bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform,
                                     int childIndex);

        /// <summary>
        /// Given a transform, compute the associated axis aligned bounding box for a child shape.
        /// </summary>
        /// <param name="aabb">The aabb results.</param>
        /// <param name="transform">The world transform of the shape.</param>
        /// <param name="childIndex">The child shape index.</param>
        public abstract void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex);

        /// <summary>
        /// Compute the mass properties of this shape using its dimensions and density.
        /// The inertia tensor is computed about the local origin, not the centroid.
        /// </summary>
        public abstract void ComputeProperties();
    }
}