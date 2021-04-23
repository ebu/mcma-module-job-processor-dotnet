namespace Mcma.Modules.JobProcessor.Worker.Requests
{
    internal class JobFailure : JobReference
    {
        public ProblemDetail Error { get; set; }
    }
}