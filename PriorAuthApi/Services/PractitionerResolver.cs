using Microsoft.EntityFrameworkCore;
using PriorAuth.Data;
using PriorAuth.Data.Entities;

namespace PriorAuthApi.Services;

public interface IPractitionerResolver
{
    Task<Practitioner?> ResolveCurrentAsync(CancellationToken ct = default);
}

public class PractitionerResolver : IPractitionerResolver
{
    private readonly IHttpContextAccessor _http;
    private readonly AppDbContext _db;

    public PractitionerResolver(IHttpContextAccessor http, AppDbContext db)
    {
        _http = http;
        _db = db;
    }

    public async Task<Practitioner?> ResolveCurrentAsync(CancellationToken ct = default)
    {
        var user = _http.HttpContext?.User;
        if (user is null) return null;

        // Microsoft.Identity.Web maps the oid claim to this URI; the short "oid"
        // is also present. Check both rather than assuming one.
        var oidClaim = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                       ?? user.FindFirst("oid")?.Value;

        if (!Guid.TryParse(oidClaim, out var oid))
            return null;

        return await _db.Practitioners
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.EntraOid == oid, ct);
    }
}