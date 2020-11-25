using System;

namespace SFA.DAS.Assessor.Functions.MockApis.DataCollection.DataGenerator
{
    public static class ProviderCountDates
    {
        public static DateTime TenDate => new DateTime(1000, 06, 15).Date;
        public static DateTime OneHundredDate => new DateTime(1000, 05, 15).Date;
        public static DateTime TwoHundredDate => new DateTime(1000, 04, 15).Date;
        public static DateTime FourHundredDate => new DateTime(1000, 03, 15).Date;
        public static DateTime EightHundredDate => new DateTime(1000, 02, 15).Date;
        public static DateTime SixteenHundredDate => new DateTime(1000, 01, 15).Date;

        public static void GetCounts(DateTime lastRunDate, out int providerCount, out int learnerCount, out int learningDeliveryCount)
        {
            if(lastRunDate.Month == TenDate.Month && lastRunDate.Day == TenDate.Day)
            {
                providerCount = 10;
                learnerCount = 4;
                learningDeliveryCount = 1;
            }
            else if (lastRunDate.Month == OneHundredDate.Month && lastRunDate.Day == OneHundredDate.Day)
            {
                providerCount = 100;
                learnerCount = 8;
                learningDeliveryCount = 2;
            }
            else if (lastRunDate.Month == TwoHundredDate.Month && lastRunDate.Day == TwoHundredDate.Day)
            {
                providerCount = 200;
                learnerCount = 16;
                learningDeliveryCount = 3;
            }
            else if (lastRunDate.Month == FourHundredDate.Month && lastRunDate.Day == FourHundredDate.Day)
            {
                providerCount = 400;
                learnerCount = 32;
                learningDeliveryCount = 4;
            }
            else if (lastRunDate.Month == EightHundredDate.Month && lastRunDate.Day == EightHundredDate.Day)
            {
                providerCount = 800;
                learnerCount = 64;
                learningDeliveryCount = 5;
            }
            else if (lastRunDate.Month == SixteenHundredDate.Month && lastRunDate.Day == SixteenHundredDate.Day)
            {
                providerCount = 1600;
                learnerCount = 128;
                learningDeliveryCount = 6;
            }
            else
            {
                providerCount = 0;
                learnerCount = 0;
                learningDeliveryCount = 0;
            }
        }
    }
}
