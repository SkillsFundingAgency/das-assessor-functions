﻿using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IDeliveryNotificationCommand : ICommand
    {
        ICollector<string> StorageQueue { get; set; }
    }
}
