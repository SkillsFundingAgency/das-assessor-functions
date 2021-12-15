using System;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Types
{
    public class UpdateLearnersInfoMessage : IEquatable<UpdateLearnersInfoMessage>
    {
        public UpdateLearnersInfoMessage(long employerAccountId, string employerName)
        {
            EmployerAccountId = employerAccountId;
            EmployerName = employerName;
        }

        public long EmployerAccountId { get; set; }
        public string EmployerName { get; set; }

        public override int GetHashCode() =>
            (EmployerAccountId, EmployerName).GetHashCode();

        public override bool Equals(object obj) => 
            obj is UpdateLearnersInfoMessage other && Equals(other);
        
        public bool Equals(UpdateLearnersInfoMessage other) =>
            (EmployerAccountId, EmployerName).Equals((other.EmployerAccountId, other.EmployerName));
    }
}
