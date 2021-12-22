using System;

namespace SFA.DAS.Assessor.Functions.Domain.Learners.Types
{
    public class UpdateLearnersInfoMessage : IEquatable<UpdateLearnersInfoMessage>
    {
        public UpdateLearnersInfoMessage(long employerAccountId, string employerName, long uln, int stdCode)
        {
            EmployerAccountId = employerAccountId;
            EmployerName = employerName;
            Uln = uln;
            StdCode = stdCode;
        }

        public long EmployerAccountId { get; set; }
        public string EmployerName { get; set; }
        public long Uln { get; set; }
        public int StdCode { get; }

        public override int GetHashCode() =>
            (EmployerAccountId, EmployerName,Uln, StdCode).GetHashCode();

        public override bool Equals(object obj) => 
            obj is UpdateLearnersInfoMessage other && Equals(other);
        
        public bool Equals(UpdateLearnersInfoMessage other) =>
            (EmployerAccountId, EmployerName, Uln, StdCode).Equals((other.EmployerAccountId, other.EmployerName, other.Uln, other.StdCode));
    }
}
