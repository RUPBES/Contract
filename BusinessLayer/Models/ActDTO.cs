namespace BusinessLayer.Models
{
    public class ActDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// причина
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// дата акта
        /// </summary>
        public DateTime? DateAct { get; set; }

        /// <summary>
        /// приостановлено с
        /// </summary>
        public DateTime? DateSuspendedFrom { get; set; }

        /// <summary>
        /// приостановлено по
        /// </summary>
        public DateTime? DateSuspendedUntil { get; set; }

        /// <summary>
        /// дата возобновления
        /// </summary>
        public DateTime? DateRenewal { get; set; }

        /// <summary>
        /// приостановлено?
        /// </summary>
        public bool? IsSuspension { get; set; }

        public int? ContractId { get; set; }

        public ContractDTO? Contract { get; set; }

        public List<FileDTO> Files { get; set; } = new List<FileDTO>();
    }
}
