using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
namespace MvcLayer.Models
{
    public class PrepaymentsTakeAddViewModel
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public DateTime? Period { get; set; }

        public DateTime? DateTransfer { get; set; }

        public decimal? Total { get; set; }

        public Boolean? IsTarget { get; set; }

        public Boolean? IsRefund { get; set; }

        public int? PrepaymentId { get; set; }

        public IFormFileCollection? FileEntity { get; set; }
    }
}
