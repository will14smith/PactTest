using System;
using System.Linq;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PactTest.Web.Controllers;
using PactTest.Web.Models;
using Xunit;

namespace PactTest.Web.Tests
{
    public class OrderControllerTests
    {
        private readonly Fixture _fixture = new Fixture();
        
        private readonly OrderStore _store;
        private readonly OrderController _sut;

        public OrderControllerTests()
        {
            _store = new OrderStore();
            _sut = new OrderController(_store);
        }

        [Fact]
        public void GetAll_ShouldReturnAllItemsFromStore()
        {
            for (var i = 0; i < 10; i++)
            {
                _store.Add(_fixture.Create<Order>());
            }
            var expected = _store.GetAll();
            
            var result = _sut.GetAll();

            Assert.Equal(expected, result);
        }   

        [Fact]
        public void Get_ShouldReturnCorrectItemFromStore()
        {
            _store.Add(_fixture.Create<Order>());
            var expected = _store.Add(_fixture.Create<Order>());
            _store.Add(_fixture.Create<Order>());
            
            var result = _sut.GetById(expected.Id);

            Assert.Equal(expected, result.Value);
        }    
        
        [Fact]
        public void Get_InvalidId_ShouldReturnNotFound()
        {
            var result = _sut.GetById(1);

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }   

        [Fact]
        public void Add_ShouldAddToStore()
        {
            var order = _fixture.Create<Order>();

            _sut.Add(order);

            var result = Assert.Single(_store.GetAll());
            Assert.Equal(order, result);
        } 
        
        [Fact]
        public void Add_ShouldReturnRedirect()
        {
            var order = _fixture.Create<Order>();

            var result = _sut.Add(order);

            var resultOrder = _store.GetAll().Single();
            var redirect = Assert.IsAssignableFrom<CreatedAtActionResult>(result);
            Assert.Equal(nameof(OrderController.GetById), redirect.ActionName);
            Assert.Equal(new RouteValueDictionary { { "Id", resultOrder.Id } }, redirect.RouteValues);
            Assert.Equal(resultOrder, redirect.Value);
        }
        
        [Fact]
        public void Update_ShouldUpdateItemInStore()
        {
            var order1 = _store.Add(_fixture.Create<Order>());
            var order2 = _store.Add(_fixture.Create<Order>());
            var updatedOrder1 = _fixture.Build<Order>()
                .With(x => x.Id, order1.Id)
                .Create();
            
            _sut.Update(order1.Id, updatedOrder1);

            Assert.Equal(updatedOrder1, _store.GetById(order1.Id));
            Assert.Equal(order2, _store.GetById(order2.Id));
        }   
        
        [Fact]
        public void Update_ShouldRedirectToOrder()
        {
            var order = _store.Add(_fixture.Create<Order>());
            
            var result = _sut.Update(order.Id, _fixture.Create<Order>());

            var redirect = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.Equal(nameof(OrderController.GetById), redirect.ActionName);
            Assert.Equal(new RouteValueDictionary { { "Id", order.Id } }, redirect.RouteValues);
        }   
        
        [Fact]
        public void Update_InvalidId_ShouldReturnNotFound()
        {
            var result = _sut.Update(1, _fixture.Create<Order>());

            Assert.IsAssignableFrom<NotFoundResult>(result);
        }    
        
        [Fact]
        public void Delete_ShouldRemoveFromStore()
        {
            var order1 = _store.Add(_fixture.Create<Order>());
            var order2 = _store.Add(_fixture.Create<Order>());
            
            _sut.Delete(order1.Id);

            Assert.Null(_store.GetById(order1.Id));
            Assert.NotNull(_store.GetById(order2.Id));
        }  
        
        [Fact]
        public void Delete_ShouldReturnNoContent()
        {
            var order = _store.Add(_fixture.Create<Order>());
            
            var result = _sut.Delete(order.Id);

            Assert.IsAssignableFrom<NoContentResult>(result);
        } 
        
        [Fact]
        public void Delete_InvalidId_ShouldReturnNotFound()
        {
            var result = _sut.Delete(1);

            Assert.IsAssignableFrom<NotFoundResult>(result);
        } 
    }
}