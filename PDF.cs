        [HttpGet]
        [Route("Quotations/GetQuoteDocument/{QuoteID}")]
        [SwaggerOperation(Summary = "Get Quote PDF", Description = "get Quote as a PDF")]

        public ActionResult<DocumentRepositories> GetQuoteDocument(int quoteId)
        {
            var client = new WebClient();
            {
                client.Credentials = new NetworkCredential("servicea", "d0nderenbl1ksem", "tritonexpress");
            };
            var report = client.DownloadData(@"http://Tiger/ReportServer?/CRMReports/Quote&QuoteID=" + quoteId + "&rs:ClearSession=true&rs:Command=Render&rs:Format=PDF");
            var doc = new DocumentRepositories
            {
                ImgContentType = "application/pdf",
                ImgData = report,
                ImgName = "Quote.pdf",
                ImgLength = report.Length
            };
            return Ok(doc);
        }
        
        
        [HttpGet]
        [Route("Quotations/EmailQuoteDocument/{QuoteID}/{email}")]
        [SwaggerOperation(Summary = "Get Quote PDF Email", Description = "get Quote as a PDF email")]
        public ActionResult<DocumentRepositories> EmailQuoteDocument(int QuoteID, string email)
        {
            var client = new WebClient();
            {
                client.Credentials = new NetworkCredential("servicea", "d0nderenbl1ksem", "tritonexpress");
            };
            var report = client.DownloadData(@"http://Tiger/ReportServer?/CRMReports/Quote&QuoteID=" + QuoteID + "&rs:ClearSession=true&rs:Command=Render&rs:Format=PDF");
            var doc = new DocumentRepositories
            {
                ImgContentType = "application/pdf",
                ImgData = report,
                ImgName = "Quote.pdf",
                ImgLength = report.Length
            };
            var stream = new MemoryStream(report, 0, report.Length, false, true);
            var attachments = new List<System.Net.Mail.Attachment>();
            attachments.Add(new System.Net.Mail.Attachment(stream, "quote.pdf"));
            Core.Email.SendIntraSystemEmail(new[] { email }, null, "administrator@tritonexpress.co.za", "Please find attached the requested quotation", "Requested Quote document", "texdcmailmbx01", attachments, null);
            return Ok();
        }
