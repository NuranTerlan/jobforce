using System;
using BaseJobs;

namespace PhysicsJobs
{
    public class PhysicsTest : IJob
    {
        public void Run()
        {
            Console.WriteLine($"Called from {nameof(PhysicsTest)}..");
        }
    }
}