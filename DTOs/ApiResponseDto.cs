namespace YopoAPI.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Status { get; set; } = "OK";
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string RequestId { get; set; } = "";
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static new ApiResponse<T> Success<T>(T data, string message = "Request successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Status = "OK",
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Error<T>(string message, List<string>? errors = null, string status = "ERROR")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Status = status,
                Message = message,
                Data = default(T),
                Errors = errors ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static new ApiResponse<object> Success(string message = "Request successful")
        {
            return new ApiResponse<object>
            {
                Success = true,
                Status = "OK",
                Message = message,
                Data = null,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<object> Error(string message, List<string>? errors = null, string status = "ERROR")
        {
            return new ApiResponse<object>
            {
                Success = false,
                Status = status,
                Message = message,
                Data = null,
                Errors = errors ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public static class ApiResponseExtensions
    {
        public static ApiResponse<T> ToApiResponse<T>(this T data, string message = "Request successful")
        {
            return ApiResponse.Success(data, message);
        }

        public static ApiResponse<object> ToApiResponse(this string message)
        {
            return ApiResponse.Success(message);
        }
    }
}
