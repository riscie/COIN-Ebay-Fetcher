using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbayFetcher.DbModels
{
    class ListingInfoDbModel
    {
        [Key]
        public int Id { get; set; }

        public bool BestOfferEnabled { get; set; }

        public bool BestOfferEnabledFieldSpecified { get; set; }

        public bool BuyItNowAvailable { get; set; }

        public bool BuyItNowAvailableFieldSpecified { get; set; }

        public string BuyItNowPrice { get; set; }

        public string ConvertedBuyItNowPrice { get; set; }

        public DateTime StartTime { get; set; }

        public bool StartTimeFieldSpecified { get; set; }

        public DateTime EndTime { get; set; }

        public bool EndTimeFieldSpecified { get; set; }

        public string ListingType { get; set; }

        public bool Gift { get; set; }

        public bool GiftFieldSpecified { get; set; }

        //public string Delimiter { get; set; }
    }
}