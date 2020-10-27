using System;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.MockApis.DataCollection.DataGenerator
{
    public class OtherDataCollectionLearningDelivery
    {
        public int? Outcome { get; set; }
        public DateTime? LearnActEndDate { get; set; }
        public string OutGrade { get; set; }
        public int? WithdrawReason { get; set; }

        public OtherDataCollectionLearningDelivery()
        {
            SetOutcome();
            SetWithdrawReason();
            SetLearnActEndDate();
            SetOutGrade();
        }

        private void SetOutcome()
        {
            int seed = new Random().Next(0, 10);

            if (seed >= 5)
                Outcome = null;
            else if (seed == 4)
                Outcome = 3;
            else if (seed == 3)
                Outcome = 8;
            else
                Outcome = 1;
        }

        private void SetWithdrawReason()
        {
            List<int?> withdrawReasonList = new List<int?> { null, 2, 3, 29, 40, 44, 97, 98 };
            int index = new Random().Next(withdrawReasonList.Count);

            if (Outcome == null)
                WithdrawReason = null;
            else
                WithdrawReason = withdrawReasonList[index];
        }

        private void SetLearnActEndDate()
        {
            int days = new Random().Next(50, 200);
            if (Outcome == null)
                LearnActEndDate = null;
            else
                LearnActEndDate = DateTime.Now.AddDays(-days);
        }

        private void SetOutGrade()
        {
            List<string> outGradeList = new List<string> { null, "PA", "FL", "MP", "PP" };
            int index = new Random().Next(outGradeList.Count);

            if (Outcome == null)
                OutGrade = null;
            else
                OutGrade = outGradeList[index];
        }
    }
}
