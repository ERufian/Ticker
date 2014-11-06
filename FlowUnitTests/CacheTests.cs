//-----------------------------------------------------------------------
// <copyright file="CacheTests.cs" company="Eusebio Rufian-Zilbermann">
//   Copyright (c) Eusebio Rufian-Zilbermann
// </copyright>
//-----------------------------------------------------------------------
namespace FlowUnitTests
{
   using System;
   using System.Collections;
   using System.Collections.Generic;

   using Microsoft.VisualStudio.TestTools.UnitTesting;

   /// <summary>
   /// Unit tests for the Cache class.
   /// </summary>
   [TestClass]
   public class CacheTests
   {
      /// <summary>
      /// Simple case, insert one tick.
      /// </summary>
      [TestMethod]
      public void Insert1Security()
      {
         IEnumerable<decimal> latestNPrices;

         Ticker.Cache cache = new Ticker.Cache(2, 2);
         cache.Tick("IBM", 200);
         latestNPrices = cache.LatestNPrices("IBM");
         CollectionAssert.AreEqual(new List<decimal> { 200 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("FB");
         Assert.IsNull(latestNPrices);
         latestNPrices = cache.LatestNPrices("MSFT");
         Assert.IsNull(latestNPrices);
      }

      /// <summary>
      /// Insert 2 ticks, with different securities each.
      /// </summary>
      [TestMethod]
      public void Insert2DifferentSecurities()
      {
         IEnumerable<decimal> latestNPrices;

         Ticker.Cache cache = new Ticker.Cache(2, 2);
         cache.Tick("IBM", 200);
         cache.Tick("FB", 75);
         latestNPrices = cache.LatestNPrices("IBM");
         CollectionAssert.AreEqual(new List<decimal> { 200 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("FB");
         CollectionAssert.AreEqual(new List<decimal> { 75 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("MSFT");
         Assert.IsNull(latestNPrices);
      }

      /// <summary>
      /// Insert multiple ticks, with one security getting 2 prices.
      /// </summary>
      [TestMethod]
      public void Insert2PricesFor1Security()
      {
         IEnumerable<decimal> latestNPrices;

         Ticker.Cache cache = new Ticker.Cache(2, 2);
         cache.Tick("IBM", 200);
         cache.Tick("FB", 75);
         cache.Tick("IBM", 201);
         latestNPrices = cache.LatestNPrices("IBM");
         CollectionAssert.AreEqual(new List<decimal> { 200, 201 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("FB");
         CollectionAssert.AreEqual(new List<decimal> { 75 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("MSFT");
         Assert.IsNull(latestNPrices);
      }

      /// <summary>
      /// Insert multiple ticks, with two securities getting 2 prices.
      /// </summary>
      [TestMethod]
      public void Insert2PricesFor2Securities()
      {
         IEnumerable<decimal> latestNPrices;

         Ticker.Cache cache = new Ticker.Cache(2, 2);
         cache.Tick("IBM", 200);
         cache.Tick("FB", 75);
         cache.Tick("IBM", 201);
         cache.Tick("FB", 76);
         latestNPrices = cache.LatestNPrices("IBM");
         CollectionAssert.AreEqual(new List<decimal> { 200, 201 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("FB");
         CollectionAssert.AreEqual(new List<decimal> { 75, 76 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("MSFT");
         Assert.IsNull(latestNPrices);
      }

      /// <summary>
      /// Insert many ticks, enough to get one old price dropped from the cache.
      /// </summary>
      [TestMethod]
      public void DropOldPrice()
      {
         IEnumerable<decimal> latestNPrices;

         Ticker.Cache cache = new Ticker.Cache(2, 2);
         cache.Tick("IBM", 200);
         cache.Tick("FB", 75);
         cache.Tick("IBM", 201);
         cache.Tick("FB", 76);
         cache.Tick("IBM", 202);
         latestNPrices = cache.LatestNPrices("IBM");
         CollectionAssert.AreEqual(new List<decimal> { 201, 202 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("FB");
         CollectionAssert.AreEqual(new List<decimal> { 75, 76 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("MSFT");
         Assert.IsNull(latestNPrices);
      }

      /// <summary>
      /// Insert many ticks, with two consecutive ticks for the same security,
      /// to test the skip-move-to-the-end optimization.
      /// </summary>
      [TestMethod]
      public void ConsecutiveTicksForSameSecurity()
      {
         IEnumerable<decimal> latestNPrices;

         Ticker.Cache cache = new Ticker.Cache(2, 2);
         cache.Tick("IBM", 200);
         cache.Tick("FB", 75);
         cache.Tick("IBM", 201);
         cache.Tick("FB", 76);
         cache.Tick("IBM", 202);
         cache.Tick("IBM", 203);
         latestNPrices = cache.LatestNPrices("IBM");
         CollectionAssert.AreEqual(new List<decimal> { 202, 203 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("FB");
         CollectionAssert.AreEqual(new List<decimal> { 75, 76 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("MSFT");
         Assert.IsNull(latestNPrices);
      }

      /// <summary>
      /// Insert many ticks, enough to get one old security dropped from the cache.
      /// </summary>
      [TestMethod]
      public void DropOldSecurity()
      {
         IEnumerable<decimal> latestNPrices;

         Ticker.Cache cache = new Ticker.Cache(2, 2);
         cache.Tick("IBM", 200);
         cache.Tick("FB", 75);
         cache.Tick("IBM", 201);
         cache.Tick("FB", 76);
         cache.Tick("IBM", 202);
         cache.Tick("IBM", 203);
         cache.Tick("MSFT", 50);
         latestNPrices = cache.LatestNPrices("IBM");
         CollectionAssert.AreEqual(new List<decimal> { 202, 203 }, new List<decimal>(latestNPrices));
         latestNPrices = cache.LatestNPrices("FB");
         Assert.IsNull(latestNPrices);
         latestNPrices = cache.LatestNPrices("MSFT");
         CollectionAssert.AreEqual(new List<decimal> { 50 }, new List<decimal>(latestNPrices));
      }
   }
}