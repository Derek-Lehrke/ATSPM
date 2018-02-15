﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MOE.Common.Business.Bins;
using MOE.Common.Business.WCFServiceLibrary;
using MOE.Common.Models;

namespace MOE.Common.Business.DataAggregation
{
    public class PreemptionAggregationBySignal:AggregationBySignal
    {
        protected override void LoadBins(SignalAggregationMetricOptions options, Models.Signal signal)
        {
            var preemptionAggregationRepository =
               Models.Repositories.PreemptAggregationDatasRepositoryFactory.Create();
            List<PreemptionAggregation> preemptions =
                preemptionAggregationRepository.GetPreemptionsBySignalIdAndDateRange(
                    signal.SignalID, BinsContainers.Min(b => b.Start), BinsContainers.Max(b => b.End));
            if (preemptions != null)
            {
                ConcurrentBag<BinsContainer> concurrentBinContainers = new ConcurrentBag<BinsContainer>();
                //foreach (var binsContainer in binsContainers)
                Parallel.ForEach(BinsContainers, binsContainer =>
                {
                    BinsContainer tempBinsContainer =
                        new BinsContainer(binsContainer.Start, binsContainer.End);
                    ConcurrentBag<Bin> concurrentBins = new ConcurrentBag<Bin>();
                    //foreach (var bin in binsContainer.Bins)
                    Parallel.ForEach(binsContainer.Bins, bin =>
                    {
                        if (preemptions.Any(s => s.BinStartTime >= bin.Start && s.BinStartTime < bin.End))
                        {
                            int preemptionSum = 0;

                            switch (options.SelectedAggregatedDataType.DataName)
                            {
                                case "PreemptNumber":
                                    preemptionSum = preemptions.Where(s => s.BinStartTime >= bin.Start && s.BinStartTime < bin.End)
                                        .Sum(s => s.PreemptNumber);
                                    break;
                                case "PreemptRequests":
                                    preemptionSum = preemptions.Where(s => s.BinStartTime >= bin.Start && s.BinStartTime < bin.End)
                                        .Sum(s => s.PreemptRequests);
                                    break;
                                case "PreemptServices":
                                    preemptionSum = preemptions.Where(s => s.BinStartTime >= bin.Start && s.BinStartTime < bin.End)
                                        .Sum(s => s.PreemptServices);
                                    break;
                            }
                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = preemptionSum,
                                Average = preemptionSum
                            });
                        }
                        else
                        {
                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = 0,
                                Average = 0
                            });
                        }

                    });
                    tempBinsContainer.Bins = concurrentBins.OrderBy(c => c.Start).ToList();
                    concurrentBinContainers.Add(tempBinsContainer);
                });
                BinsContainers = concurrentBinContainers.OrderBy(b => b.Start).ToList();
            }
        }
        

        public PreemptionAggregationBySignal(SignalPreemptionAggregationOptions options, Models.Signal signal) : base(options, signal)
        {
            LoadBins(options, signal);
        }
        
        
    }
    
        
    
        
}