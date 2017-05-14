using System;
using System.ComponentModel.DataAnnotations;

namespace EbayFetcher.DbModels
{
    class SellingStatusDbModel
    {
        [Key]
        public int Id { get; set; }

        public double CurrentPrice { get; set; }

        public int BidCount { get; set; }

        public int InterestCount { get; set; }

        public string SellingState { get; set; }
    }
}