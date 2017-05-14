using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using EbayFetcher.com.ebay.developer.FindingsService;
using EbayFetcher.DbModels;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace EbayFetcher
{
    internal class Program
    {
        public const bool IsProd = true;

        private static void Main()
        {
            // Map from Ebay Service Models to DB Models using AutoMapper
            Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<SearchItem, SearchItemDbObject>()
                        .ForMember(dest => dest.ItemId, opt => opt.MapFrom(source => long.Parse(source.itemId)))
                        .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(source => source.primaryCategory.categoryName))
                        .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(source => source.primaryCategory.categoryId))
                        .ForMember(dest => dest.ShippingCurrency, opt => opt.MapFrom(source => source.shippingInfo.shippingServiceCost.currencyId))
                        .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(source => source.shippingInfo.shippingServiceCost.Value))
                        .ForMember(dest => dest.ShipToLocations, opt => opt.MapFrom(source => string.Join(",", source.shippingInfo.shipToLocations)));
                    cfg.CreateMap<ListingInfo, ListingInfoDbModel>()
                        .ForMember(dest => dest.BuyItNowPrice, opt => opt.MapFrom(source => source.buyItNowPrice.Value))
                        .ForMember(dest => dest.ConvertedBuyItNowPrice, opt => opt.MapFrom(source => source.convertedBuyItNowPrice.Value));
                    cfg.CreateMap<Condition, ConditionDbModel>();
                    cfg.CreateMap<SellingStatus, SellingStatusDbModel>()
                    .ForMember(dest => dest.BidCount, opt => opt.MapFrom(source => source.bidCountSpecified ? source.bidCount : -1))
                    .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(source => source.currentPrice.Value))
                    .ForMember(dest => dest.SellingState, opt => opt.MapFrom(source => source.sellingState));
                }
            );

            var ebayFetcher = new FetcherService.EbayFetcher();
            var results = ebayFetcher.FetchResults();

            // Console Output
            foreach (var item in results)
            {
                Console.WriteLine("Item found: " + item.itemId + " title: " + item.title);
            }

            // JSON File Output
            File.WriteAllText(@"C:\temp\Items_" + DateTime.Now.ToFileTimeUtc() + ".json", JsonConvert.SerializeObject(results));

            // Get Numbers of interested people only from auctions and aren't finished yet
            var interestedsOfSearchItem = new Dictionary<string, int>();
            foreach (var item in results.Where(i => (i.listingInfo.listingType == "Auction" || i.listingInfo.listingType == "AuctionWithBIN") && i.sellingStatus.sellingState == "Active"))
            {
                var web = new HtmlWeb();
                var document = web.Load(item.viewItemURL);
                var interestedNode = document.DocumentNode.SelectSingleNode("//span[@class='vi-buybox-watchcount']");
                if (interestedNode == null) continue;
                var interested = string.IsNullOrEmpty(interestedNode.InnerText) ? -1 : Int32.Parse(interestedNode.InnerText);
                interestedsOfSearchItem.Add(item.itemId, interested);
            }

            // DB Output
            using (var context = new EbayFetcherDbContext())
            {
                context.Database.EnsureCreated();
                var mappedSearchItems = Mapper.Map<IEnumerable<SearchItem>, IEnumerable<SearchItemDbObject>>(results);

                // Set number of interested in model, where existing
                foreach (var searchItem in mappedSearchItems.Where(si => interestedsOfSearchItem.ContainsKey(si.ItemId.ToString()))) searchItem.SellingStatus.InterestCount = interestedsOfSearchItem[searchItem.ItemId.ToString()];

                // Save fields
                context.SearchItems.AddRange(mappedSearchItems);
                context.SaveChanges();
            }
        }
    }
}