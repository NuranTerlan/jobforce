using System;
using BaseJobs;

namespace MathJobs
{
    public class MathTest : IJob
    {
        public void Run()
        {
            Console.WriteLine($"Called from {nameof(MathTest)}..");
        }
    }
}