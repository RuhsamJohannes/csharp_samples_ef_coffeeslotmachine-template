using CoffeeSlotMachine.Core.Logic;
using CoffeeSlotMachine.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CoffeeSlotMachine.ControllerTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
            {
                applicationDbContext.Database.EnsureDeleted();
                applicationDbContext.Database.Migrate();
            }
        }


        [TestMethod]
        public void T01_GetCoinDepot_CoinTypesCount_ShouldReturn6Types_3perType_SumIs1155Cents()
        {
            using (OrderController controller = new OrderController())
            {
                var depot = controller.GetCoinDepot().ToArray();
                Assert.AreEqual(6, depot.Count(), "Sechs Münzarten im Depot");
                foreach (var coin in depot)
                {
                    Assert.AreEqual(3, coin.Amount, "Je Münzart sind drei Stück im Depot");
                }

                int sumOfCents = depot.Sum(coin => coin.CoinValue * coin.Amount);
                Assert.AreEqual(1155, sumOfCents, "Beim Start sind 1155 Cents im Depot");
            }
        }

        [TestMethod]
        public void T02_GetProducts_9Products_FromCappuccinoToRistretto()
        {
            using (OrderController statisticsController = new OrderController())
            {
                var products = statisticsController.GetProducts().ToArray();
                Assert.AreEqual(9, products.Length, "Neun Produkte wurden erzeugt");
                Assert.AreEqual("Cappuccino", products[0].Name);
                Assert.AreEqual("Ristretto", products[8].Name);
            }
        }

        [TestMethod]
        public void T03_BuyOneCoffee_OneCoinIsEnough_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Cappuccino");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("20;10;5", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
                Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
            }
        }
        /*
        [TestMethod]
        public void T04_BuyOneCoffee_ExactThrowInOneCoin_CheckCoinsAndOrders()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void T05_BuyOneCoffee_MoreCoins_CheckCoinsAndOrders()
        {
            throw new NotImplementedException();
        }


        [TestMethod()]
        public void T06_BuyMoreCoffees_OneCoins_CheckCoinsAndOrders()
        {
            throw new NotImplementedException();
        }


        [TestMethod()]
        public void T07_BuyMoreCoffees_UntilDonation_CheckCoinsAndOrders()
        {
            throw new NotImplementedException();
        }*/

    }
}
