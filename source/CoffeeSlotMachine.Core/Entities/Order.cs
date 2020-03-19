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
        public int ThrownInCents => -1;

        /// <summary>
        /// Summe der Cents die zurückgegeben werden
        /// </summary>
        public int ReturnCents => -1;


        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        /// <summary>
        /// Kann der Automat mangels Kleingeld nicht
        /// mehr herausgeben, wird der Rest als Spende verbucht
        /// </summary>
        public int DonationCents => -1;

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
        public void FinishPayment(IEnumerable<Coin> coins)
        {
            throw new NotImplementedException();
        }
    }
}
