﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Common.Models.ViewModel.RouteEdit
{
    public class RouteMapViewModel:Chart.SignalSearchViewModel
    {
        public List<string> SelectedSignals { get; set; } = new List<string> { "1", "2" };
        public RouteMapViewModel()
        {

        }
    }
}