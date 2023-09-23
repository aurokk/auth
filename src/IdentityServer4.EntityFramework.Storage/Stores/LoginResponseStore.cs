#nullable enable

using IdentityServer4.Storage.Stores;
using Newtonsoft.Json;

namespace IdentityServer4.EntityFramework.Storage.Stores;

public sealed record LoginResponseData(
    Guid LoginRequestId,
    string? SubjectId,
    bool IsSuccess
);

public class LoginResponseStore : ILoginResponseStore
{
    private readonly IStore _store;

    public LoginResponseStore(IStore store)
    {
        _store = store;
    }

    public async Task<LoginResponse> Get(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        var storeItem = await _store.Get(storeItemKey, ct);
        var data = JsonConvert.DeserializeObject<LoginResponseData>(storeItem.Value) ??
                   throw new ApplicationException();
        var loginResponse = new LoginResponse(
            Id: id,
            LoginRequestId: data.LoginRequestId,
            SubjectId: data.SubjectId,
            IsSuccess: data.IsSuccess,
            CreatedAtUtc: storeItem.CreatedAtUtc,
            RemoveAtUtc: storeItem.RemoveAtUtc
        );
        return loginResponse;
    }

    public async Task Create(LoginResponse loginResponse, CancellationToken ct)
    {
        var storeItemKey = BuildKey(loginResponse.Id);
        var data = new LoginResponseData(
            LoginRequestId: loginResponse.LoginRequestId,
            SubjectId: loginResponse.SubjectId,
            IsSuccess: loginResponse.IsSuccess
        );
        var value = JsonConvert.SerializeObject(data);
        var storeItem = new StoreItem(
            Key: storeItemKey,
            Value: value,
            CreatedAtUtc: loginResponse.CreatedAtUtc,
            RemoveAtUtc: loginResponse.RemoveAtUtc
        );
        await _store.Create(storeItem, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        var storeItemKey = BuildKey(id);
        await _store.Delete(storeItemKey, ct);
    }

    private static string BuildKey(Guid id) => $"{id:N}_loginresponse";
}