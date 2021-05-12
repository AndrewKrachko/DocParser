using DocParser;
using DocParserWeb.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StringGenerator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DocParserWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<ThreadingConfig> _threadingConfig;
        private readonly char[] _charSet;

        public HomeController(IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger, IOptions<ThreadingConfig> threadingConfig)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _threadingConfig = threadingConfig;
            _charSet = StringGenerator.StringGenerator.GetCharArrayAsciiNumbersAndLettersRange();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(StoreDataSourceModel model)
        {
            var folder = StringGenerator.StringGenerator.GenerateString(_charSet, 32);
            switch (model.input_mode)
            {
                case string _ when model.input_mode == model.input_modes[0]:
                    ParallelDownloadFiles(Request.Form.Files, folder);
                    break;
                case string _ when model.input_mode == model.input_modes[1]:
                    if (model.input_address != null)
                    {
                        LoadFilesFromUrls(model.input_address.Split('\n'), folder);
                    }
                    break;
            }

            if (Directory.Exists(Path.Combine(_webHostEnvironment.ContentRootPath, folder)))
            {
                var fileNames = Directory.GetFiles(Path.Combine(_webHostEnvironment.ContentRootPath, folder));
                model.storeItems = GetStoreItemsFromFiles(fileNames);
                Thread.Sleep(1000);
                Directory.Delete(folder, true);
            }

            return View(model);
        }

        private List<StoreItem> GetStoreItemsFromFiles(string[] files)
        {
            var result = new List<StoreItem>();
            Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = _threadingConfig.Value.ThreadLimits }, async file =>
            {
                using (var fileStream = new FileStream(file, FileMode.Open))
                {
                    var parser = new Parser(new UTF8Encoding(), new Logger<Parser>(new LoggerFactory()));
                    foreach (var item in await parser.ParseStreamAsync(fileStream))
                    {
                        Parser.AddStoreItemToList(result, item);
                    }
                }
            });

            return result;
        }

        private void LoadFilesFromUrls(string[] urls, string folder)
        {
            Parallel.ForEach(urls, new ParallelOptions() { MaxDegreeOfParallelism = _threadingConfig.Value.ThreadLimits }, async url =>
            {
                try
                {
                    var newRequest = HttpWebRequest.Create(url);
                    var webResponse = newRequest.GetResponse();
                    if (webResponse.ContentLength > 0)
                    {
                        var stream = webResponse.GetResponseStream();
                        var resp = HttpContext.Response;
                        resp.ContentType = "application/octet-stream";
                        await DownloadFileToServerAsync(stream, folder);
                        stream.Close();
                    }
                }
                catch (UriFormatException)
                {
                    _logger.LogError("Некорректный формат uri.");
                }
                catch (WebException)
                {
                    _logger.LogError("Несуществующее имя входного файла для режима http.");
                }
                catch (Exception)
                {
                    _logger.LogError("Ошибка загрузки файла по http.");
                }
            });
        }

        private void ParallelDownloadFiles(IFormFileCollection requestFiles, string folder)
        {
            if (requestFiles.Count > 0)
            {
                Parallel.ForEach(requestFiles, async file =>
                {
                    if (file.Length > 0)
                    {
                        await DownloadFileToServerAsync(file.OpenReadStream(), folder);
                    }
                });
            }
        }

        private async Task DownloadFileToServerAsync(Stream file, string folder)
        {
            var folderPath = Path.Combine(_webHostEnvironment.ContentRootPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, folder, StringGenerator.StringGenerator.GenerateString(_charSet, 32));
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            file.Close();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
