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
        private string _providerUri;
        private string _pactServiceUri;
        private IWebHost _webHost;
        private ITestOutputHelper _outputHelper;

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
                Outputters = new List<IOutput> { new XUnitOutput(_outputHelper) },
                Verbose = true
            };

            var verifier = new PactVerifier(config);
            verifier
                .ProviderState($"{_pactServiceUri}/provider-states")
                .ServiceProvider("OrderApi", _providerUri)
                
                .HonoursPactWith("CommandLine")
                .PactUri(@"..\..\..\..\PactTest.CommandLine.Tests\pacts\commandline-orderapi.json")
                
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