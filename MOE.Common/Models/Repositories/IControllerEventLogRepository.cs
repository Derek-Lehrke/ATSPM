﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Common.Models.Repositories
{
    public interface IControllerEventLogRepository
    {
        double GetTmcVolume(DateTime startDate, DateTime endDate, string signalId, int phase);
        List<Controller_Event_Log> GetSplitEvents(string signalId, DateTime startTime, DateTime endTime);
        List<Controller_Event_Log> GetSignalEventsByEventCode(string signalId, 
            DateTime startTime, DateTime endTime, int eventCode);
        List<Controller_Event_Log> GetSignalEventsByEventCodes(string signalId,
            DateTime startTime, DateTime endTime, List<int> eventCodes);
        List<Controller_Event_Log> GetEventsByEventCodesParam(string signalId,
           DateTime startTime, DateTime endTime, List<int> eventCodes, int param);

        List<Controller_Event_Log> GetTopEventsAfterDateByEventCodesParam(string signalId, DateTime timestamp, List<int> eventCodes, int param, int top);
        int GetEventCountByEventCodesParamDateTimeRange(string signalId,
            DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute,
            List<int> eventCodes, int param);
        List<Controller_Event_Log> GetEventsByEventCodesParamDateTimeRange(string signalId,
            DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute,
            List<int> eventCodes, int param);
        List<Controller_Event_Log> GetEventsByEventCodesParamWithOffset(string signalId,
           DateTime startTime, DateTime endTime, List<int> eventCodes, int param, double offset);
        Controller_Event_Log GetFirstEventBeforeDate(string signalId,
            int eventCode, DateTime date);
        List<Controller_Event_Log> GetSignalEventsBetweenDates(string signalId,
             DateTime startTime, DateTime endTime);
        List<Controller_Event_Log> GetTopNumberOfSignalEventsBetweenDates(string signalId, int numberOfRecords,
                     DateTime startTime, DateTime endTime);
        int GetDetectorActivationCount(string signalId,
             DateTime startTime, DateTime endTime, int detectorChannel);
        int GetRecordCount(string signalId, DateTime startTime, DateTime endTime);
        List<Controller_Event_Log> GetAllAggregationCodes(string signalId, DateTime startTime, DateTime endTime);

        Controller_Event_Log GetFirstEventBeforeDateByEventCodeAndParameter(string signalId, int eventCode,
            int eventParam, DateTime date);
    }
}
