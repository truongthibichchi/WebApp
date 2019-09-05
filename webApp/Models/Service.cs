namespace webApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Service")]
    public partial class Service
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Service()
        {
            ServiceColumnDbMappings = new HashSet<ServiceColumnDbMapping>();
        }

        public int id { get; set; }

        [StringLength(200)]
        public string serviceName { get; set; }

        [StringLength(200)]
        public string wordTemplate { get; set; }

        public int? isActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ServiceColumnDbMapping> ServiceColumnDbMappings { get; set; }
    }
}
