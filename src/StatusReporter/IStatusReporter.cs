namespace StatusReporter;

/// <summary>Interface for reporting the status of a server.</summary>
public interface IStatusReporter
{
    /// <summary>Gets the current status of the server.</summary>
    /// <returns>A <see cref="ApplicationStatus" /> object representing the current status of the server.</returns>
    public ApplicationStatus GetStatus();
}
