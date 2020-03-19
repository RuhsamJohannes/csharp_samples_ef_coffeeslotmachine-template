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
                return new Order()
                {
                    ProductId = product.Id,
                    Time = DateTime.Now,
                    Product = product
                };
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
            
            order.ThrownInCoinValues += coinValue.ToString() + ';';
            List<Coin> coins = StringCoinValuesToList(order);

            int coinSum = 0;

            foreach (Coin item in coins)
            {
                coinSum += item.CoinValue;
            } 

            if (order.InsertCoin(coinSum))
            {
                int difference = coinSum - order.Product.PriceInCents;

                if(difference > 5)
                {

                }


                return true;
            }

            return false;
        }

        private List<Coin> StringCoinValuesToList(Order order)
        {
            string[] coinsString = order.ThrownInCoinValues?.Split(';');
            List<Coin> coins = new List<Coin>();

            if (coinsString != null)
            {
                for (int i = 0; i < coinsString.Count(); i++)
                {
                    if (int.TryParse(coinsString[i], out int value))
                    {
                        Coin coin = coins.Find(c => c.CoinValue == value);
                        if (coin == null)
                        {
                            coins.Add(new Coin
                            {
                                Amount = +1,
                                CoinValue = value
                            });
                        }
                        else
                        {
                            coin.Amount++;
                        }
                    }
                }
            }

            return coins;
        }

        /// <summary>
        /// Gibt den aktuellen Inhalt der Kasse, sortiert nach Münzwert absteigend zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Coin> GetCoinDepot()
        {
            return _coinRepository.GetAllCoins()
                .OrderBy(c => c.CoinValue);
        }


        /// <summary>
        /// Gibt den Inhalt des Münzdepots als String zurück
        /// </summary>
        /// <returns></returns>
        public string GetCoinDepotString()
        {
            Coin[] coins = GetCoinDepot().ToArray();

            string result = $"{coins[0].Amount}*{coins[0].CoinValue}";

            for (int i = 1; i < coins.Length; i++)
            {
                result += $"{coins[i].Amount}*{coins[i].CoinValue}";
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
