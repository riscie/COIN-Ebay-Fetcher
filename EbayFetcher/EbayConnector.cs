using EbayFetcher.com.ebay.developer.EbayService;

namespace EbayFetcher
{
    public class EbayConnector
    {
        private const string SiteId = "0";
        private const string Version = "405";

        private static eBayAPIInterfaceService OpenEbayServiceConnection(string callName)
        {
            // Connection settings
            var endpoint = Program.IsProd ? Properties.Settings.Default.endPoint_prod : Properties.Settings.Default.endPoint_sandbox;
            var appId = Program.IsProd ? Properties.Settings.Default.appId_prod : Properties.Settings.Default.appId_sandbox;
            var devId = Program.IsProd ? Properties.Settings.Default.devId_prod : Properties.Settings.Default.devId_sandbox;
            var certId = Program.IsProd ? Properties.Settings.Default.certId_prod : Properties.Settings.Default.certId_sandbox;

            // Build the request URL
            var requestUrl = endpoint
            + "?callname=" + callName
            + "&siteid=" + SiteId
            + "&appid=" + appId
            + "&version=" + Version
            + "&routing=default";

            // Create the service
            return new eBayAPIInterfaceService
            {
                Url = requestUrl,
                RequesterCredentials = new CustomSecurityHeaderType
                {
                    eBayAuthToken = Program.IsProd ? Properties.Settings.Default.token_prod : Properties.Settings.Default.token_sandbox,
                    Credentials = new UserIdPasswordType { AppId = appId, DevId = devId, AuthCert = certId }
                }
            };
        }

        public static GetCategoriesResponseType GetCategoriesOf(string id)
        {
            var service = OpenEbayServiceConnection("GetCategories");

            // Get Virtual Reality Category
            var categoryRequestType = new GetCategoriesRequestType
            {
                DetailLevel = new[] { DetailLevelCodeType.ReturnAll },
                CategorySiteID = "0",
                Version = Version,
                CategoryParent = new[] { id }
            };

            return service.GetCategories(categoryRequestType);
        }
    }
}
