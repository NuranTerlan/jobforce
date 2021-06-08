using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using BaseJobs;

namespace JobExecutingViaMq.Helpers
{
    public static class ReflectionHelpers
    {
        private static readonly NameValueCollection AppConfig;

        static ReflectionHelpers()
        {
            AppConfig = ConfigurationManager.AppSettings;
        }
        
        // public static Dictionary<string, Type> GetDerivedClassesOf(string interfaceName)
        // {
        //     var nameTypeDict = new Dictionary<string, Type>();
        //     
        //     var assembly =
        //         Assembly.LoadFrom(AppConfig.Get("job-implementations-source") ?? string.Empty);
        //     
        //     Console.WriteLine($"Searching for children of {interfaceName}:");
        //     foreach (var type in assembly.GetExportedTypes())
        //     {
        //         if (type.GetInterface(interfaceName, true) != null)
        //         {
        //             Console.WriteLine($"\tFound Type: {type.Name}");
        //             nameTypeDict.Add(type.Name, type);
        //         }
        //     }
        //
        //     return nameTypeDict;
        // }

        private static Type GetJobFromDll(string jobName, string dll)
        {
            var assembly =
                Assembly.LoadFrom(dll);

            var baseJob = AppConfig.Get("base-job-abstraction") ?? string.Empty;
            var returnedType = assembly.GetExportedTypes().FirstOrDefault(type => type.Name.Equals(jobName));

            if (returnedType is null)
            {
                Console.WriteLine($"Can't find {jobName} at {dll}!");
                return null;
            }
            
            if (returnedType.GetInterface(baseJob, true) is null)
            {
                Console.WriteLine($"{jobName} is not a job!");
                return null;
            }

            return returnedType;
        }
        
        public static void ExecuteJob(string jobName, string dll)
        {
            var type = GetJobFromDll(jobName, dll);
            if (type is null) return;
            
            var job = (IJob) type.InvokeMember(null!,
                BindingFlags.DeclaredOnly |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.CreateInstance,
                null, null, null);

            if (job is null)
            {
                Console.WriteLine($"Can't find definition for {jobName}! Sorry..");
                return;
            }
            Console.WriteLine($"\nExecution for job #{jobName} is starting:");
            job.Run();
            Console.WriteLine($"Execution for job #{jobName} ended!\n");
        }
    }
}