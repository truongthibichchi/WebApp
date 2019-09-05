namespace webApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ServiceColumnDbMapping")]
    public partial class ServiceColumnDbMapping
    {
        public int id { get; set; }

        public int serviceId { get; set; }

        [Required]
        public string csvColumnName { get; set; }

        [Required]
        public string dbColumnName { get; set; }

        public int? length { get; set; }

        public virtual Service Service { get; set; }
    }
}
