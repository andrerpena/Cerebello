using System;
using System.Collections.Generic;
using System.Linq;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public class BillingInfo
    {
        public BillingInfo()
        {
            this.Items = new List<Item>();
        }

        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public decimal TotalValue
        {
            get { return this.Items.Sum(item => item.Value); }
        }

        public List<Item> Items { get; private set; }

        public class Item
        {
            public decimal Value { get; set; }
            public ContractTypes ContractType { get; set; }
        }

        public DateTime DueDate { get; set; }
    }
}