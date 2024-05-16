namespace TaskR.Data
{
    public partial class Task
    {
        public bool IsUrgent ()
        {
            //Todo 
            return Random.Shared.NextSingle() > 0.5;
        }
    }
}
