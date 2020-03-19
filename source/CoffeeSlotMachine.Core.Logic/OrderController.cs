using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using CoffeeSlotMachine.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeSlotMachine.Core.Logic
{
    /// <summary>
    /// Verwaltet einen Bestellablauf. 
    /// </summary>
    public class OrderController : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICoinRepository _coinRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderController()
        {
            _dbContext = new ApplicationDbContext();

            _coinRepository = new CoinRepository(_dbContext);
            _orderRepository = new OrderRepository(_dbContext);
            _productRepository = new ProductRepository(_dbContext);
        }


        /// <summary>
        /// Gibt alle Produkte sortiert nach Namen zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Product> GetProducts()
        {
            return _productRepository.GetAllProducts()
                .OrderBy(p => p.Name);
        }

        /// <summary>
        /// Eine Bestellung wird für das Produkt angelegt.
        /// </summary>
        /// <param name="product"></param>
        public Order OrderCoffee(Product product)
        {
            if (product != null)
            {
                Order order = new Order()
                {
                    Time = DateTime.Now,
                    Product = product
                };

                _dbContext.Orders.Add(order);
                return order;
            }
            else
            {
                throw new ArgumentNullException(nameof(product));
            }
        }

        /// <summary>
        /// Münze einwerfen. 
        /// Wurde zumindest der Produktpreis eingeworfen, Münzen in Depot übernehmen
        /// und für Order Retourgeld festlegen. Bestellug abschließen.
        /// </summary>
        /// <returns>true, wenn die Bestellung abgeschlossen ist</returns>
        public bool InsertCoin(Order order, int coinValue)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
            else if (coinValue == 0)
            {
                throw new ArgumentException(nameof(order));
            }
            else
            {
                if (String.IsNullOrEmpty(order.ThrownInCoinValues))
                {
                    order.ThrownInCoinValues += coinValue.ToString();
                }
                else
                {
                    order.ThrownInCoinValues += $";{coinValue}";
                }

                if (order.InsertCoin(coinValue))
                {
                    string[] coins = order.ThrownInCoinValues.Split(';');
                    List<Coin> coinsList = _dbContext.Coins.ToList();

                    foreach (var item in coins)
                    {
                        coinsList.FirstOrDefault(v => v.CoinValue == Convert.ToInt32(item))
                                    .Amount++;
                    }
                    _dbContext.UpdateRange(coinsList);

                    order.FinishPayment(coinsList);

                    _dbContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gibt den aktuellen Inhalt der Kasse, sortiert nach Münzwert absteigend zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Coin> GetCoinDepot()
        {
            return _coinRepository.GetAllCoins()
                .OrderByDescending(c => c.CoinValue);
        }


        /// <summary>
        /// Gibt den Inhalt des Münzdepots als String zurück
        /// </summary>
        /// <returns></returns>
        public string GetCoinDepotString()
        {
            IEnumerable<Coin> coins = GetCoinDepot();

            string result = string.Empty;

            foreach (Coin coin in coins)
            {
                if (string.IsNullOrEmpty(result))
                {
                    result = $"{coin.Amount}*{coin.CoinValue}";
                }
                else
                {
                    result += $" + {coin.Amount}*{coin.CoinValue}";
                }
            }

            return result;
        }

        /// <summary>
        /// Liefert alle Orders inkl. der Produkte zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Order> GetAllOrdersWithProduct()
        {
            return _orderRepository.GetAllWithProduct();
        }

        /// <summary>
        /// IDisposable:
        ///
        /// - Zusammenräumen (zB. des ApplicationDbContext).
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
