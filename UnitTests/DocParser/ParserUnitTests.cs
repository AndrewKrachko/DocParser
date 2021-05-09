using DocParser;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
    public class Tests
    {
        Parser _parser;

        [SetUp]
        public void Setup()
        {
            var loggerMoq = new Mock<ILogger<Parser>>();
            loggerMoq.SetupAllProperties();
            _parser = new Parser(new UTF8Encoding(), loggerMoq.Object);
        }

        [TestCase("Отвертка,10", "Отвертка", 10)]
        [TestCase(" Гвоздь,5", "Гвоздь", 5)]
        [TestCase("Уксус (1 л) , 76 ", "Уксус (1 л)", 76)]
        public void TryParseToTupleValidTest(string input, string name, int count)
        {
            var result = _parser.TryParseToStoreItem(input, out var storeItem);

            Assert.IsTrue(result);
            Assert.AreEqual(name, storeItem.Name);
            Assert.AreEqual(count, storeItem.Count);
        }

        [TestCase("")]
        [TestCase("Abc")]
        [TestCase("Abc, ")]
        [TestCase("Abc, a")]
        [TestCase("Abc,, 11")]
        public void TryParseToTupleInValidTest(string input)
        {
            var result = _parser.TryParseToStoreItem(input, out var storeItem);

            Assert.IsFalse(result);
            Assert.IsNull(storeItem);
        }

        [Test]
        public void ParseToUniqStoreItemsValidTest()
        {
            var buffer = new UTF8Encoding().GetBytes("Болты, 7\nВинты, 99\nЗаклепки, 15\nДырки, 22\nБулки, 1\nВилки,66");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreEqual(6, storeList.Count);
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "Болты" && sl.Count == 7));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "Винты" && sl.Count == 99));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "Заклепки" && sl.Count == 15));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "Дырки" && sl.Count == 22));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "Булки" && sl.Count == 1));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "Вилки" && sl.Count == 66));
        }

        [Test]
        public void ParseToUniqStoreItemsDuplicateTest()
        {
            var buffer = new UTF8Encoding().GetBytes("Заклепки, 15\nзаклепки, 14\nЗАКЛЕПКИ, 16");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreEqual(1, storeList.Count);
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "Заклепки" && sl.Count == 45));
            Assert.IsFalse(storeList.Exists(sl => sl.Name == "заклепки"));
            Assert.IsFalse(storeList.Exists(sl => sl.Name == "ЗАКЛЕПКИ"));
        }

        [Test]
        public void ParseToUniqStoreItemsInvalidStringsTest()
        {
            var buffer = new UTF8Encoding().GetBytes("\n\n,\n   ,11\n;");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreEqual(0, storeList.Count);
        }


        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingAsciiTest()
        {
            var buffer = new ASCIIEncoding().GetBytes("Болты, 7\nВинты, 99\nЗаклепки, 15\nДырки, 22\nБулки, 1\nВилки,66");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingUtf7Test()
        {
            var buffer = new UTF7Encoding().GetBytes("Болты, 7\nВинты, 99\nЗаклепки, 15\nДырки, 22\nБулки, 1\nВилки,66");
            var storeList = new List<StoreItem>();
            var parserAscii = new Parser(new UnicodeEncoding(), new Mock<ILogger<Parser>>().SetupAllProperties().Object);
            parserAscii.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingUtf32Test()
        {
            var buffer = new UTF32Encoding().GetBytes("Болты, 7\nВинты, 99\nЗаклепки, 15\nДырки, 22\nБулки, 1\nВилки,66");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingUnicodeTest()
        {
            var buffer = new UnicodeEncoding().GetBytes("Болты, 7\nВинты, 99\nЗаклепки, 15\nДырки, 22\nБулки, 1\nВилки,66");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingUtf8Test()
        {

            var buffer = new UTF8Encoding().GetBytes("Болты, 7\nВинты, 99\nЗаклепки, 15\nДырки, 22\nБулки, 1\nВилки,66");
            var storeList = new List<StoreItem>();
            var parserAscii = new Parser(new UnicodeEncoding(), new Mock<ILogger<Parser>>().SetupAllProperties().Object);
            parserAscii.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
    }
}