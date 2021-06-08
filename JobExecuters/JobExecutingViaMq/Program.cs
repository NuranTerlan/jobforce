using System;
using JobExecutingViaMq.Helpers;

namespace JobExecutingViaMq
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Job Executing Service (JES) is starting..\n\n");
            MqHelpers.WaitMessages();
        }
    }
}