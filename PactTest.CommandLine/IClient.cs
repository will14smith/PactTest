using System.Collections.Generic;
using System.Threading.Tasks;

namespace PactTest.CommandLine
{
    public interface IClient
    {
        Task<IReadOnlyCollection<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);
        Task<Order> AddAsync(OrderAdd model);
        Task<Order> UpdateAsync(int id, OrderUpdate model);
        Task<bool> DeleteAsync(int id);
    }
}