using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactNet;
using PactNet.Mocks.MockHttpService;

namespace PactTest.CommandLine.Tests
{
    public class OrderConsumerPact : IDisposable
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        
        public IPactBuilder PactBuilder { get; }
        public IMockProviderService MockProviderService { get; }
        
        public int MockServerPort => 9222;
        public string MockProviderServiceBaseUri => $"http://localhost:{MockServerPort}";

        public OrderConsumerPact()
        {
            PactBuilder = new PactBuilder(new PactConfig { SpecificationVersion = "2.0.0" });
            
            PactBuilder
                .ServiceConsumer("CommandLine")
                .HasPactWith("OrderApi");
            
            MockProviderService = PactBuilder.MockService(MockServerPort, JsonSettings);
        }
        
        public void Dispose()
        {
            PactBuilder.Build();
        }
    }
}