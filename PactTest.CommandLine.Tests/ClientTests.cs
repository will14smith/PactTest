using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;

namespace PactTest.CommandLine.Tests
{
    public class ClientTests : IClassFixture<OrderConsumerPact>
    {
        private readonly IFixture _fixture = new DeterministicFixture(10);

        private readonly IMockProviderService _mockProviderService;

        private readonly Client _sut;
        
        public ClientTests(OrderConsumerPact data)
        {
            _mockProviderService = data.MockProviderService;
            _mockProviderService.ClearInteractions();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(data.MockProviderServiceBaseUri)
            };
            _sut = new Client(httpClient);
        }
        
        [Fact]
        public async Task GetAll_ShouldGetAllItemsFromApi()
        {
            var order1 = _fixture.Create<Order>();
            var order2 = _fixture.Create<Order>();
            
            _mockProviderService
                .Given("There are 2 orders in the store")
                .UponReceiving("A GET request to list all the orders")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = "/order",
                    Headers = new Dictionary<string, object>
                    {
                        { "Accept", "application/json" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, object>
                    {
                        { "Content-Type", "application/json" }
                    },
                    Body = new []
                    {
                        order1,
                        order2,
                    }
                });
            
            var result = await _sut.GetAllAsync();
            
            Assert.Collection(result, 
                actual1 => AssertOrder(order1, actual1),
                actual2 => AssertOrder(order2, actual2));
            
            _mockProviderService.VerifyInteractions();
        }

        private static void AssertOrder(Order expected, Order actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Person, actual.Person);
            Assert.Equal(expected.Item, actual.Item);
        }
    }
}