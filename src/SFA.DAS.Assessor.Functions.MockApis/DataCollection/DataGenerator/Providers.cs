using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.MockApis.DataCollection.DataGenerator
{
    public static class Providers
    {
        static Providers()
        {
            for(int count = 0; count < 5000; count++)
            {
                ProvidersList.Add(10000000 + (count * 500));
            }
        }

        public static List<int> ProvidersList = new List<int>();
    }
}
