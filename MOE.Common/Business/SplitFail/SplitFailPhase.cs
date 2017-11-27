﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MOE.Common.Business.CustomReport;
using MOE.Common.Business.WCFServiceLibrary;
using MOE.Common.Models;
using MOE.Common.Models.Repositories;

namespace MOE.Common.Business.SplitFail
{
    public class SplitFailPhase
    {
        public List<SplitFailBin> Bins { get; private set; } = new List<SplitFailBin>();
        public int TotalFails { get; private set; } = 0;
        public Approach Approach { get; }
        private bool GetPermissivePhase { get; }
        public List<CycleSplitFail> Cycles { get; }
        public List<PlanSplitFail> Plans { get; }
        public Dictionary<string, string> Statistics { get; }
        private List<SplitFailDetectorActivation> _detectorActivations = new List<SplitFailDetectorActivation>();

        public SplitFailPhase(Approach approach, SplitFailOptions options, bool getPermissivePhase)
        {
            Approach = approach;
            GetPermissivePhase = getPermissivePhase;
            Cycles = CycleFactory.GetSplitFailCycles(options, approach, getPermissivePhase);
            SetDetectorActivations(options);
            AddDetectorActivationsToCycles();
            Plans = PlanFactory.GetSplitFailPlans(Cycles, options, Approach);
            TotalFails = Cycles.Count(c => c.IsSplitFail);
            Statistics = new Dictionary<string, string>();
            Statistics.Add("Total Split Failures" , TotalFails.ToString());
            SetBinStatistics(options);
        }

        private void AddDetectorActivationsToCycles()
        {
            foreach (var cycleSplitFail in Cycles)
            {
                cycleSplitFail.SetDetectorActivations(_detectorActivations);
            }
        }

        private void SetBinStatistics(SplitFailOptions options)
        {
            DateTime startTime = options.StartDate;
            do
            {
                DateTime endTime = startTime.AddMinutes(15);
                var cycles = Cycles.Where(c => c.StartTime >= startTime && c.StartTime < endTime).ToList();
                Bins.Add(new SplitFailBin(startTime, endTime, cycles));
                startTime = startTime.AddMinutes(15);
            } while (startTime < options.EndDate);
        }
        
        private List<SplitFailDetectorActivation> CombineDetectorActivations(List<SplitFailDetectorActivation> detectorActivations)
        {
            List<SplitFailDetectorActivation> tempDetectorActivations = new List<SplitFailDetectorActivation>();
            foreach (var current in detectorActivations)
            {
                if (!current.ReviewedForOverlap)
                {
                    var overlapingActivations = detectorActivations.Where(d =>
                        ((d.DetectorOn >= current.DetectorOn && d.DetectorOn <= current.DetectorOff && d.DetectorOff >= current.DetectorOff)
                         || (d.DetectorOn <= current.DetectorOn && d.DetectorOff >= current.DetectorOn && d.DetectorOff <= current.DetectorOff)
                         || (d.DetectorOn >= current.DetectorOn && d.DetectorOff <= current.DetectorOff)
                         || (d.DetectorOn <= current.DetectorOn && d.DetectorOff >= current.DetectorOff))
                        && d.ReviewedForOverlap == false).ToList();
                    if (overlapingActivations.Any())
                    {
                        var tempDetectorActivation = new SplitFailDetectorActivation
                        {
                            DetectorOn = overlapingActivations.Min(o => o.DetectorOn),
                            DetectorOff = overlapingActivations.Max(o => o.DetectorOff)
                        };
                        tempDetectorActivations.Add(tempDetectorActivation);
                        foreach (var splitFailDetectorActivation in overlapingActivations)
                        {
                            splitFailDetectorActivation.ReviewedForOverlap = true;
                        }
                    }
                }
            }
            if (detectorActivations.Count != tempDetectorActivations.Count)
            {
                tempDetectorActivations = CombineDetectorActivations(tempDetectorActivations);
            }
            return  tempDetectorActivations.OrderBy(t => t.DetectorOn).ToList();
        }
        
        private void SetDetectorActivations(SplitFailOptions options)
        {
            var controllerEventsRepository = ControllerEventLogRepositoryFactory.Create();
            int phaseNumber = GetPermissivePhase ? Approach.PermissivePhaseNumber.Value : Approach.ProtectedPhaseNumber;
            var detectors = Approach.Signal.GetDetectorsForSignalThatSupportAMetricByPhaseNumber(12, phaseNumber);
            foreach (var detector in detectors)
            {
                var lastCycle = Cycles.LastOrDefault();
                options.EndDate = lastCycle?.EndTime ?? options.EndDate;
                var events = controllerEventsRepository.GetEventsByEventCodesParam(Approach.SignalID,
                    options.StartDate, options.EndDate, new List<int>() {81, 82}, detector.DetChannel);
                if (!events.Any())
                {
                    CheckForDetectorOnBeforeStart(options, controllerEventsRepository, detector);
                }
                else
                {
                    //AddDetectorOnToBeginningIfNecessary(options, detector, events);
                    //AddDetectorOffToEndIfNecessary(options, detector, events);
                    AddDetectorActivationsFromList(events);
                }
            }
            _detectorActivations = CombineDetectorActivations(_detectorActivations);
        }

        private void AddDetectorActivationsFromList(List<Controller_Event_Log> events)
        {
            for (int i = 0; i < events.Count - 2; i++)
            {
                if (events[i].EventCode == 81 && events[i + 1].EventCode == 82)
                {
                    _detectorActivations.Add(new SplitFailDetectorActivation
                    {
                        DetectorOn = events[i].Timestamp,
                        DetectorOff = events[i + 1].Timestamp
                    });
                }
            }
        }

        private static void AddDetectorOffToEndIfNecessary(SplitFailOptions options, Models.Detector detector, List<Controller_Event_Log> events)
        {
            if (events.LastOrDefault()?.EventCode == 81)
            {
                events.Insert(0, new Controller_Event_Log
                {
                    Timestamp = options.EndDate,
                    EventCode = 82,
                    EventParam = detector.DetChannel,
                    SignalID = options.SignalID
                });
            }
        }

        private static void AddDetectorOnToBeginningIfNecessary(SplitFailOptions options, Models.Detector detector, List<Controller_Event_Log> events)
        {
            if (events.FirstOrDefault()?.EventCode == 82)
            {
                events.Insert(0, new Controller_Event_Log
                {
                    Timestamp = options.StartDate,
                    EventCode = 81,
                    EventParam = detector.DetChannel,
                    SignalID = options.SignalID
                });
            }
        }

        private void CheckForDetectorOnBeforeStart(SplitFailOptions options, IControllerEventLogRepository controllerEventsRepository, Models.Detector detector)
        {
            var eventOnBeforeStart = controllerEventsRepository.GetFirstEventBeforeDateByEventCodeAndParameter(options.SignalID,
                    detector.DetChannel, 81, options.StartDate);
            var eventOffBeforeStart = controllerEventsRepository.GetFirstEventBeforeDateByEventCodeAndParameter(options.SignalID,
                    detector.DetChannel, 82, options.StartDate);
            if (eventOnBeforeStart != null && eventOffBeforeStart == null)
            {
                _detectorActivations.Add(new SplitFailDetectorActivation
                {
                    DetectorOn = options.StartDate,
                    DetectorOff = options.EndDate
                });
            }
        }
    }
}

        
    
