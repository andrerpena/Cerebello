using System;
using System.Collections.Generic;
using System.Linq;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public class InvoiceInfo
    {
        public InvoiceInfo()
        {
            this.Items = new List<Item>();
        }

        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public decimal TotalAmount
        {
            get { return this.Items.Sum(item => item.Amount); }
        }

        public decimal TotalDiscount
        {
            get { return this.Items.Sum(item => item.DiscountAmount); }
        }

        public List<Item> Items { get; private set; }

        public class Item
        {
            public decimal Amount { get; set; }
            public decimal DiscountAmount { get; set; }
            public ContractTypes ContractType { get; set; }
        }

        public DateTime DueDate { get; set; }

        public bool IsSaved { get; set; }

        public string NameId
        {
            get { return string.Format("contract-{0}-cycle-start-{1}", this.Items[0].ContractType, this.Start.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss")); }
        }
    }
}