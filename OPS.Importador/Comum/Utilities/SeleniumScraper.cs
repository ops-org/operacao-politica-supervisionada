using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace OPS.Importador.Comum.Utilities
{
    public class SeleniumScraper
    {
        public readonly ILogger<SeleniumScraper> logger;
        protected readonly AppSettings appSettings;

        public SeleniumScraper(IServiceProvider serviceProvider)
        {
            appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
            logger = serviceProvider.GetRequiredService<ILogger<SeleniumScraper>>();
        }

        public void BaixarArquivosParana()
        {
            // Opens the public consulta page, waits for a human to solve CAPTCHA if present,
            // then clicks the "Consultar" button. Browser is shown (not headless) so user can solve captcha.
            string url = "https://consultas.assembleia.pr.leg.br/#/ressarcimento";

            ChromeOptions options = new ChromeOptions();
            // enable performance logs
            options.SetLoggingPreference(OpenQA.Selenium.LogType.Performance, OpenQA.Selenium.LogLevel.All);
            // Keep the browser visible so the CAPTCHA can be solved manually
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var driver = new ChromeDriver(service, options);
            // enable Network domain so requestIds and bodies are available
            try
            {
                driver.ExecuteCdpCommand("Network.enable", new Dictionary<string, object>());
            }
            catch
            {
                // handle older chromeDriver versions or log
            }

            try
            {
                driver.Navigate().GoToUrl(url);
                logger.LogInformation("Open page: {PageUrl}", url);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                WebDriverWait waitCaptcha = new WebDriverWait(driver, TimeSpan.FromMinutes(10));

                WaitUntil(wait, By.CssSelector("iframe[src*='recaptcha'], iframe[src*='grecaptcha']"));
                logger.LogInformation("Chaptcha is visible");

                CheckCaptchaAndWaitSolve(driver, waitCaptcha);

                var today = DateTime.Today;
                var dateToImport = today.AddMonths(-4);
                if (appSettings.ForceImport) dateToImport = new DateTime(2023, 1, 1);

                while (dateToImport < today)
                {
                    if (dateToImport.Year == DateTime.Today.Year && dateToImport.Month > DateTime.Today.Month)
                    {
                        // skip future months
                        break;
                    }

                    SetSelectOptionByText(driver, "mes", ResolveMes(dateToImport.Month));
                    driver.FindElement(OpenQA.Selenium.By.Id("exercicio")).SendKeys(dateToImport.Year.ToString());

                    do
                    {
                        // Click Consultar
                        OpenQA.Selenium.IWebElement consultarButton = driver.FindElement(OpenQA.Selenium.By.XPath("//button[contains(normalize-space(.), 'Consultar')]"));
                        consultarButton.Click();
                        logger.LogInformation("Consultando dados de {Mes}/{Ano}", dateToImport.Month, dateToImport.Year);

                        try
                        {
                            WaitUntil(wait, By.CssSelector(".card-body>.ng-star-inserted, .recaptcha, .alert-danger, .alert-info"));
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Erro ao aguardar resultados");
                        }

                        if (ExistsElement(driver, By.CssSelector(".alert-info"))) // Nenhum ressarcimento encontrado
                        {
                            return;
                        }
                    } while (CheckCaptchaAndWaitSolve(driver, waitCaptcha));

                    var tableRows = driver.FindElements(OpenQA.Selenium.By.CssSelector(".card-body>.ng-star-inserted tbody tr"));
                    logger.LogInformation("Interagir em {Count} Resistros", tableRows.Count);

                    foreach (var row in tableRows)
                    {
                        //var parlamentar = row.FindElement(OpenQA.Selenium.By.XPath("//td[2]")).Text;
                        //var outPath = Path.Combine("C:\\temp\\Parana", $"{ano}_{mes}_{parlamentar}_tmp.json");
                        //if (File.Exists(outPath)) continue;

                        do
                        {
                            row.FindElement(OpenQA.Selenium.By.CssSelector("button")).Click();

                            logger.LogInformation("Visualizar despesas");

                            try
                            {
                                WaitUntil(wait, By.CssSelector(".modal h5, .recaptcha, .alert-danger"));
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning(ex, "Erro ao aguardar resultados");
                            }


                        } while (CheckCaptchaAndWaitSolve(driver, waitCaptcha));

                        var parlamentar = driver.FindElement(OpenQA.Selenium.By.CssSelector(".modal h5")).Text;

                        // Capture CDP network events (exact XHR details) and persist them.
                        try
                        {
                            var captured = CaptureXhrNetworkEntries(driver, filterUrl: "/api/public/ressarcimento/despesas-ressarcimento/");
                            if (captured != null && captured.Count > 0)
                            {
                                var outPath = Path.Combine(appSettings.TempFolder, "Parana", $"{dateToImport.Year}_{dateToImport.Month}_{parlamentar}_tmp.json");
                                logger.LogInformation("Captured {Count} matching XHR entries for parlamentar {Parlamentar}", captured.Count, parlamentar);

                                //File.WriteAllText(outPath, JsonSerializer.Serialize(captured, new JsonSerializerOptions { WriteIndented = true }));
                                // Caso haja capcha, o último contem o json valido.
                                File.WriteAllText(outPath, captured.LastOrDefault().Body);
                                logger.LogInformation("Saved captured XHR entries to {Path}", outPath);
                            }
                            else
                            {
                                logger.LogInformation("No matching XHR entries captured.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to capture XHR network entries");
                        }

                        // Short grace wait to allow any JS to complete
                        // Wait for results to appear (common selectors: table, .table, .results). Adjust if site changes.
                        WaitUntil(wait, By.CssSelector("button.close"));
                        driver.FindElement(OpenQA.Selenium.By.CssSelector("button.close")).Click();
                    }

                    dateToImport = dateToImport.AddMonths(1);
                }
            }
            finally
            {
                // Keep a small delay before closing so a developer can inspect if needed (optional).
                // driver.Quit() will close the browser; comment out the next line to keep the browser open for debugging.
                driver.Quit();
            }
        }

        public void BaixarArquivosPiaui()
        {
            var url = "https://transparencia.al.pi.leg.br/grid_transp_publico_gecop/";
            ChromeOptions options = new ChromeOptions();
            // Keep the browser visible so the CAPTCHA can be solved manually
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            var downloadDir = Path.Combine(appSettings.TempFolder, "Estados", Core.Enumerators.Estados.Piaui.ToString());
            options.AddUserProfilePreference("download.default_directory", downloadDir);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("disable-popup-blocking", "true");
            //options.AddUserProfilePreference("plugins.always_open_pdf_externally", true); // For PDFs

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var driver = new ChromeDriver(service, options);
            // enable Network domain so requestIds and bodies are available

            driver.Navigate().GoToUrl(url);
            logger.LogInformation("Open page: {PageUrl}", url);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            wait.Until(d => d.FindElement(By.CssSelector("#id_div_interativ_search i"))).Click();

            wait.Until(d => d.FindElement(By.XPath("//div[@id=\"id_tab_mv_legis_link\"]//a[normalize-space(text())=\"20a Legislatura (2023 a 2027)\"]"))).Click();

            wait.Until(d =>
            {
                try
                {
                    var results = d.FindElements(By.CssSelector("#id_tab_mv_legis_link .simplemv_legis"));
                    return results?.Count() == 1;
                }
                catch
                {
                    return false;
                }
            });

            // Short grace wait to allow any JS to complete
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));

            wait.Until(d => d.FindElement(By.CssSelector("#csv_top"))).Click();

            // Method 3: Switch by WebElement (most reliable)
            IWebElement iframe = wait.Until(d => d.FindElement(By.Id("TB_iframeContent")));
            driver.SwitchTo().Frame(iframe);

            wait.Until(d => d.FindElement(By.Id("bok"))).Click();

            // Short grace wait to allow any JS to complete
            //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));

            wait.Until(d =>
            {
                try
                {
                    // Não remover essa linha.
                    Console.WriteLine($"Page: {d.PageSource}");

                    var element = d.FindElement(By.CssSelector("span[id^=\"pb_msg-\"]"));
                    logger.LogInformation("Mensagem: {Msg}", element?.Text);

                    return element?.Text.Contains("Arquivo gerado com sucesso") ?? false;
                }
                catch
                {
                    return false;
                }
            });

            // grid_transp_publico_gecop.csv -> CLPI-despesas-legislatura_tmp.csv
            DownloadWithCustomName(driver, downloadDir, "CLPI-despesas-legislatura_tmp.csv", By.Id("idBtnDown"), 60);
        }

        public bool CheckCaptchaAndWaitSolve(ChromeDriver driver, WebDriverWait wait)
        {
            // Check if a captcha solving is required by looking for captcha elements.
            // This method can be adjusted based on the actual captcha implementation on the target site.
            var captchaExists = ExistsElement(driver, By.CssSelector(".recaptcha, .alert-danger"));

            if (captchaExists)
            {
                logger.LogInformation("Wait to solve Chaptcha");

                // Wait until captcha is solved: either the Consultar button becomes enabled OR grecaptcha badge disappears.
                bool captchaSolved = wait.Until(d =>
                {
                    try
                    {
                        // Some sites show iframe-based captcha; if grecaptcha iframe not present consider solved
                        var frames = d.FindElements(OpenQA.Selenium.By.CssSelector(".recaptcha, .alert-danger"));
                        return !frames.Any(x => x.Displayed);
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (!captchaSolved)
                    throw new Exception("CAPTCHA não foi resolvido dentro do tempo limite.");

                logger.LogInformation("Chaptcha was solved");
                return true; // indicate captcha was present and is now solved
            }

            logger.LogInformation("Chaptcha não localizado");
            return false;
        }

        private static bool ExistsElement(ChromeDriver driver, By by)
        {
            try
            {
                var result = driver.FindElement(by);
                return result?.Displayed ?? false;
            }
            catch
            {
                return false;
            }
        }

        private static void WaitUntil(WebDriverWait wait, By by)
        {
            wait.Until(d =>
            {
                try
                {
                    var results = d.FindElements(by);
                    return results?.Any(x => x.Displayed) ?? false;
                }
                catch
                {
                    return false;
                }
            });
        }

        private string ResolveMes(int mes) => mes switch
        {
            1 => "Janeiro",
            2 => "Fevereiro",
            3 => "Março",
            4 => "Abril",
            5 => "Maio",
            6 => "Junho",
            7 => "Julho",
            8 => "Agosto",
            9 => "Setembro",
            10 => "Outubro",
            11 => "Novembro",
            12 => "Dezembro",
            _ => throw new NotImplementedException(),
        };

        private void DownloadWithCustomName(IWebDriver driver, string downloadPath, string customFileName, By downloadButton, int waitSeconds = 30)
        {
            var tempFile = "";
            var fileSystemWatcher = new FileSystemWatcher(downloadPath);
            var fileCreatedEvent = new ManualResetEvent(false);

            fileSystemWatcher.Renamed += (s, e) =>
            {
                if (!e.FullPath.EndsWith(".crdownload", StringComparison.InvariantCultureIgnoreCase))
                {
                    tempFile = e.FullPath;
                    fileCreatedEvent.Set();
                }
            };

            fileSystemWatcher.EnableRaisingEvents = true;

            driver.FindElement(downloadButton).Click();

            if (fileCreatedEvent.WaitOne(TimeSpan.FromSeconds(waitSeconds)))
            {
                System.Threading.Thread.Sleep(500); // Wait for file to finish writing
                var customPath = Path.Combine(downloadPath, customFileName);
                File.Move(tempFile, customPath, true);
                fileCreatedEvent.Reset();
            }

            fileSystemWatcher.Dispose();
        }

        private List<CapturedNetworkEntry> CaptureXhrNetworkEntries(IWebDriver driver, string filterUrl)
        {
            var chromeDriver = (ChromeDriver)driver;

            // Collect performance logs
            var logs = driver.Manage().Logs.GetLog("performance");
            var entries = new List<CapturedNetworkEntry>();

            foreach (var log in logs)
            {
                try
                {
                    using (var doc = JsonDocument.Parse(log.Message))
                    {
                        if (!doc.RootElement.TryGetProperty("message", out var message)) continue;
                        if (!message.TryGetProperty("method", out var methodElement)) continue;
                        var method = methodElement.GetString();

                        if (string.Equals(method, "Network.responseReceived", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var @params = message.GetProperty("params");
                            var response = @params.GetProperty("response");
                            var url = response.GetProperty("url").GetString();

                            if (string.IsNullOrEmpty(url) || !url.Contains(filterUrl, StringComparison.InvariantCultureIgnoreCase)) continue;

                            var requestId = @params.GetProperty("requestId").GetString();
                            var status = response.TryGetProperty("status", out var statusProp) ? (int?)statusProp.GetInt32() : null;
                            Dictionary<string, object> respHeaders = null;
                            if (response.TryGetProperty("headers", out var headersProp) && headersProp.ValueKind == JsonValueKind.Object)
                            {
                                respHeaders = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                                foreach (var p in headersProp.EnumerateObject())
                                {
                                    respHeaders[p.Name] = p.Value.ToString();
                                }
                            }

                            // Attempt to retrieve request headers from earlier requestWillBeSent event if available in logs
                            Dictionary<string, object> reqHeaders = null;
                            var requestMethod = @params.TryGetProperty("request", out var reqProp) && reqProp.TryGetProperty("method", out var mProp) ? mProp.GetString() : null;
                            if (@params.TryGetProperty("request", out var requestProp) && requestProp.ValueKind == JsonValueKind.Object)
                            {
                                if (requestProp.TryGetProperty("headers", out var rh) && rh.ValueKind == JsonValueKind.Object)
                                {
                                    reqHeaders = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                                    foreach (var p in rh.EnumerateObject())
                                    {
                                        reqHeaders[p.Name] = p.Value.ToString();
                                    }
                                }
                            }

                            // Compose entry
                            var entry = new CapturedNetworkEntry()
                            {
                                RequestId = requestId,
                                Url = url,
                                Method = requestMethod,
                                Status = status,
                                ResponseHeaders = respHeaders,
                                RequestHeaders = reqHeaders,
                                Timing = null,
                                Body = null
                            };

                            // Get response body via CDP
                            try
                            {
                                var result = chromeDriver.ExecuteCdpCommand("Network.getResponseBody", new Dictionary<string, object> { { "requestId", requestId } });
                                if (result is Dictionary<string, object> dict && dict.TryGetValue("body", out var bodyObj))
                                {
                                    entry.Body = bodyObj?.ToString();
                                }
                                else if (result is IDictionary<string, object> idict && idict.ContainsKey("body"))
                                {
                                    entry.Body = idict["body"]?.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                // Some responses may not be available or the requestId may be invalid; log and continue
                                logger.LogDebug(ex, "Network.getResponseBody failed for requestId {RequestId}", requestId);
                            }

                            entries.Add(entry);
                        }
                    }
                }
                catch
                {
                    // ignore parse errors
                }
            }

            return entries;
        }

        private void SetSelectOptionByValue(OpenQA.Selenium.IWebDriver driver, string selectId, string value)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            OpenQA.Selenium.IWebElement selectElement = wait.Until(d => d.FindElement(OpenQA.Selenium.By.Id(selectId)));
            SelectElement select = new SelectElement(selectElement);
            select.SelectByValue(value);
            OpenQA.Selenium.IJavaScriptExecutor js = (OpenQA.Selenium.IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", selectElement);
        }

        private void SetSelectOptionByText(OpenQA.Selenium.IWebDriver driver, string selectId, string visibleText)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            OpenQA.Selenium.IWebElement selectElement = wait.Until(d => d.FindElement(OpenQA.Selenium.By.Id(selectId)));
            SelectElement select = new SelectElement(selectElement);
            select.SelectByText(visibleText);
            OpenQA.Selenium.IJavaScriptExecutor js = (OpenQA.Selenium.IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", selectElement);
        }

        private void SetSelectOptionByIndex(OpenQA.Selenium.IWebDriver driver, string selectId, int index)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            OpenQA.Selenium.IWebElement selectElement = wait.Until(d => d.FindElement(OpenQA.Selenium.By.Id(selectId)));
            SelectElement select = new SelectElement(selectElement);
            select.SelectByIndex(index);
            OpenQA.Selenium.IJavaScriptExecutor js = (OpenQA.Selenium.IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", selectElement);
        }

        private class CapturedNetworkEntry
        {
            public string RequestId { get; set; }
            public string Url { get; set; }
            public string Method { get; set; }
            public int? Status { get; set; }
            public Dictionary<string, object> ResponseHeaders { get; set; }
            public Dictionary<string, object> RequestHeaders { get; set; }
            public double? Timing { get; set; }
            public string Body { get; set; }
        }
    }
}
