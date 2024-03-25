using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace MediGuru.DataExtractionTool.Helpers;

internal static class ElasticSearchClientCreator
{
    public static ElasticsearchClient Create(string username, string password, string host,  string port)
    {
        var clientSettings = new ElasticsearchClientSettings(new Uri($"https://{host}:{port}"));
        clientSettings.MaximumRetries(5);
        clientSettings.RequestTimeout(TimeSpan.FromMinutes(1));
        clientSettings.Authentication(new BasicAuthentication(username, password));
        clientSettings.ThrowExceptions(true);
        clientSettings.EnableDebugMode();
        clientSettings.ServerCertificateValidationCallback(
                    (obj, certificate, chain, errors) => true);

        return new ElasticsearchClient(clientSettings);
    }
}
