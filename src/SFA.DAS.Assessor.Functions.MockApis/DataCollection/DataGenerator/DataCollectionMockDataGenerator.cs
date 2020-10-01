using Bogus;
using Bogus.Extensions.UnitedKingdom;
using CountryData.Bogus;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Assessor.Functions.MockApis.DataCollection.DataGenerator
{
    public class DataCollectionMockDataGenerator
    {
        protected DateTime _startDate { get; set; } = DateTime.Now.AddDays(-100);
        protected DateTime _endDate { get; set; } = DateTime.Now;
        protected int? _aimType { get; set; } = null;
        protected List<int> _fundModels { get; set; } = new List<int>();

        public DataCollectionMockDataGenerator(int? aimType, List<int> fundModels)
        {
            _aimType = aimType;
            _fundModels = fundModels;
        }
        
        public IEnumerable<DataCollectionLearner> GetLearners(int provider, int noOfLearners, int noOfLearningDeliveries)
        {
            var dataCollectionLearnerGenerator = new Faker<DataCollectionLearner>()
                .RuleFor(l => l.Ukprn, f => provider)
                .RuleFor(l => l.LearnRefNumber, f => f.Random.Long(1000000000, 9999999999).ToString())
                .RuleFor(l => l.Uln, f => provider + f.IndexVariable++)
                .RuleFor(l => l.FamilyName, f => f.Name.LastName())
                .RuleFor(l => l.GivenNames, f => f.Name.FirstName())
                .RuleFor(l => l.DateOfBirth, f => f.Person.DateOfBirth)
                .RuleFor(l => l.NiNumber, f => f.Finance.Nino().Replace(" ", ""))
                .RuleFor(l => l.LearningDeliveries, f =>
                    {
                        var learningDeliveries = GetLearningDeliveries(new Random().Next(1, 3), null).ToList();
                        learningDeliveries.AddRange(GetLearningDeliveries(noOfLearningDeliveries, _aimType).ToList());
                        return learningDeliveries;
                    });

            return dataCollectionLearnerGenerator.Generate(noOfLearners);
        }

        public IEnumerable<DataCollectionLearningDelivery> GetLearningDeliveries(int count, int? aimType = null)
        {
            List<int> aimTypelList = new List<int>() { 2, 3, 4 };
            List<int> fundingModelList = new List<int>() { 36, 81, 99 };
            List<int?> stdCodelList = new List<int?>() { 5, 26, 59, 93};
            List<string> epaOrgIDlList = new List<string>() { "EPA0002", "EPA0006", "EPA0011", "EPA0016", "EPA0026", "EPA0045", "EPA0060", "EPA0077", "EPA0110" };
            List<int> outcomeList = new List<int>() { 1, 3, 8 };
            List<int?> compStatusList = new List<int?>() { null, 1, 2, 3, 6 };
            var otherDataCollectionLearningDelivery = new OtherDataCollectionLearningDelivery();

            var dataCollectionLearningDeliveryGenerator = new Faker<DataCollectionLearningDelivery>()
                .RuleFor(ld => ld.AimType, f => aimType != null ? aimType : f.PickRandom(aimTypelList))
                .RuleFor(ld => ld.LearnStartDate, f => f.Date.Between(_startDate, _endDate))
                .RuleFor(ld => ld.LearnPlanEndDate, f => f.Date.Between(_endDate.AddMonths(3), _endDate.AddMonths(6)))
                .RuleFor(ld => ld.FundModel, f => f.PickRandom(fundingModelList))
                .RuleFor(ld => ld.StdCode, f => aimType != null ? f.PickRandom(stdCodelList) : (new Random().Next(0, 3) == 0 ? null : f.PickRandom(stdCodelList)))
                .RuleFor(ld => ld.DelLocPostCode, f => f.Country().UnitedKingdom().PostCode() + " " + f.Random.Int(0, 9) + f.Random.Word().Substring(0, 2).ToUpper())
                .RuleFor(ld => ld.EpaOrgID, f => new Random().Next(0, 3) > 0 ? null : f.PickRandom(epaOrgIDlList))
                .RuleFor(ld => ld.CompStatus, f => f.PickRandom(compStatusList))
                .RuleFor(ld => ld.LearnActEndDate, f => otherDataCollectionLearningDelivery.LearnActEndDate)
                .RuleFor(ld => ld.WithdrawReason, f => otherDataCollectionLearningDelivery.WithdrawReason)
                .RuleFor(ld => ld.Outcome, f => otherDataCollectionLearningDelivery.Outcome)
                .RuleFor(ld => ld.OutGrade, f => otherDataCollectionLearningDelivery.OutGrade);

            return dataCollectionLearningDeliveryGenerator.Generate(count);
        }
    }
}
