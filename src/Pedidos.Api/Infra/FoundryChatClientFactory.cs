using System.ClientModel.Primitives;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

namespace Pedidos.Api.Infra;

public static class FoundryChatClientFactory
{
    public static IChatClient Criar(string endpoint, string deployment)
    {
        // Entra ID: nenhuma chave de API no código, no appsettings ou no prompt
        var politica = new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default");

#pragma warning disable OPENAI001
        var chatClient = new ChatClient(deployment, politica,
            new OpenAIClientOptions { Endpoint = new Uri(new Uri(endpoint), "openai/v1/") });
#pragma warning restore OPENAI001

        return chatClient.AsIChatClient();
    }
}
