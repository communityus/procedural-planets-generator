﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MyEngine.Components
{

    [ComponentSetting(allowMultiple = false)]
    public partial class Transform : Component
    {
        WorldPos m_position = WorldPos.Zero;
        Vector3 m_scale = Vector3.One;
        Quaternion m_rotation = Quaternion.Identity;

        public Transform(Entity entity) : base(entity)
        {
        }

        public WorldPos Position
        {
            set
            {
                m_position = value;
                // TODO
                // Entity.RaiseOnChanged(ChangedFlags.Position);
            }
            get
            {
                return m_position;
            }
        }
        public Vector3 Scale
        {
            set
            {
                m_scale = value;
                // TODO
                // Entity.RaiseOnChanged(ChangedFlags.Scale);
            }
            get
            {
                return m_scale;
            }
        }
        public Quaternion Rotation
        {
            set
            {
                m_rotation = value;
                // TODO
                // Entity.RaiseOnChanged(ChangedFlags.Rotation);
            }
            get
            {
                return m_rotation;
            }
        }




        public Vector3 Right
        {
            get
            {
                return Constants.Vector3Right.RotateBy(Rotation);
            }
        }
        public Vector3 Up
        {
            get
            {
                return Constants.Vector3Up.RotateBy(Rotation);
            }
        }

        public Vector3 Forward
        {
            get
            {
                return Constants.Vector3Forward.RotateBy(Rotation);
            }
            set
            {
                this.Rotation = value.LookRot();
            }
        }
        
        public Matrix4 GetLocalToWorldMatrix(WorldPos viewPointPos)
        {
            return
                Matrix4.CreateScale(Scale) *
                Matrix4.CreateFromQuaternion(Rotation) *
                Matrix4.CreateTranslation(viewPointPos.Towards(Position).ToVector3());
        }

        public Matrix4 GetWorldToLocalMatrix(WorldPos viewPointPos)
        {
            return Matrix4.Invert(GetLocalToWorldMatrix(viewPointPos));
        }

        public void Translate(Vector3 translation, Space relativeTo = Space.Self)
        {
            if (relativeTo == Space.Self)
            {
                var m = Matrix4.CreateTranslation(translation) * Matrix4.CreateFromQuaternion(Rotation);
                this.Position += m.ExtractTranslation();
            }
            else if (relativeTo == Space.World)
            {
                this.Position += translation;
            }
        }

        /*
        /// <summary>
        /// Transforms position from local space to world space.
        /// </summary>
        /// <param name="local"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 local)
        {
            Vector3 world;
            var mat = LocalToWorldMatrix;
            Vector3.TransformPosition(ref local, ref mat, out world);
            return world;
        }

        
        /// <summary>
        /// Transforms position from world space to local space. The opposite of Transform.TransformPoint.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public Vector3 InverseTransformPoint(Vector3 world)
        {
            Vector3 local;
            var mat = WorldToLocalMatrix;
            Vector3.TransformPosition(ref world, ref mat, out local);
            return local;
        }


        public Vector3 TransformDirection(Vector3 local)
        {
            Vector3 world;
            var mat = LocalToWorldMatrix;
            Vector3.TransformVector(ref local, ref mat, out world);
            return world;
        }

        public Vector3 InverseTransformDirection(Vector3 world)
        {
            Vector3 local;
            var mat = WorldToLocalMatrix;
            Vector3.TransformVector(ref world, ref mat, out local);
            return local;
        }
        */
        public void LookAt(WorldPos worldPosition, Vector3 worldUp)
        {            
            this.Rotation = Matrix4.LookAt(Vector3.Zero, this.Position.Towards(worldPosition).ToVector3(), worldUp).ExtractRotation();
        }
        public void LookAt(WorldPos worldPosition)
        {
            var dir = this.Position.Towards(worldPosition);
            this.Rotation = dir.ToVector3().LookRot();
        }

        //public Matrix4 GetScalePosRotMatrix()
        //{
        //    return
        //        Matrix4.CreateScale(scale) *
        //        Matrix4.CreateTranslation(position) *
        //        Matrix4.CreateFromQuaternion(rotation);
        //}
        //public Matrix4 GetPosRotMatrix()
        //{
        //    return
        //        Matrix4.CreateTranslation(position) *
        //        Matrix4.CreateFromQuaternion(rotation);
        //}

    }
}
