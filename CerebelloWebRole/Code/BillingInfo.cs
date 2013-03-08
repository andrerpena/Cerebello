using System;
using System.Collections.Generic;
using System.Linq;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public class BillingInfo
    {
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public decimal TotalValue
        {
            get { return this.Items.Sum(item => item.Value); }
        }

        public List<Item> Items { get; set; }

        public class Item
        {
            public decimal Value { get; set; }
            public ContractTypes ContractType { get; set; }
        }
    }

    public class PartialBillingInfo
    {
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Value { get; set; }
        public int ContractId { get; set; }
    }
}