using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using AutoMapper;
using EbayFetcher.com.ebay.developer.FindingsService;
using EbayFetcher.DbModels;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Slack;
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

            //Setup Logging
            var slackConfig = new SlackConfiguration()
            {
                WebhookUrl = new Uri("https://hooks.slack.com/services/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"),
                MinLevel = LogLevel.Information
            };
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddSlack(slackConfig, "Ebay Fetcher", "Production");
            var logger = loggerFactory.CreateLogger("Ebay Fetcher");


//            // Fetch Items
            try
            {
                var ebayFetcher = new FetcherService.EbayFetcher();
                var results = ebayFetcher.FetchResults().ToList();

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
                    var mappedSearchItems = Mapper.Map<IEnumerable<SearchItem>, IEnumerable<SearchItemDbObject>>(results).ToList();

                    // Set number of interested in model, where existing
                    foreach (var searchItem in mappedSearchItems.Where(si => interestedsOfSearchItem.ContainsKey(si.ItemId.ToString()))) searchItem.SellingStatus.InterestCount = interestedsOfSearchItem[searchItem.ItemId.ToString()];

                    //Get current entries from DB
                    var updateCount = 0;
                    var newCount = 0;
                    foreach (var searchItem in mappedSearchItems)
                    {
                        var itemToUpdate = context.SearchItems.FirstOrDefault(i => i.ItemId == searchItem.ItemId);
                        if (itemToUpdate != null)
                        {
                            var updatedItem = Mapper.Map(itemToUpdate, searchItem);
                            context.SearchItems.Update(updatedItem);
                            updateCount++;
                        }
                        else
                        {
                            context.Add(searchItem);
                            newCount++;
                        }
                    }
                    // Save fields
                    context.SaveChanges();
                    logger.LogInformation($"Created {newCount} new entries. / Updated {updateCount} entries in the db...");
                }
            }
            catch (Exception e)

            {
                Console.WriteLine(e.Message);
                logger.LogError("Error Fetching items... please investigate.");
                Console.ReadKey();
            }

            List<SearchItemDbObject> itemsToFetchGpsLocations;
            using (var context = new EbayFetcherDbContext())
            {
                itemsToFetchGpsLocations = context.SearchItems.Where(s => s.Longitude == null && s.Latitude == null && s.PostalCode != null).ToList();
            }
            var locCount = 0;
            foreach (var item in itemsToFetchGpsLocations)
            {
                using (var wc = new WebClient())
                {
                    if (locCount % 10 == 0)
                    {
                        Console.WriteLine(locCount);
                    }
                    try
                    {
                        using (var context = new EbayFetcherDbContext())
                        {
                            var json = wc.DownloadString($"https://maps.googleapis.com/maps/api/geocode/json?address={item.PostalCode}+{item.Location}+{item.Country}&key=AIzaSyDlVUkkbAyjj_Xxr9OoLtD8uflgJVGyJ98");
                            var deserializedLocation = JsonConvert.DeserializeObject<RootLocationObject>(json);
                            var lat = deserializedLocation.results.FirstOrDefault();
                            item.Latitude = lat?.geometry.location.lat.ToString(CultureInfo.InvariantCulture) ?? "";
                            var lng = deserializedLocation.results.FirstOrDefault();
                            item.Longitude = lng?.geometry.location.lng.ToString(CultureInfo.InvariantCulture) ?? "";
                            locCount++;
                            context.SearchItems.Update(item);
                            context.SaveChanges();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error fetching locations: " + e.Message);
                    }
                }
            }

            // Save Location Updates
            Console.WriteLine($"Updated Longitude / Latitude on  {itemsToFetchGpsLocations.Count()} items.");
            logger.LogInformation($"Updated Longitude / Latitude on {locCount} items.");
        }
    }
}