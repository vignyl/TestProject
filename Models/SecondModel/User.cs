namespace TestProject.Models.SecondModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("User")]
    public partial class User
    {
        [Column(TypeName = "numeric")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal id { get; set; }

        [Required]
        [StringLength(254)]
        public string full_name { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string code { get; set; }

        [Required]
        [StringLength(100)]
        public string password { get; set; }

        public bool status { get; set; }

        [Column(TypeName = "numeric")] 
        public decimal reduction { get; set; }
    }
}
