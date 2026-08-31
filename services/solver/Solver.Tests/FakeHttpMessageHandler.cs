using System.Net;

namespace Solver.Tests;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly string _responseJson;
    public string? LastRequestUri { get; private set; }

    public FakeHttpMessageHandler(string responseJson)
    {
        _responseJson = responseJson;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri?.ToString();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_responseJson)
        };
        return Task.FromResult(response);
    }
}
