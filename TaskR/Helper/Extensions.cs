namespace TaskR.Helper
{
    public static class DateTimeExtensions
    {
        public static string ToInputString(this DateTime? dateTime) 
        { 
            if (!dateTime.HasValue) { return String.Empty; }
            return dateTime.Value.ToString("yyyy-MM-dd HH:mm:ss").Replace(' ', 'T');
        }
    }
}
