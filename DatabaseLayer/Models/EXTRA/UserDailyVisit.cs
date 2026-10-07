namespace DatabaseLayer.Models.EXTRA
{
    public class UserDailyVisit
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public string UserName { get; set; }
        public DateOnly Date { get; set; }
        public DateTime FirstVisitUtc { get; set; }
        public DateTime LastVisitUtc { get; set; }
    }
}