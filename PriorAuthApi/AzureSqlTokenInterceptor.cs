using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using Azure.Identity;
using Microsoft.Data.SqlClient;

public class AzureSqlTokenInterceptor : DbConnectionInterceptor
{
    private readonly DefaultAzureCredential _credential;

    public AzureSqlTokenInterceptor(DefaultAzureCredential credential)
    {
        _credential = credential;
    }

    public override InterceptionResult ConnectionOpening(
        DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
    {
        if (connection is SqlConnection sqlConnection)
        {
            sqlConnection.AccessToken = GetToken();
        }
        return base.ConnectionOpening(connection, eventData, result);
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
    {
        if (connection is SqlConnection sqlConnection)
        {
            sqlConnection.AccessToken = await GetTokenAsync(cancellationToken);
        }
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    private string GetToken()
    {
        return _credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" })).Token;
    }

    private async ValueTask<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var tokenResult = await _credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }), 
            cancellationToken);
        return tokenResult.Token;
    }
}