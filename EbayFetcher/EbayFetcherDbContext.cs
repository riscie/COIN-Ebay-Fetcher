using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbayFetcher.com.ebay.developer.FindingsService;
using EbayFetcher.DbModels;
using Microsoft.EntityFrameworkCore;
using MySQL.Data.EntityFrameworkCore.Extensions;

namespace EbayFetcher
{
    class EbayFetcherDbContext : DbContext
    {
        public DbSet<SearchItemDbObject> SearchItems { get; set; }
        public DbSet<ListingInfoDbModel> ListingInfos { get; set; }
        public DbSet<ConditionDbModel> Conditions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseMySQL(@"server=localhost;userid=root;pwd=;port=3306;database=coin_db;sslmode=none;");
            optionsBuilder.UseMySQL(@"server=46.101.252.74;userid=coin;pwd=coinPw;port=3310;database=coin_db;sslmode=none;");
        }
    }
}