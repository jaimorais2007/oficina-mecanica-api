namespace OficinaApi.Application.DTOs;

public record class ServiceOrderAlertDto(string Message, bool concluded, DateTime CreatedAt);
    