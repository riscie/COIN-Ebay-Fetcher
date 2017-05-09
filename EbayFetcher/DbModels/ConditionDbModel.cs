using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbayFetcher.DbModels
{
    class ConditionDbModel
    {
        [Key]
        public int Id { get; set; }

//        public int ConditionId { get; set; }

        public bool ConditionIdFieldSpecified { get; set; }

        public string ConditionDisplayName { get; set; }

//        public string Delimiter { get; set; }
    }
}