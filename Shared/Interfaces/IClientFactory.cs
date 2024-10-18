namespace Shared.Interfaces
{
    public interface IClientFactory
    {
        IClient GetClient(string clientType);
    }
}
