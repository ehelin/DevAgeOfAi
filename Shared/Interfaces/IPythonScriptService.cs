using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IPythonScriptService
    {
        Task StartAsync();
        Task<string> SendInputAsync(string input);
        void Stop();
    }
}
