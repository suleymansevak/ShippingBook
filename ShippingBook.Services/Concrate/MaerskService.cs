using System;
using System.Globalization;
using System.Text.Json;
using Microsoft.Playwright;
using ShippingBook.Core.Entities;
using ShippingBook.Services.Abstract;
using static ShippingBook.Core.Entities.MaerskResponse;

namespace ShippingBook.Services.Concrate
{
    public class MaerskService : IMaerskService
    {
        public async Task<BaseResponse<MaerskResponse>> GetTable(MaerskRequest request)
        {
            try
            {
                // Playwright'i başlat
                using var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false,
                    Args = new[] { "--disable-blink-features=AutomationControlled" }
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"
                });


                var cookiesFile = Path.Combine(Directory.GetCurrentDirectory(), "cookies.json");

                await LoadOrLogin(context, cookiesFile);


                var page = await context.NewPageAsync();


                await Task.Delay(3000);
                // Giriş yaptıktan sonra hedef sayfaya git
                await page.GotoAsync("https://www.maersk.com/book/");

                await Task.Delay(3000);
                var locator = page.Locator("h1.mds-headline--large");

                if (await locator.CountAsync() > 0)
                {
                    var text = await locator.First.InnerTextAsync();
                    if (text.Trim() == "Login")
                    {
                        await LoginAndSaveCookies(context, cookiesFile);

                        await Task.Delay(3000);
                        await page.GotoAsync("https://www.maersk.com/book/");
                    }
                }


                await Task.Delay(3000);
                if (await page.Locator("#login-submit-button").IsVisibleAsync())
                {
                    await page.ClickAsync("#login-submit-button");
                }

                await Task.Delay(8000);
                var declineButton = await page.Locator(".coi-banner__decline").AllAsync();
                if (await declineButton[0].CountAsync() > 0 && await declineButton[0].IsVisibleAsync())
                {


                    // Aynı class a ait butonları bul ilkine tıkla 
                    var buttons = await page.Locator(".coi-banner__decline").AllAsync();
                    foreach (var button in buttons)
                    {
                        if (await button.IsVisibleAsync()) // Görünür olanı seç
                        {
                            await button.ClickAsync();
                            break;
                        }
                    }
                }


                await page.WaitForTimeoutAsync(1000);
                await page.FillAsync("#mc-input-origin", request.From);
                await page.WaitForTimeoutAsync(1000);
                await page.WaitForSelectorAsync($"text={request.From}");
                await page.WaitForTimeoutAsync(1000);
                await page.Locator($"text={request.From}").ClickAsync(new() { Force = true });


                await page.FillAsync("#mc-input-destination", request.To);
                await page.WaitForTimeoutAsync(1000);
                await page.WaitForSelectorAsync($"text={request.To}");
                await page.WaitForTimeoutAsync(1000);
                await page.Locator($"text={request.To}").ClickAsync(new() { Force = true });


                // Commodity ---------------------------------------------------------------------
                await page.WaitForTimeoutAsync(1000);
                //await page.EvaluateAsync("window.scrollBy(0, 100);"); // 300 px yukarı
                //await page.WaitForTimeoutAsync(1000);

                var dropdown = page.GetByPlaceholder("Type in minimum 2 characters");
                // İlk tıklama
                await dropdown.ClickAsync();
                await page.WaitForTimeoutAsync(500);

                await page.GetByPlaceholder("Type in minimum 2 characters").ClickAsync(); // Boş arama yaparak 
                await page.WaitForTimeoutAsync(1000);
                // Yukarı kaydırmak için sayfayı scroll et


                var element = page.Locator($"text='a'").First;

                await page.WaitForTimeoutAsync(500);


                await element.ClickAsync(new() { Force = true }); // Tıklama işlemi

                // Commodity -------------------------------------------------------------------------


                var TypeAndSize = await page.GetByPlaceholder("Select container type and size").AllAsync();
                if (TypeAndSize.Count > 1)
                    TypeAndSize[1].FillAsync(request.ContainerType);
                else
                    TypeAndSize[0].FillAsync(request.ContainerType);


                await page.WaitForTimeoutAsync(1000);
                await page.WaitForSelectorAsync($"text={request.ContainerType}");
                await page.Locator($"text={request.ContainerType}").First.ClickAsync(new() { Force = true });



                await page.WaitForTimeoutAsync(1000);
                var NumOfCont = await page.GetByPlaceholder("Select number of containers").AllAsync();
                if (NumOfCont.Count > 0)
                {
                    NumOfCont[1].FillAsync(request.ContainerCount);
                }
                else
                {
                    NumOfCont?.First().FillAsync(request.ContainerCount);
                }




                await page.WaitForTimeoutAsync(2000);
                await page.FillAsync("#mc-input-weight", request.CargoWeight); // Kargo ağırlığı

                await page.WaitForTimeoutAsync(1000);
                await page.Locator("#priceOwner").ClickAsync();


                await page.WaitForTimeoutAsync(2000);
                var DateOfBook = await page.Locator("#earliestDepartureDatePicker").AllAsync();

                await DateOfBook[1].ClickAsync();

                if (request.CargoDate.Month.ToString() == DateTime.Now.Month.ToString())
                {
                    await page.ClickAsync($"text='{request.CargoDate.Day}'");
                }
                else
                {
                    var nextBtnClickCount = request.CargoDate.Month - DateTime.Now.Month;
                    for (int i = 0; i < nextBtnClickCount; i++)
                    {
                        await page.WaitForSelectorAsync("#nextButton", new()
                        {
                            State = WaitForSelectorState.Attached,
                            Timeout = 15000
                        });
                        var nextButton = page.Locator("#nextButton");
                        await nextButton.ClickAsync(new() { Timeout = 3000 });
                        await page.WaitForTimeoutAsync(500); // Takvimin güncellenmesini bekle
                        await page.ClickAsync($"text='{request.CargoDate.Day}'");
                    }

                    //var dateLocator2 = page.Locator("td:not(.disabled) >> text='16'");

                    //if (await dateLocator2.IsVisibleAsync())
                    //{
                    //    await dateLocator2.ClickAsync();
                    //}
                    //else
                    //{
                    //    throw new Exception("Seçilen tarih pasif veya bulunamadı!");
                    //}
                }


                await page.WaitForTimeoutAsync(1000);
                // Booking  butonuna tıkla
                await page.ClickAsync("#od3cpContinueButton");

                await page.WaitForTimeoutAsync(1000);
                await page.Locator("[data-cy='cancelButton']").ClickAsync();


                await page.WaitForTimeoutAsync(1000);
                await page.WaitForSelectorAsync("article:has-text('Book')");
                var article = await page.Locator("article:has-text('Book')").AllAsync();
                await page.WaitForTimeoutAsync(1000);
                if (article.Count > 0)
                {
                    await article[0].Locator("span.hyperlink-button:has-text('Price breakdown & details')").ClickAsync();
                }


                await page.WaitForTimeoutAsync(1000);
                await page.Locator("table").WaitForAsync(new() { Timeout = 60000 });

                var tableHtml = await article[0].Locator("table").InnerHTMLAsync();

                var tableRows = await page.Locator("table tbody tr").AllAsync();


                //SaveHtmlTableToExcel(tableHtml, "");

                var result = await MappingTableToModel(tableRows, tableHtml);



                Console.WriteLine("İşlem tamamlandı!");
                await browser.CloseAsync();

                return new BaseResponse<MaerskResponse>("ok", result);


            }
            catch (Exception ex)
            {
                return new BaseResponse<MaerskResponse>(ex.Message, new MaerskResponse());
            }
        }





        private async Task<MaerskResponse> MappingTableToModel(IReadOnlyList<ILocator> tableRow, string tableHtml)
        {
            var freightChargeTable = new MaerskResponse();

            foreach (var row in tableRow)
            {
                var cells = await row.Locator("td div").AllTextContentsAsync();
                if (cells.Count >= 6)
                {
                    freightChargeTable.Charges.Add(new FreightCharge
                    {
                        ChargeType = cells[0].Trim(),
                        Basis = cells[1].Trim(),
                        Quantity = int.TryParse(cells[2], out int qty) ? qty : 0,
                        Currency = cells[3].Trim(),
                        UnitPrice = decimal.TryParse(cells[4], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal unitPrice) ? unitPrice : 0,
                        TotalPrice = decimal.TryParse(cells[5], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalPrice) ? totalPrice : 0
                    });
                }
            }

            return freightChargeTable;
        }



        private async Task LoadOrLogin(IBrowserContext context, string cookiesFile)
        {
            if (File.Exists(cookiesFile))
            {
                var json = await File.ReadAllTextAsync(cookiesFile);
                var cookies = JsonSerializer.Deserialize<List<Cookie>>(json);

                if (cookies != null && cookies.Any(c => c.Expires > DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
                {
                    await context.AddCookiesAsync(cookies);
                }
                else
                {
                    Console.WriteLine("Cookies süresi dolmuş, tekrar giriş yapılıyor...");
                    await LoginAndSaveCookies(context, cookiesFile);
                }
            }
            else
            {
                Console.WriteLine("Cookie dosyası bulunamadı, giriş yapılıyor...");
                await LoginAndSaveCookies(context, cookiesFile);
            }
        }


        private async Task LoginAndSaveCookies(IBrowserContext context, string cookiesFile)
        {
            IPage page;
            var hasOpenPage = context.Pages;
            if (hasOpenPage.Count == 0)
            {
                page = await context.NewPageAsync();
                await page.GotoAsync("https://accounts.maersk.com/ocean-maeu/auth/login");
                // Çerezleri temizle
                await page.EvaluateAsync("window.localStorage.clear();");
            }
            else
            {
                page = hasOpenPage[0];
            }




            // 3 saniye bekle
            await Task.Delay(3000);
            // Cookie banner kapat
            var declineButtons = await page.Locator(".coi-banner__decline").AllAsync();
            foreach (var button in declineButtons)
            {
                if (await button.IsVisibleAsync())
                {
                    await button.ClickAsync();
                    break;
                }
            }


            await page.EvaluateAsync(@"
            element => {
                element.value = 'trukkermer';
                element.dispatchEvent(new Event('input', { bubbles: true }));
                element.dispatchEvent(new Event('change', { bubbles: true }));
            }
        ", await page.QuerySelectorAsync("#username"));

            await page.EvaluateAsync(@"
            element => {
                element.value = 'KnT@121trmer';
                element.dispatchEvent(new Event('input', { bubbles: true }));
                element.dispatchEvent(new Event('change', { bubbles: true }));
            }
        ", await page.QuerySelectorAsync("#password"));


            // Giriş yap butonuna tıkla
            await page.ClickAsync("#login-submit-button");

            await Task.Delay(3000);

            // Giriş sonrası çerezleri al ve kaydet
            var cookies = await context.CookiesAsync();
            var json = JsonSerializer.Serialize(cookies, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(cookiesFile, json);
        }

    }
}

