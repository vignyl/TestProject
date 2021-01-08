namespace TestProject.Models.FirstModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Payment")]
    public partial class Payment
    {
        [Column(TypeName = "numeric")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal id { get; set; }

        [Column(TypeName = "numeric")]
        public decimal payer_id { get; set; }
         
        [StringLength(254)]
        public string code { get; set; }

        public decimal amount { get; set; }

        public decimal original_amount { get; set; }

        [Column(TypeName = "text")]
        public string description { get; set; }

        public virtual User User { get; set; }
    }
}
