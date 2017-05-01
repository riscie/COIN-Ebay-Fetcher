using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EbayFetcher.com.ebay.developer.FindingsService;
using Newtonsoft.Json;

namespace EbayFetcher
{
    internal class Program
    {
        public static bool IsProd = true;

        private static void Main()
        {
            var idCategories = EbayConnector.GetCategoriesOf(IsProd ? Properties.Settings.Default.idVirtualReality_prod : Properties.Settings.Default.idVirtualReality_sandbox)
                .CategoryArray.Select(c => c.CategoryID)
                .Where(c => c == "183067" || c == "183068" || c == "183069").ToArray();
            var service = new CustomFindingService { Url = IsProd ? Properties.Settings.Default.urlFindings_prod : Properties.Settings.Default.urlFindings_sandbox };

            // Creating response object
            var findItemsByCategoryRequest = new FindItemsByCategoryRequest { categoryId = idCategories, paginationInput = new PaginationInput { pageNumber = 1, pageNumberSpecified = true } };
            var response = service.findItemsByCategory(findItemsByCategoryRequest);

            var items = new List<SearchItem>();
            items.AddRange(response.searchResult.item);
            for (var i = 2; i < response.paginationOutput.totalPages; i++)
            {
                var tmpFindItemsByCategoryRequest = new FindItemsByCategoryRequest { categoryId = idCategories, paginationInput = new PaginationInput { pageNumber = i, pageNumberSpecified = true } };
                items.AddRange(service.findItemsByCategory(tmpFindItemsByCategoryRequest).searchResult.item);
            }

            var orderedEnumerable = items.OrderBy(i => i.itemId).GroupBy(i => i.itemId).Select(g => g.First()).ToList();
            foreach (var item in orderedEnumerable) Console.WriteLine("Item found: " + item.itemId + " title: " + item.title);


            File.WriteAllText(@"C:\temp\Items_" + DateTime.Now.ToFileTimeUtc() + ".txt", JsonConvert.SerializeObject(orderedEnumerable));

            // Make the call to GeteBayOfficialTime
            Console.Write("Finishing");
        }
    }
}
