//-----------------------------------------------------------------------
// <copyright file="Cache.cs" company="Eusebio Rufian-Zilbermann">
//   Copyright (c) Eusebio Rufian-Zilbermann
// </copyright>
//-----------------------------------------------------------------------
namespace Ticker
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;

   /// <summary>
   /// Two level Least Recently Used cache for a stream of stock quotes.
   /// </summary>
   public class Cache
   {
      /// <summary>
      /// Maximum Number of Securities to cache.
      /// </summary>
      private readonly int numSecurities;

      /// <summary>
      /// Maximum Number of prices to cache for each security.
      /// </summary>
      private readonly int numPrices;

      /// <summary>
      /// List of last N accessed securities, ordered by last access.
      /// </summary>
      private LinkedList<string> lastN;

      /// <summary>
      /// Relation of security names to price lists.
      /// </summary>
      private Dictionary<string, int> currentSymbols;

      /// <summary>
      /// Last N prices received for a specific security.
      /// </summary>
      private Queue<decimal>[] prices;

      /// <summary>
      /// Initializes a new instance of the <see cref="Cache"/> class.
      /// </summary>
      /// <param name="numSecurities">The maximum number of securities to cache.</param>
      /// <param name="numPrices">The maximum number of prices to cache for each security.</param>
      public Cache(int numSecurities, int numPrices)
      {
         this.numSecurities = numSecurities;
         this.numPrices = numPrices;
         this.lastN = new LinkedList<string>();
         this.currentSymbols = new Dictionary<string, int>(this.numSecurities);
         this.prices = new Queue<decimal>[this.numSecurities];
         this.InitializeWithDummyData();
      }

      /// <summary>
      /// Cache information for the latest quote tick.
      /// </summary>
      /// <param name="security">Name of the security in the quote tick.</param>
      /// <param name="price">Price of the security in the quote tick.</param>
      public void Tick(string security, decimal price)
      {
         if (this.currentSymbols.ContainsKey(security))
         {
            if (0 != string.Compare(this.lastN.Last.Value, security))
            {
               this.lastN.Remove(security); // This is potentially slow (comparatively), O(numSecurities)
               this.lastN.AddLast(security);
            }

            if (this.prices[this.currentSymbols[security]].Count >= this.numPrices)
            {
               this.prices[this.currentSymbols[security]].Dequeue();
            }

            this.prices[this.currentSymbols[security]].Enqueue(price);
         }
         else
         {
            string removedSecurity = this.lastN.First.Value;
            int removedIndex = this.currentSymbols[removedSecurity];
            this.lastN.RemoveFirst();
            this.lastN.AddLast(security);
            this.prices[removedIndex] = new Queue<decimal>();
            this.prices[removedIndex].Enqueue(price);
            this.currentSymbols.Remove(removedSecurity);
            this.currentSymbols.Add(security, removedIndex);
         }
      }

      /// <summary>
      /// Get the latest N prices in the cache for a given security.
      /// </summary>
      /// <param name="security">The security to search for.</param>
      /// <returns>An enumeration of prices, or null if the security is not found in the cache.</returns>
      public IEnumerable<decimal> LatestNPrices(string security)
      {
         return this.currentSymbols.ContainsKey(security) ?
            this.prices[this.currentSymbols[security]].AsEnumerable() :
            (IEnumerable<decimal>)null;
      }

      /// <summary>
      /// Initialize the cache with data to be discarded when actual ticks come in.
      /// </summary>
      /// <remarks>
      /// It is also possible to initialize the cache as empty and then fill it up
      /// as items come in, that would make instantiation faster and initial memory consumption lower
      /// but that would require additional checks during normal operation and different behavior
      /// when the cache is in a partially primed state. If the expectation is for a long-running
      /// cache where number of ticks is much greater number of securities cached, it is more effective
      /// overall pre-populating the cache with data.
      /// </remarks>
      private void InitializeWithDummyData()
      {
         for (int i = 0; this.numSecurities > i; i++)
         {
            this.lastN.AddLast("Dummy" + i);
         }

         for (int i = 0; this.numSecurities > i; i++)
         {
            this.currentSymbols.Add("Dummy" + i, i);
         }

         for (int i = 0; this.numSecurities > i; i++)
         {
            this.prices[i] = new Queue<decimal>(this.numPrices);
         }
      }
   }
}