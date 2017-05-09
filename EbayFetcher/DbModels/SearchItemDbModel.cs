using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbayFetcher.com.ebay.developer.FindingsService;
using EbayFetcher.DbModels;

namespace EbayFetcher
{
    class SearchItemDbObject
    {
        [Key]
        public long ItemId { get; set; }

        public string Title { get; set; }

        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string GalleryUrl { get; set; }

        public string ViewItemUrl { get; set; }

        public string PostalCode { get; set; }

        public string Location { get; set; }

        public string Country { get; set; }

        public string ShippingCurrency { get; set; }

        public string ShippingCost { get; set; }

        public string ShipToLocations { get; set; }

        public ListingInfoDbModel ListingInfo { get; set; }

        public ConditionDbModel Condition { get; set; }
    }
}