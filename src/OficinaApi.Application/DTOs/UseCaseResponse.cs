using System;

namespace OficinaApi.Application.DTOs
{
    public class UseCaseResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string[] Messages { get; set; } = Array.Empty<string>();
        public T Response { get; set; } = default!;

        public UseCaseResponse() { }

        public UseCaseResponse(bool isSuccess, string[] messages, T response)
        {
            IsSuccess = isSuccess;
            Messages = messages;
            Response = response;
        }

        public static UseCaseResponse<T> Success(T response)
        {
            return new UseCaseResponse<T>(true, Array.Empty<string>(), response);
        }

        public static UseCaseResponse<T> Failure(params string[] messages)
        {
            return new UseCaseResponse<T>(false, messages, default!);
        }
    }
}
