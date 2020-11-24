namespace SFA.DAS.Assessor.Functions.ExternalApis.SecureMessage.Types
{
    public class CreateMessageResponse
    {
        public string Key { get; set; }
        public CreateMessageResponseLinks Links { get; set; }
    }

    public class CreateMessageResponseLinks
    {
        public string Api { get; set; }
        public string Web { get; set; }
    }
}
