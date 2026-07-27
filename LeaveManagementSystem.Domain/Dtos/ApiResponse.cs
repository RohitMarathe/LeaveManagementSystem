namespace LeaveManagementSystem.Domain.Dtos
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }

        public string Message { get; init; } = string.Empty;

        public T? Data { get; init; }
    }
}
