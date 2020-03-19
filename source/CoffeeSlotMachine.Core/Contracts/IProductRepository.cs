using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Contracts
{
    public interface IProductRepository
    {
        Product[] GetAllProducts();
        Product GetProductById(int id);
    }
}