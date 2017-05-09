using System;
using System.Collections.Generic;
using System.IO;
using AutoMapper;
using EbayFetcher.com.ebay.developer.FindingsService;
using EbayFetcher.DbModels;
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


            // DB Output
            using (var context = new EbayFetcherDbContext())
            {
                context.Database.EnsureCreated();
                context.SearchItems.AddRange(Mapper.Map<IEnumerable<SearchItem>, IEnumerable<SearchItemDbObject>>(results));
                context.SaveChanges();
            }
        }
    }
}