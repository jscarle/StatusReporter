namespace StatusReporter;

/// <summary>Interface for reporting the status of an application.</summary>
public interface IStatusReporter
{
    /// <summary>Gets the current status of the application.</summary>
    /// <returns>A <see cref="ApplicationStatus" /> object representing the current status of the application.</returns>
    public ApplicationStatus GetStatus();
}
