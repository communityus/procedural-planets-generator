﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using MyEngine.Components;
using System.Collections;
using System.Threading;

namespace MyEngine
{
	public struct CameraSpaceBounds
	{
		public float depth;
		public float minX;
		public float maxX;
		public float minY;
		public float maxY;
	}

	public class SoftwareDepthRasterizer
	{
		readonly ushort width;
		readonly ushort height;
		readonly ushort widthMinusOne;
		readonly ushort heightMinusOne;
		readonly float halfWidth;
		readonly float halfHeight;

		float[] depthBuffer;

		float[] clearedDepthBuffer;

		public SoftwareDepthRasterizer(ushort width, ushort height)
		{
			this.width = width;
			this.height = height;
			this.widthMinusOne = (ushort)(width - 1);
			this.heightMinusOne = (ushort)(height - 1);
			this.halfWidth = width / 2.0f;
			this.halfHeight = height / 2.0f;
			depthBuffer = new float[width * height];
			clearedDepthBuffer = new float[width * height];
			for (int i = 0; i < clearedDepthBuffer.Length; i++)
				clearedDepthBuffer[i] = 0;// float.MaxValue;
		}

		public void Clear()
		{
			UpdateDebugView();
			Array.Copy(clearedDepthBuffer, depthBuffer, depthBuffer.Length);
		}


		/// <summary>
		/// Enumerable of vectors where each 3 vectors represent one triangle, in camera space.
		/// </summary>
		/// <param name="tr"></param>
		public void AddTriangles(IEnumerable<Vector3> tr)
		{
			if (tr == null) return;
			var e = tr.GetEnumerator();
			while (true)
			{
				Vector3 a, b, c;
				if (!e.MoveNext()) break;
				a = e.Current;
				if (!e.MoveNext()) break;
				b = e.Current;
				if (!e.MoveNext()) break;
				c = e.Current;

				AddTriangle(ref a, ref b, ref c);
			}
		}


		// http://stackoverflow.com/a/33558594/782022
		static int iround(float num) => (int)(num + 0.5f);
		static int min(int a, int b, int c)
		{
			if (a <= b && a <= c) return a;
			if (b <= a && b <= c) return b;
			return c;
		}
		static int max(int a, int b, int c)
		{
			if (a >= b && a >= c) return a;
			if (b >= a && b >= c) return b;
			return c;
		}
		static float min(float a, float b, float c)
		{
			if (a <= b && a <= c) return a;
			if (b <= a && b <= c) return b;
			return c;
		}
		static float max(float a, float b, float c)
		{
			if (a >= b && a >= c) return a;
			if (b >= a && b >= c) return b;
			return c;
		}

		public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			AddTriangle(ref v1, ref v2, ref v3);
		}


		// based on half-space function approach
		// base code from: http://forum.devmaster.net/t/advanced-rasterization/6145
		// further ideas from: https://www.gamedev.net/topic/637373-software-occlusion-culling-rasterizer-what-about-small-cracks/
		// inspiration from: http://www.frostbite.com/wp-content/uploads/2013/05/CullingTheBattlefield.pdf
		public void AddTriangle(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3)
		{
			// 28.4 fixed-point coordinates
			int Y1 = iround(16.0f * (v1.Y + 1) * halfHeight);
			int Y2 = iround(16.0f * (v2.Y + 1) * halfHeight);
			int Y3 = iround(16.0f * (v3.Y + 1) * halfHeight);

			int X1 = iround(16.0f * (v1.X + 1) * halfWidth);
			int X2 = iround(16.0f * (v2.X + 1) * halfWidth);
			int X3 = iround(16.0f * (v3.X + 1) * halfWidth);

			// Deltas
			int DX12 = X1 - X2;
			int DX23 = X2 - X3;
			int DX31 = X3 - X1;

			int DY12 = Y1 - Y2;
			int DY23 = Y2 - Y3;
			int DY31 = Y3 - Y1;

			// Fixed-point deltas
			int FDX12 = DX12 << 4;
			int FDX23 = DX23 << 4;
			int FDX31 = DX31 << 4;

			int FDY12 = DY12 << 4;
			int FDY23 = DY23 << 4;
			int FDY31 = DY31 << 4;

			// Bounding rectangle
			int minx = (min(X1, X2, X3) + 0xF) >> 4;
			int maxx = (max(X1, X2, X3) + 0xF) >> 4;
			int miny = (min(Y1, Y2, Y3) + 0xF) >> 4;
			int maxy = (max(Y1, Y2, Y3) + 0xF) >> 4;


			if (minx < 0 && maxx < 0) return;
			if (miny < 0 && maxy < 0) return;
			if (minx > width && maxx > width) return;
			if (miny > height && maxy > height) return;

			if (minx < 0) minx = 0;
			if (maxx >= widthMinusOne) maxx = widthMinusOne;
			if (miny < 0) miny = 0;
			if (maxy >= heightMinusOne) maxy = heightMinusOne;

			float maxz = max(v1.Z, v2.Z, v3.Z);
			int depthIndex = miny * width;


			// Half-edge ants
			int C1 = DY12 * X1 - DX12 * Y1;
			int C2 = DY23 * X2 - DX23 * Y2;
			int C3 = DY31 * X3 - DX31 * Y3;

			// Correct for fill convention
			if (DY12 < 0 || (DY12 == 0 && DX12 > 0)) C1++;
			if (DY23 < 0 || (DY23 == 0 && DX23 > 0)) C2++;
			if (DY31 < 0 || (DY31 == 0 && DX31 > 0)) C3++;

			int CY1 = C1 + DX12 * (miny << 4) - DY12 * (minx << 4);
			int CY2 = C2 + DX23 * (miny << 4) - DY23 * (minx << 4);
			int CY3 = C3 + DX31 * (miny << 4) - DY31 * (minx << 4);

			for (int y = miny; y < maxy; y++)
			{
				int CX1 = CY1;
				int CX2 = CY2;
				int CX3 = CY3;

				for (int x = minx; x < maxx; x++)
				{
					if (CX1 > 0 && CX2 > 0 && CX3 > 0)
					{
						var index = depthIndex + x;
						if (index > 0 && index < depthBuffer.Length)
						{
							if (depthBuffer[index] == 0 || depthBuffer[index] > maxz)
								depthBuffer[index] = maxz;
						}
					}

					CX1 -= FDY12;
					CX2 -= FDY23;
					CX3 -= FDY31;
				}

				CY1 += FDX12;
				CY2 += FDX23;
				CY3 += FDX31;

				depthIndex += width;
			}
		}


		public bool BoundsVisible(CameraSpaceBounds bounds)
		{
			return true;
		}

		// http://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
		public Vector3 CalculateBarycentric(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 atPoint)
		{
			var v0 = b - a;
			var v1 = c - a;
			var v2 = atPoint - a;
			var d00 = v0.Dot(ref v0);
			var d01 = v0.Dot(ref v1);
			var d11 = v1.Dot(ref v1);
			var d20 = v2.Dot(ref v0);
			var d21 = v2.Dot(ref v1);
			var denom = d00 * d11 - d01 * d01;
			var result = new Vector3();
			result.Y = (d11 * d20 - d01 * d21) / denom;
			result.Z = (d00 * d21 - d01 * d20) / denom;
			result.X = 1.0f - result.Y - result.Z;
			return result;
		}


		ImageViewer viewer;
		public void UpdateDebugView()
		{
			if (viewer != null)
			{
				var min = depthBuffer.Where(d => d != 0).DefaultIfEmpty(0).Min();
				var max = depthBuffer.Where(d => d != 0).DefaultIfEmpty(float.MaxValue).Max();
				viewer.SetData(width, height, (x, y) =>
				{
					var d = depthBuffer[(heightMinusOne - y) * width + x];
					if (d == 0) return new Vector4(0, 0, 0.3f, 1);
					return new Vector4(Vector3.One * ((d - min) / (max - min)), 1);
				});
			}
		}
		public void Show()
		{
			Hide();
			viewer = ImageViewer.ShowNew();
			UpdateDebugView();
		}
		public void Hide()
		{
			if (viewer != null)
			{
				viewer.Hide();
				viewer = null;
			}
		}

		//public void Add(Mesh mesh)
		//{
		//	var indicies = mesh.TriangleIndicies;
		//	var vertices = mesh.Vertices;

		//	for (int i = 0; i < indicies.Count; i += 3)
		//	{
		//		var a = Vector3.TransformVector(vertices[indicies[i + 0]], transformMatrix);
		//		var b = Vector3.TransformVector(vertices[indicies[i + 1]], transformMatrix);
		//		var c = Vector3.TransformVector(vertices[indicies[i + 2]], transformMatrix);
		//	}
		//}
	}
}
