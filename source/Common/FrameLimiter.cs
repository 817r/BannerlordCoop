﻿using System;
using System.Diagnostics;
using System.Threading;

namespace Common
{
    public class FrameLimiter
    {
        private readonly MovingAverage m_Avg;
        private readonly long m_TargetTicksPerFrame;
        private readonly Stopwatch m_Timer;
        private readonly Action<long> m_WaitUntilTick;
        public TimeSpan LastFrameTime;
        private double m_AverageTicksPerFrame;

        public FrameLimiter(TimeSpan targetFrameTime)
        {
            m_TargetTicksPerFrame = targetFrameTime.Ticks;
            m_Timer = Stopwatch.StartNew();
            if (targetFrameTime == TimeSpan.Zero)
            {
                m_WaitUntilTick = tick => { };
            }
            else if (targetFrameTime > TimeSpan.FromMilliseconds(1))
            {
                m_WaitUntilTick = tick =>
                {
                    TimeSpan waitTime = TimeSpan.FromTicks(tick) - m_Timer.Elapsed;
                    if (waitTime > TimeSpan.Zero)
                    {
                        Thread.Sleep(waitTime);
                    }
                };
            }
            else
            {
                m_WaitUntilTick = tick =>
                {
                    SpinWait.SpinUntil(() => m_Timer.ElapsedTicks >= tick);
                };
            }

            m_Avg = new MovingAverage(32);
        }

        public TimeSpan AverageFrameTime => TimeSpan.FromTicks((long) m_AverageTicksPerFrame);

        public void Throttle()
        {
            long elapsedTicks = m_Timer.Elapsed.Ticks;
            m_AverageTicksPerFrame = m_Avg.Push(elapsedTicks);
            if (m_AverageTicksPerFrame < m_TargetTicksPerFrame)
            {
                m_WaitUntilTick(
                    elapsedTicks + (m_TargetTicksPerFrame - (long) m_AverageTicksPerFrame));
            }

            LastFrameTime = m_Timer.Elapsed;
            m_Timer.Restart();
        }
    }
}
