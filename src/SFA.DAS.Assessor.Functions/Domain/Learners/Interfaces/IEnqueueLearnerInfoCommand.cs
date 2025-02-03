namespace SFA.DAS.Assessor.Functions.Domain.Learners.Interfaces
{
    public interface IEnqueueLearnerInfoCommand
    {
        Task Execute(string message);
    }
}