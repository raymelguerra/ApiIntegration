using Domain.Enums;

namespace Domain.Extensions
{
    public static class JobTypeExtensions
    {
        public static string ToJobKey(this JobType jobType) => jobType.ToString();
        
        public static bool IsValidJobKey(this string? jobKey)
        {
            if (string.IsNullOrWhiteSpace(jobKey)) return false;
            return Enum.TryParse<JobType>(jobKey.Trim(), ignoreCase: true, out _);
        }
        
        public static bool TryParseJobKey(this string? jobKey, out JobType result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(jobKey)) return false;
            return Enum.TryParse<JobType>(jobKey.Trim(), ignoreCase: true, out result);
        }
        
    }
}