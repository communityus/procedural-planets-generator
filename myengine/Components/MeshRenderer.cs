﻿using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyEngine.Components
{
	[Flags]
	public enum MyRenderingMode
	{
		DontRender = 0,
		RenderGeometry = (1 << 1),
		CastShadows = (1 << 2),
		RenderGeometryAndCastShadows = (1 << 1) | (1 << 2),
	}

	public class MeshRenderer : Renderer
	{
		/// <summary>
		/// Offset position from parent entity.
		/// </summary>
		public WorldPos Offset { get; set; }

		Mesh m_mesh;

		public Mesh Mesh
		{
			set
			{
				if (m_mesh != value)
				{
					m_mesh = value;
				}
			}
			get
			{
				return m_mesh;
			}
		}

		public event Action OnMashDataChanged;

		static readonly Vector3[] extentsTransformsToEdges = {
																 new Vector3( 1, 1, 1),
																 new Vector3( 1, 1,-1),
																 new Vector3( 1,-1, 1),
																 new Vector3( 1,-1,-1),
																 new Vector3(-1, 1, 1),
																 new Vector3(-1, 1,-1),
																 new Vector3(-1,-1, 1),
																 new Vector3(-1,-1,-1),
															 };

		public MeshRenderer(Entity entity) : base(entity)
		{
			Material = Dependency.Create<Material>();
			RenderingMode = MyRenderingMode.RenderGeometryAndCastShadows;
		}

		public override Bounds GetCameraSpaceBounds(WorldPos viewPointPos)
		{
			var relativePos = (viewPointPos - Offset).Towards(Entity.Transform.Position).ToVector3();
			if (Mesh == null)
			{
				return new Bounds(relativePos);
			}

			// without rotation and scale
			var boundsCenter = relativePos + Mesh.Bounds.Center;
			var bounds = new Bounds(boundsCenter);

			var boundsExtents = (Mesh.Bounds.Extents * Entity.Transform.Scale).RotateBy(Entity.Transform.Rotation);
			for (int i = 0; i < 8; i++)
			{
				bounds.Encapsulate(boundsCenter + boundsExtents.CompomentWiseMult(extentsTransformsToEdges[i]));
			}

			return bounds;
		}

		public override void UploadUBOandDraw(Camera camera, UniformBlock ubo)
		{
			var modelMat = this.Entity.Transform.GetLocalToWorldMatrix(camera.Transform.Position - Offset);
			var modelViewMat = modelMat * camera.GetRotationMatrix();
			ubo.model.modelMatrix = modelMat;
			ubo.model.modelViewMatrix = modelViewMat;
			ubo.model.modelViewProjectionMatrix = modelViewMat * camera.GetProjectionMat();
			ubo.modelUBO.UploadData();
			Mesh.Draw(Material.GBufferShader.HasTesselation);
		}

		public override bool ShouldRenderInContext(object renderContext)
		{
			return Mesh != null && Material != null && Material.DepthGrabShader != null && base.ShouldRenderInContext(renderContext);
		}
	}
}