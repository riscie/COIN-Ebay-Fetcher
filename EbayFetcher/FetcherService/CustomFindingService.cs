using System;
using System.Net;
using EbayFetcher.com.ebay.developer.FindingsService;

namespace EbayFetcher.FetcherService
{
    public class CustomFindingService : FindingService
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var request = (HttpWebRequest)base.GetWebRequest(new Uri(Program.IsProd ? Properties.Settings.Default.urlFindings_prod : Properties.Settings.Default.urlFindings_sandbox));
            request.Headers.Add("X-EBAY-SOA-SECURITY-APPNAME", Program.IsProd ? Properties.Settings.Default.appId_prod : Properties.Settings.Default.appId_sandbox);
            request.Headers.Add("X-EBAY-SOA-OPERATION-NAME", "findItemsByCategory");
            request.Headers.Add("X-EBAY-SOA-SERVICE-NAME", "FindingService");
            request.Headers.Add("X-EBAY-SOA-MESSAGE-PROTOCOL", "SOAP11");
            request.Headers.Add("X-EBAY-SOA-SERVICE-VERSION", "1.0.0");
            request.Headers.Add("X-EBAY-SOA-GLOBAL-ID", "EBAY-US");
            return request;
        }
    }
}
