using System.Collections.Generic;
using System.Linq;
using EbayFetcher.com.ebay.developer.FindingsService;

namespace EbayFetcher.FetcherService
{
    internal class EbayFetcher
    {
        private readonly string[] _idCategories;
        private readonly CustomFindingService _service;
        private readonly FindItemsByCategoryRequest _findItemsByCategoryRequest;

        public EbayFetcher()
        {
            // Setting up Categories
            _idCategories = EbayConnector.GetCategoriesOf(Properties.Settings.Default.idVirtualReality_prod)
                .CategoryArray.Select(c => c.CategoryID)
                .Where(c => c == "183067" || c == "183068" || c == "183069")
                .ToArray();
            _service = new CustomFindingService {Url = Properties.Settings.Default.urlFindings_prod};
            _findItemsByCategoryRequest = new FindItemsByCategoryRequest {categoryId = _idCategories, paginationInput = new PaginationInput {pageNumber = 1, pageNumberSpecified = true}};
        }

        public IEnumerable<SearchItem> FetchResults()
        {
            var response = _service.findItemsByCategory(_findItemsByCategoryRequest);
            var items = new List<SearchItem>();
            for (var i = 2; i < response.paginationOutput.totalPages; i++)
            {
                var tmpFindItemsByCategoryRequest = new FindItemsByCategoryRequest {categoryId = _idCategories, paginationInput = new PaginationInput {pageNumber = i, pageNumberSpecified = true}};
                items.AddRange(_service.findItemsByCategory(tmpFindItemsByCategoryRequest).searchResult.item);
            }

            return items.OrderBy(i => i.itemId).GroupBy(i => i.itemId).Select(g => g.First()).ToList();
        }
    }
}