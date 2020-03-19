using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeSlotMachine.Core.Entities
{
    /// <summary>
    /// Bestellung verwaltet das bestellte Produkt, die eingeworfenen Münzen und
    /// die Münzen die zurückgegeben werden.
    /// </summary>
    public class Order : EntityObject
    {
        /// <summary>
        /// Datum und Uhrzeit der Bestellung
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Werte der eingeworfenen Münzen als Text. Die einzelnen 
        /// Münzwerte sind durch ; getrennt (z.B. "10;20;10;50")
        /// </summary>
        public String ThrownInCoinValues { get; set; }

        /// <summary>
        /// Zurückgegebene Münzwerte mit ; getrennt
        /// </summary>
        public String ReturnCoinValues { get; set; }

        /// <summary>
        /// Summe der eingeworfenen Cents.
        /// </summary>
        public int ThrownInCents => CalculateThrownInCents();

        private int CalculateThrownInCents()
        {
            string[] coins = ThrownInCoinValues?.Split(';');
            int result = 0;

            foreach (var item in coins)
            {
                result += Convert.ToInt32(item);
            }

            return result;
        }

        /// <summary>
        /// Summe der Cents die zurückgegeben werden
        /// </summary>
        public int ReturnCents => ThrownInCents - Product.PriceInCents;

        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        /// <summary>
        /// Kann der Automat mangels Kleingeld nicht
        /// mehr herausgeben, wird der Rest als Spende verbucht
        /// </summary>
        [NotMapped]
        public int DonationCents { get; set; }

        /// <summary>
        /// Münze wird eingenommen.
        /// </summary>
        /// <param name="coinValue"></param>
        /// <returns>isFinished ist true, wenn der Produktpreis zumindest erreicht wurde</returns>
        public bool InsertCoin(int coinValue)
        {
            if (coinValue >= Product.PriceInCents)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Übernahme des Einwurfs in das Münzdepot.
        /// Rückgabe des Retourgeldes aus der Kasse. Staffelung des Retourgeldes
        /// hängt vom Inhalt der Kasse ab.
        /// </summary>
        /// <param name="coins">Aktueller Zustand des Münzdepots</param>
        public void FinishPayment(List<Coin> coins)
        {
            coins.Reverse();

            int possibleReturn = 0;
            int returnCencts = ReturnCents;
            if (returnCencts > 0)
            {
                foreach (var coin in coins)
                {
                    if (coin.CoinValue <= returnCencts)
                    {
                        possibleReturn += coin.CoinValue * coin.Amount;
                    }
                }

                if (possibleReturn >= returnCencts)
                {
                    foreach (var coin in coins)
                    {
                        while (coin.Amount > 0 && returnCencts >= coin.CoinValue)
                        {
                            if (string.IsNullOrEmpty(ReturnCoinValues))
                            {
                                ReturnCoinValues = $"{coin.CoinValue}";
                                coin.Amount--;
                            }
                            else
                            {
                                ReturnCoinValues += $";{coin.CoinValue}";
                                coin.Amount--;
                            }
                            returnCencts -= coin.CoinValue;
                        }
                    }
                }
            }

            if (returnCencts > 0)
            {
                DonationCents = returnCencts;
            }
            else
            {
                DonationCents = 0;
            }
        }
    }
}
