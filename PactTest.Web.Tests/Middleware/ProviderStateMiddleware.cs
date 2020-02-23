using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PactTest.Web.Models;

namespace PactTest.Web.Tests.Middleware
{
    public class ProviderStateMiddleware
    {
        private const string ConsumerName = "CommandLine";
        
        private readonly RequestDelegate _next;
        private readonly IDictionary<string, Action> _providerStates;

        public ProviderStateMiddleware(RequestDelegate next, OrderStore store)
        {
            _next = next;
            _providerStates = new Dictionary<string, Action>
            {
                {
                    "There are 2 orders in the store",
                    () =>
                    {
                        foreach (var order in store.GetAll())
                        {
                            store.Delete(order.Id);
                        }
                        
                        store.Add(new Order { Person = "person1", Item = "item1" });
                        store.Add(new Order { Person = "person2", Item = "item2" });
                    }
                }
            };
        }
        
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value == "/provider-states")
            {
                HandleProviderStatesRequestAsync(context);
                await context.Response.WriteAsync(string.Empty);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleProviderStatesRequestAsync(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            if (!string.Equals(context.Request.Method, HttpMethod.Post.ToString(), StringComparison.CurrentCultureIgnoreCase) || context.Request.Body == null)
            {
                return;
            }
            
            string jsonRequestBody;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                jsonRequestBody = await reader.ReadToEndAsync();
            }

            var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);

            // A null or empty provider state key must be handled
            if (providerState != null && !string.IsNullOrEmpty(providerState.State) && providerState.Consumer == ConsumerName)
            {
                _providerStates[providerState.State].Invoke();
            }
        }
    }
}