namespace OficinaApi.Domain.Enums;

public enum OrderStatus
{
    Received = 0,
    InDiagnostics = 1,
    WaitingApproval = 2,
    Executing = 3,
    Finished = 4,
    Delivered = 5,
    Refused = 6
}
