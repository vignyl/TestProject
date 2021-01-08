namespace TestProject.Models.FirstModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial; 

    [Table("User")]
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            Payment = new HashSet<Payment>();
        } 

        [Column(TypeName = "numeric")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal id { get; set; }
         
        [StringLength(254)]
        public string full_name { get; set; }

        [Required]
        [StringLength(254)]
        public string email { get; set; }

        [Required]
        [StringLength(100)]
        public string password { get; set; }

        [NotMapped] 
        public bool status { get; set; }

        [NotMapped] 
        public bool isSecond { get; set; }

        [NotMapped]
        public decimal reduction { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payment> Payment { get; set; }
    }
}
