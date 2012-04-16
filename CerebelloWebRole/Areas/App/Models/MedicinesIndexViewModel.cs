using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class MedicinesIndexViewModel
    {
        public List<MedicineViewModel> Medicines { get; set; }
        public int Count { get; set; }
    }
}