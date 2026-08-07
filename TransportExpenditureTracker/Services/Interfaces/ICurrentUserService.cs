namespace TransportExpenditureTracker.Services.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }

    bool IsAdmin { get; }
}