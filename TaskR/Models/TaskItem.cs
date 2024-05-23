namespace TaskR.Data
{
    public partial class TaskItem
    {
        public bool IsUrgent ()
        {
            if (IsCompleted)
                return false;
            if (Deadline is null)
                return false;
            return DateTime.Now >= Deadline.Value.AddDays(-7);
            //return Random.Shared.NextSingle() > 0.5;
        }
    }
}
