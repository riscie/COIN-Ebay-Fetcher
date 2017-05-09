using System.ComponentModel.DataAnnotations;

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