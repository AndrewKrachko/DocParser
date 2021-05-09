using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DocParser
{
    public class Parser
    {
        private readonly Encoding _encoding;
        private readonly ILogger<Parser> _logger;

        public Parser(Encoding encoding, ILogger<Parser> logger)
        {
            _encoding = encoding;
            _logger = logger;
        }

        public List<StoreItem> ParseStream(Stream stream)
        {
            try
            {
                var storeList = new List<StoreItem>();
                if (stream.CanRead)
                {
                    var buffer = new byte[stream.Length];
                    stream.ReadAsync(buffer, 0, (int)stream.Length);
                    ParseToUniqStoreItems(buffer, storeList);
                }
                return storeList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ParseToUniqStoreItems(byte[] buffer, List<StoreItem> tupleList)
        {
            try
            {
                var bufferString = _encoding.GetString(buffer).Split('\n');
                foreach (var substring in bufferString)
                {
                    if (TryParseToStoreItem(substring, out var storeItem))
                    {
                        var tupleListItem = tupleList.FirstOrDefault(t => t.Name.ToUpper() == storeItem.Name.ToUpper());
                        if (tupleListItem != null)
                        {
                            tupleListItem.Count += storeItem.Count;
                        }
                        else
                        {
                            tupleList.Add(storeItem);
                        }
                    }
                }
            }
            catch (Exception)
            {
                _logger.LogWarning("Неверная кодировка файла.");
            }

        }

        public bool TryParseToStoreItem(string substring, out StoreItem storeItem)
        {
            try
            {
                var substringParts = substring.Split(',');
                if (substringParts.Length > 1)
                {
                    storeItem = new StoreItem();
                    if (!string.IsNullOrWhiteSpace(substringParts[0]))
                    {
                        storeItem.Name = substringParts[0].Trim();
                    }
                    else
                    {
                        throw new Exception();
                    }

                    if (int.TryParse(substringParts[1], out var count))
                    {
                        storeItem.Count = count;
                    }
                    else
                    {
                        throw new Exception();
                    }

                    return true;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                _logger.LogWarning("Неверный формат строки файла.");
                storeItem = null;
                return false;
            }
        }
    }
}
