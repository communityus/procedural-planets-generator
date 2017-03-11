﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{

	public class FrameTime
	{
		Queue<DateTime> frameTimes1sec = new Queue<DateTime>();
		Queue<DateTime> frameTimes10sec = new Queue<DateTime>();
		System.Diagnostics.Stopwatch eventThreadTime = new System.Diagnostics.Stopwatch();

		/// <summary>
		/// Delta Time from last frame.
		/// </summary>
		public double DeltaTime { get; private set; }
		public double DeltaTime1Second { get; private set; }
		public double DeltaTime10Seconds { get; private set; }

		/// <summary>
		/// Fps of last frame.
		/// </summary>
		public double Fps { get; private set; }
		public double FpsPer1Sec { get; private set; }
		public double FpsPer10Sec { get; private set; }

		public double CurrentFrameElapsedSeconds => eventThreadTime.ElapsedMilliseconds / 1000.0f;
		public double CurrentFrameElapsedTimeFps => 1 / CurrentFrameElapsedSeconds;

		public void FrameBegan()
		{
			DeltaTime = eventThreadTime.ElapsedMilliseconds / 1000.0;
			eventThreadTime.Restart();
			if (DeltaTime > 0)
				Fps = 1 / DeltaTime;
			else
				Fps = double.Epsilon;

			var now = DateTime.Now;

			frameTimes1sec.Enqueue(now);
			frameTimes10sec.Enqueue(now);

			while ((now - frameTimes1sec.Peek()).TotalSeconds > 1) frameTimes1sec.Dequeue();
			while ((now - frameTimes10sec.Peek()).TotalSeconds > 10) frameTimes10sec.Dequeue();

			if ((now - frameTimes1sec.Peek()).TotalSeconds > 0.9)
				FpsPer1Sec = frameTimes1sec.Count;
			else
				FpsPer1Sec = Fps;

			if ((now - frameTimes10sec.Peek()).TotalSeconds > 9.9)
				FpsPer10Sec = frameTimes10sec.Count / 10.0;
			else
				FpsPer10Sec = FpsPer1Sec;

			DeltaTime1Second = 1.0 / FpsPer1Sec;
			DeltaTime10Seconds = 10.0 / FpsPer10Sec;
		}
		public override string ToString()
		{
			return $"FPS:{Fps.ToString("0.")}, avg over 1s {FpsPer1Sec.ToString("0.")}, avg over 10s:{FpsPer10Sec.ToString("0.")}";
		}
	}
}