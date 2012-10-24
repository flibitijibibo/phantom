﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Core;
using Phantom.Physics;

namespace Phantom.Shapes
{
    public class CompoundShape : Shape
    {
        private struct Container
        {
            public readonly Vector2 Offset;
            public readonly Shape Shape;
            public Container(Vector2 offset, Shape shape)
            {
                this.Offset = offset;
                this.Shape = shape;
            }
        }
        public override float RoughRadius
        {
            get { return this.roughRadius; }
        }

        private float roughRadius;

        private List<Container> shapes;
        private List<CollisionData> results;
        private Core.Entity stub;

        public CompoundShape()
        {
            this.shapes = new List<Container>();
            this.results = new List<CollisionData>();
            this.stub = new Entity(Vector2.Zero);
        }

#if DEBUG
        protected override void OnComponentAdded(Core.Component component)
        {
            if (component is Shape)
            {
                throw new Exception("don't add sub-shapes as components to a CompoundShape.");
            }
            base.OnComponentAdded(component);
        }
#endif

        public void AddShape(Vector2 offset, Shape shape)
        {
            this.shapes.Add(new Container(offset, shape));
            this.UpdateRoughRadius();
        }

        private void UpdateRoughRadius()
        {
            roughRadius = 0;
            for (int i = 0; i < this.shapes.Count; i++)
                roughRadius = Math.Max(roughRadius, (this.shapes[i].Offset.Length() + this.shapes[i].Shape.RoughRadius));
        }

        public override CollisionData Collide(Shape other)
        {
            results.Clear();
            Matrix rot = Matrix.CreateRotationZ(this.Entity.Orientation);
            for (int i = 0; i < this.shapes.Count; i++)
            {
                stub.Position = this.Entity.Position + Vector2.Transform(this.shapes[i].Offset, rot);
                stub.Orientation = this.Entity.Orientation;
                this.shapes[i].Shape.SetStubEntity(stub);
                CollisionData result = this.shapes[i].Shape.Collide(other);
                if (result.IsValid)
                    results.Add(result);
            }
            if (results.Count == 0)
                return CollisionData.Empty;
            CollisionData largest = new CollisionData(float.MinValue);
            for (int i = 0; i < results.Count; i++)
                if (results[i].Interpenetration > largest.Interpenetration)
                    largest = results[i];
            return largest;
        }

        public override OUT Accept<OUT, IN>(ShapeVisitor<OUT, IN> visitor, IN data)
        {
            results.Clear();
            Entity stub = new Entity(Vector2.Zero);
            Matrix rot = Matrix.CreateRotationZ(this.Entity.Orientation);
            for (int i = 0; i < this.shapes.Count; i++)
            {
                stub.Position = this.Entity.Position + Vector2.Transform(this.shapes[i].Offset, rot);
                stub.Orientation = this.Entity.Orientation;
                this.shapes[i].Shape.SetStubEntity(stub);
                OUT o = this.shapes[i].Shape.Accept(visitor, data);
                if (o is CollisionData && ((CollisionData)(object)o).IsValid)
                    results.Add(((CollisionData)(object)o));
            }
            if (results.Count == 0)
                return default(OUT);
            CollisionData largest = new CollisionData(float.MinValue);
            for (int i = 0; i < results.Count; i++)
                if (results[i].Interpenetration > largest.Interpenetration)
                    largest = results[i];
            return (OUT)(object)largest;
        }
    }
}