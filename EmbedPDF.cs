        [HttpGet]
        public async Task<IActionResult> View(long quoteId)
        {
            var quotationViewModel = new QuotationViewModel();
            quotationViewModel.VendorQuoteModel = await QuoteService.GetQuoteByID(quoteId);
            //TransportTypes = await _transportTypes.GetAllTransportTypes(),
            quotationViewModel.VendorQuoteModel.AllowedCustomerList = (await UserMapService.GetUserCustomerMapModel(User.GetUserId())).Customers;
            //AllowedSundries = await _quotesApi.GetQuoteSurcharges()    
            quotationViewModel.quoteId = quoteId;
            return View(quotationViewModel);

        }

        [HttpGet]
        public ActionResult RenderQuotationAsPDF(long quoteId)
        {
            var quotationViewModel = new QuotationViewModel();
            quotationViewModel.GetQuotationAsPDF = GetProposalPdf(quoteId);

            return File(quotationViewModel.GetQuotationAsPDF, "application/pdf");
        }

        private static byte[] GetProposalPdf(long quoteId)
        {
            var quotationViewModel = new QuotationViewModel();
            var section = _config.GetSection("Credentials");
            var client = new WebClient();
            {
                client.Credentials = new NetworkCredential(section.GetSection("username").Value, section.GetSection("password").Value, section.GetSection("domain").Value);
            }
            quotationViewModel.ReportUrl = "http://tiger/ReportServer/Pages/ReportViewer.aspx?/CRMReports/QuoteNewFormat&QuoteID=@quoteId&rs:ClearSession=true&rc:Parameters=Collapsed&rs:Command=Render&rs:Format=PDF".Replace("@quoteId", quoteId.ToString());

            try
            {
                return client.DownloadData(quotationViewModel.ReportUrl);
            }
            catch
            {
                return null;
            }
        }
