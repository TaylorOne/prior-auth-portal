using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace PriorAuthApi.Data;

public class TransientLoginRetryStrategy : SqlServerRetryingExecutionStrategy
{
    public TransientLoginRetryStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies,
               maxRetryCount: 5,
               maxRetryDelay: TimeSpan.FromSeconds(10),
               errorNumbersToAdd: null) { }

    protected override bool ShouldRetryOn(Exception exception)
    {
        if (base.ShouldRetryOn(exception)) return true;

        for (var ex = exception; ex != null; ex = ex.InnerException)
        {
            if (ex is System.Net.Sockets.SocketException sock &&
                (sock.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset ||
                 sock.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted))
                return true;
            if (ex is System.IO.IOException) return true;
        }
        return false;
    }
}