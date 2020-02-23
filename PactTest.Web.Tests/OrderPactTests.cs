using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactTest.Web.Tests.Middleware;
using Xunit;
using Xunit.Abstractions;

namespace PactTest.Web.Tests
{
    public class OrderPactTests : IDisposable
    {
        private readonly string _providerUri;
        private readonly string _pactServiceUri;
        private readonly IWebHost _webHost;
        private readonly ITestOutputHelper _outputHelper;

        public OrderPactTests(ITestOutputHelper output)
        {
            _outputHelper = output;
            _providerUri = "http://localhost:9001";
            _pactServiceUri = "http://localhost:9001";
            
            _webHost = WebHost.CreateDefaultBuilder()
                .UseUrls(_pactServiceUri)
                .ConfigureServices(services => services.AddTransient<IStartupFilter, ProviderStateMiddlewareStartupFilter>())
                .UseStartup<Startup>()
                .Build();

            _webHost.Start();
        }

        [Fact]
        public void OrderApi_ShouldHonourConsumerPact()
        {
            var config = new PactVerifierConfig
            {
                ProviderVersion = "1.0.1",
                PublishVerificationResults = true,
                
                Outputters = new List<IOutput> { new XUnitOutput(_outputHelper) },
                Verbose = true
            };

            var verifier = new PactVerifier(config);
            verifier
                .ProviderState($"{_pactServiceUri}/provider-states")
                .ServiceProvider("OrderApi", _providerUri)
                
                .PactBroker("http://localhost:9292", enablePending: true)
                
                .Verify();
        }

        public void Dispose()
        {
            _webHost.StopAsync().Wait();
            _webHost.Dispose();
        }
    }

    public class XUnitOutput : IOutput
    {
        private readonly ITestOutputHelper _output;

        public XUnitOutput(ITestOutputHelper output)
        {
            _output = output;
        }

        public void WriteLine(string message)
        {
            _output.WriteLine(message);
        }
    }
}