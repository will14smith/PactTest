using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PactTest.Web.Models;

namespace PactTest.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderStore _store;

        public OrderController(OrderStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IEnumerable<Order> GetAll()
        {
            return _store.GetAll();
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Order> GetById(int id)
        {
            var result = _store.GetById(id);
            if (result == null)
            {
                return NotFound();
            }

            return result;
        }  
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult Add([FromBody] Order order)
        {
            var result = _store.Add(order);

            return CreatedAtAction(nameof(GetById), result.Id);
        }   
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(int id, [FromBody] Order order)
        {
            order.Id = id;
            
            var result = _store.Update(order);
            if (result == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(GetById), result.Id);
        }       
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            if (!_store.Delete(id))
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}