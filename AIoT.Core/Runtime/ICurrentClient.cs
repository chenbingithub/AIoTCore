namespace AIoT.Core.Runtime
{
    public interface ICurrentClient
    {
        string Id { get; }

        bool IsAuthenticated { get; }
       
    }
}