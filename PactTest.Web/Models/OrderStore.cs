using System.Collections.Generic;
using System.Linq;

namespace PactTest.Web.Models
{
    public class OrderStore
    {
        private readonly List<Order> _orders = new List<Order>();

        public IReadOnlyCollection<Order> GetAll()
        {
            return _orders.ToList();
        }
        
        public Order GetById(int id)
        {
            return _orders.Find(x => x.Id == id);
        }
        
        public Order Add(Order order)
        {
            order.Id = _orders.Count + 1;
            
            _orders.Add(order);
            
            return order;
        }

        public Order Update(Order order)
        {
            var index = _orders.FindIndex(x => x.Id == order.Id);
            if (index == -1)
            {
                return null;
            }
            
            _orders[index] = order;

            return order;
        }

        public bool Delete(int id)
        {
            var index = _orders.FindIndex(x => x.Id == id);
            if (index == -1)
            {
                return false;
            }
            
            _orders.RemoveAt(index);

            return true;
        }
    }
}