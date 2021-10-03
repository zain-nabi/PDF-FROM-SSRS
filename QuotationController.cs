using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Triton.Interface.CRM;
using Triton.Interface.TritonGroup;
using Triton.Model.CRM.Tables;
using Triton.Core;
using Vendor.Services.CustomModels;
using Quotes = Vendor.Services.CustomModels.Quotes;
using QuoteLines = Vendor.Services.CustomModels.QuoteLines;

namespace Triton.BusinessOnline.Controllers
{
    [Authorize]
    public class QuotationController : Controller
    {
        private readonly IQuotes _quotes;
        private readonly IQuotesAPI _quotesApi;
        private readonly ICustomers _customers;
        private readonly ITransportTypes _transportTypes;
        private readonly IUserMap _userMapService;
        private readonly IQuotes _IquouesApi;

        public QuotationController(IQuotes quotes, ICustomers customers, ITransportTypes transportTypes,
            IQuotesAPI quotesApi, IUserMap userMapService, IQuotes Iquotes)
        {
            _quotes = quotes;
            _quotesApi = quotesApi;
            _customers = customers;
            _transportTypes = transportTypes;
            _userMapService = userMapService;
            _IquouesApi = Iquotes;
        }


        //public async Task<IActionResult> Index()
        //{
        //    return View();
        //}
        [HttpGet]
        public async Task<IActionResult> Create(long? quoteId)
        {
            try
            {
                VendorQuoteModel model;

                if (quoteId.HasValue)
                    model = await _quotes.GetQuoteByID(quoteId.Value);
                else
                {
                    model = new VendorQuoteModel
                    {
                        Quote = new Model.CRM.Tables.Quotes { ServiceTypeID = 1 }
                    };
                }

                model.TransportTypes = await _transportTypes.GetAllTransportTypes();

                var ids = new[] { "1", "2" };
                model.TransportTypes = model.TransportTypes.Where(x => x.TransportTypeID == 1 || x.TransportTypeID == 2).ToList();
                model.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
                //model.AllowedSundries = await _quotesApi.GetQuoteSurcharges();

                //model.SundryDropDownList = new List<SundryList>();
                //model.CrossBorderSundryDropDownList = new List<CrossBorderSundryList>();

                //foreach (var item in model.AllowedSundries.Where(x => x.CountryCode == "SA").Where(x => !x.Heading.Contains("SAT") || !x.Heading.Contains("EARLY")).OrderBy(x => x.Heading))
                //{
                //    model.SundryDropDownList.Add(new SundryList
                //    {
                //        Description = item.Description,
                //        Value = $"{item.OutChargeCode}|{item.ChargeAmount}",
                //        ChargeAmount = item.ChargeAmount,
                //        Heading = item.Heading,
                //        Selected = false,
                //        OutUniqueId = item.OutUniqueId
                //    });
                //}

                //foreach (var item in model.AllowedSundries.Where(x => x.CountryCode != "SA").Where(x => x.Description != "DEMURRAGE CHARGE").OrderBy(x => x.Heading))
                //{
                //    model.CrossBorderSundryDropDownList.Add(new CrossBorderSundryList
                //    {
                //        Description = item.CountryCode + " " + "-" + " " + item.Description + " " + "-" + " " + $"({item.ChargeAmount})",
                //        Value = $"{item.OutChargeCode}|{item.ChargeAmount}",
                //        ChargeAmount = item.ChargeAmount,
                //        Heading = item.Heading,
                //        Selected = false,
                //        OutUniqueId = item.OutUniqueId
                //    });
                //}

                //if (model.AllowedCustomerList.Count == 0)
                //{
                //    var cash = await _customers.GetCrmCustomerById(500);
                //    model.AllowedCustomerList.Add(cash);
                //}

                //model.TransportTypes = model.TransportTypes.Where(f => f.TransportTypeID != 6).ToList();

                //// Remove the white spaces from the telephone number
                //if (model.Quote.SenTelNo != null)
                //{
                //    model.Quote.SenTelNo = model.Quote.SenTelNo.Replace(" ", "");
                //}

                //if (model.Quote.RecTelNo != null)
                //{
                //    model.Quote.RecTelNo = model.Quote.RecTelNo.Replace(" ", "");
                //}

                return View(model);
            }
            catch
            {
                ViewData["Header"] = "404";
                ViewData["Message"] =
                    "We are experiencing a problem with the quotations.  Please contact Triton Express";
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(VendorQuoteModel model)
        {
            if (ModelState.IsValid)
            {
                //model.AllowedCustomerList = await _customers.GetCrmCustomersByRepUserId(149);
                //model.AllowedSundries = await _quotesApi.GetQuoteSurcharges();
                //model.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
                model.TransportTypes = await _transportTypes.GetAllTransportTypes();


                //Add QuoteLines to Model
                model.QuoteLines =
                    JsonConvert.DeserializeObject<List<Triton.Model.CRM.Tables.QuoteLines>>(WebUtility.UrlDecode(model.QuoteLineHF) ?? string.Empty);
                //if (!string.IsNullOrEmpty(model.SaSurchages))
                //{
                //    model.QuoteSundrys = JsonConvert.DeserializeObject<List<Triton.Model.CRM.Tables.QuoteSundrys>>(WebUtility.UrlDecode(model.SaSurchages) ??
                //                                    string.Empty);
                //}
                //if (!string.IsNullOrEmpty(model.NonSaSurchages))
                //{
                //    model.QuoteSundrys = JsonConvert.DeserializeObject<List<Triton.Model.CRM.Tables.QuoteSundrys>>(WebUtility.UrlDecode(model.NonSaSurchages) ??
                //                    string.Empty);
                //}

                model.Quote.ServiceTypeText = model.TransportTypes.Find(m => m.TransportTypeID == model.Quote.ServiceTypeID)?.Description.Trim();
                model.Quote.CreatedByTSUserID = User.GetUserId(); //GetTritonSecurityUserID();
                model.Quote.CreatedOn = DateTime.Now;
                model.Quote.CustCode = "FLO104";

                // Add on the Sundry charges
                //model.QuoteSundrys = new List<Triton.Model.CRM.Tables.QuoteSundrys>();
                // Sundry charge / sundry service
                //foreach (var item in model.SundryDropDownList)
                //{
                //    if (item.Selected)
                //    {
                //        var quoteSundryItem = new Triton.Model.CRM.Tables.QuoteSundrys
                //        {
                //            SundryService = item.Value,
                //            SundryCharge = item.ChargeAmount
                //        };

                //        model.QuoteSundrys.Add(quoteSundryItem);
                //    }
                //}

                //var response = await _quotesApi.PostQuoteUAT(model);
                var response = await _quotesApi.PostQuoteProduction(model);
                if (response.ReturnCode == "0000")
                {
                    //ViewData["Header"] = "Successfully saved";
                    //ViewData["Message"] =
                    //    $"Thank you for creating a quote.  We will provide a confirmation of this quotation.<h6>Quote No:  {response.Reference}</h6>";
                    //ViewData["Url"] = $"{Request.Path}";
                    var Quote = await _IquouesApi.GetQuoteByQuoteNumber(response.Reference);

                    //return View("~/Views/Shared/_Success.cshtml");
                    return RedirectToAction("View", "Quotation", new { Quote.Quote.QuoteID });
                }

                model.TransportTypes = await _transportTypes.GetAllTransportTypes();

                // Failed to create the quote
                ViewData["Header"] = "Oops";
                ViewData["Message"] = $"{response.ReturnCode} - {response.ReturnMessage}";

                return View("~/Views/Shared/Error.cshtml");
            }

            model.TransportTypes = await _transportTypes.GetAllTransportTypes();
            //model.AllowedCustomerList = await _customers.GetCrmCustomersByRepUserId(250);
            //model.AllowedSundries = await _quotesApi.GetQuoteSurcharges();
            //model.SundryDropDownList = new List<SundryList>();

            //foreach (var item in model.AllowedSundries)
            //{
            //    model.SundryDropDownList.Add(new SundryList
            //    {
            //        Description = item.Description,
            //        Value = item.OutChargeCode,
            //        Selected = false
            //    });
            //}

            ModelState.AddModelError("Error", ModelState.Keys.SelectMany(key => ModelState[key].Errors).FirstOrDefault()?.ErrorMessage);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> View(long quoteId)
        {
            var model = await _quotes.GetQuoteByID(quoteId);
            //TransportTypes = await _transportTypes.GetAllTransportTypes(),
            model.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
            //AllowedSundries = await _quotesApi.GetQuoteSurcharges()
            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new VendorQuoteSearchModel
            {
                AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(VendorQuoteSearchModel model)
        {

            var dateSplit = model.FilterDate.Split("-");

            model.DateFrom = Convert.ToDateTime(dateSplit[0].Trim());
            model.DateTo = Convert.ToDateTime(dateSplit[1].Trim());

            var query = await _quotes.GetQuotesbyCustomerIdOptRefandDates(model.CustomerID, model.QuoteRef,
                model.DateFrom, model.DateTo);
            query.AllowedCustomerList = (await _userMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
            query.ShowReport = true;
            return View(query);
        }

        public async Task<ActionResult> QuotePDF(int quoteId)
        {
            var x = await _quotesApi.GetQuoteDocument(quoteId);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = $"Quote.pdf",
                Inline = false,
            };
            return File(x.ImgData, "application/pdf");
        }

        public async Task<ActionResult> EmailQuote(int quoteId, string email)
        {
            try
            {
                await _quotesApi.EmailQuoteDocument(quoteId, email, User.GetUserId());
            }
            catch
            {
                // ignored
            }

            return RedirectToAction("View", "Quotation", new { quoteId });
        }

    }


}