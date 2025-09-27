using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PersonalFinanceTrackerAPI.Models;

[Table("transactions")]
public partial class Transaction
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("userid")]
    public Guid? Userid { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("category")]
    [StringLength(50)]
    public string Category { get; set; } = null!;

    [Column("type")]
    [StringLength(10)]
    public string Type { get; set; } = null!;

    [Column("note")]
    public string? Note { get; set; }

    [Column("createdat", TypeName = "timestamp with time zone")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Transactions")]
    public virtual User? User { get; set; }
}
