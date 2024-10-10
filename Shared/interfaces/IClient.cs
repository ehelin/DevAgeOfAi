namespace Shared.Interfaces
{
    public interface IClient
    {
        Task<string> GetCompletionAsync(string prompt);
    }
}
