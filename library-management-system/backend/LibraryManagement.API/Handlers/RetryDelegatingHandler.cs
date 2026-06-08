namespace LibraryManagement.API.Handlers;

public class RetryDelegatingHandler : DelegatingHandler
{
    private const int MaxRetries = 3;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return response;
            if (attempt < MaxRetries)
                await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
        }

        return response!;
    }
}
