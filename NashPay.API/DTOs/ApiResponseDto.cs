namespace NashPay.API.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public Dictionary<string, string>? Errors { get; set; }

        public ApiResponseDto() 
        { 
            Errors = new Dictionary<string, string>();
        }

        public ApiResponseDto(bool success, string? message, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = new Dictionary<string, string>();
        }

        public ApiResponseDto(bool success, string? message, Dictionary<string, string>? errors)
        {
            Success = success;
            Message = message;
            Errors = errors ?? new Dictionary<string, string>();
        }
    }

    public class ApiResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, string>? Errors { get; set; }

        public ApiResponseDto() 
        { 
            Errors = new Dictionary<string, string>();
        }

        public ApiResponseDto(bool success, string? message)
        {
            Success = success;
            Message = message;
            Errors = new Dictionary<string, string>();
        }

        public ApiResponseDto(bool success, string? message, Dictionary<string, string>? errors)
        {
            Success = success;
            Message = message;
            Errors = errors ?? new Dictionary<string, string>();
        }
    }
}
