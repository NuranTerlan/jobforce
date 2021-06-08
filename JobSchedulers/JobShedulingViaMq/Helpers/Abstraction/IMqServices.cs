using System.Threading.Tasks;

namespace JobShedulingViaMq.Helpers.Abstraction
{
    public interface IMqServices
    {
        void PublishExecutingJob(string jobName);
    }
}