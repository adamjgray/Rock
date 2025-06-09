
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    public class CommunicationFlowPerformanceFlowInstanceBag
    {
        public Guid Guid { get; set; }

        public DateTime? StartDate { get; set; }

        public List<CommunicationFlowPerformanceConversionHistoryBag> Conversions { get; set; }

        public List<CommunicationFlowPerformanceRecipientBag> Recipients { get; set; }
    }
}
