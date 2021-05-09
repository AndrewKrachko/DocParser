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

        [TestCase("��������,10", "��������", 10)]
        [TestCase(" ������,5", "������", 5)]
        [TestCase("����� (1 �) , 76 ", "����� (1 �)", 76)]
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
            var buffer = new UTF8Encoding().GetBytes("�����, 7\n�����, 99\n��������, 15\n�����, 22\n�����, 1\n�����,66");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreEqual(6, storeList.Count);
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "�����" && sl.Count == 7));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "�����" && sl.Count == 99));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "��������" && sl.Count == 15));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "�����" && sl.Count == 22));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "�����" && sl.Count == 1));
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "�����" && sl.Count == 66));
        }

        [Test]
        public void ParseToUniqStoreItemsDuplicateTest()
        {
            var buffer = new UTF8Encoding().GetBytes("��������, 15\n��������, 14\n��������, 16");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreEqual(1, storeList.Count);
            Assert.IsTrue(storeList.Exists(sl => sl.Name == "��������" && sl.Count == 45));
            Assert.IsFalse(storeList.Exists(sl => sl.Name == "��������"));
            Assert.IsFalse(storeList.Exists(sl => sl.Name == "��������"));
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
            var buffer = new ASCIIEncoding().GetBytes("�����, 7\n�����, 99\n��������, 15\n�����, 22\n�����, 1\n�����,66");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingUtf7Test()
        {
            var buffer = new UTF7Encoding().GetBytes("�����, 7\n�����, 99\n��������, 15\n�����, 22\n�����, 1\n�����,66");
            var storeList = new List<StoreItem>();
            var parserAscii = new Parser(new UnicodeEncoding(), new Mock<ILogger<Parser>>().SetupAllProperties().Object);
            parserAscii.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingUtf32Test()
        {
            var buffer = new UTF32Encoding().GetBytes("�����, 7\n�����, 99\n��������, 15\n�����, 22\n�����, 1\n�����,66");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingUnicodeTest()
        {
            var buffer = new UnicodeEncoding().GetBytes("�����, 7\n�����, 99\n��������, 15\n�����, 22\n�����, 1\n�����,66");
            var storeList = new List<StoreItem>();
            _parser.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
        [Test]
        public void ParseToUniqStoreItemsInvalidEncodingUtf8Test()
        {

            var buffer = new UTF8Encoding().GetBytes("�����, 7\n�����, 99\n��������, 15\n�����, 22\n�����, 1\n�����,66");
            var storeList = new List<StoreItem>();
            var parserAscii = new Parser(new UnicodeEncoding(), new Mock<ILogger<Parser>>().SetupAllProperties().Object);
            parserAscii.ParseToUniqStoreItems(buffer, storeList);

            Assert.AreNotEqual(6, storeList.Count);
        }
    }
}