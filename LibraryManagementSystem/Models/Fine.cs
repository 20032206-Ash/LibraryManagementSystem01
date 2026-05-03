using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public enum FineStatus
    {
        Unpaid,
        Paid
    }

    public class Fine
    {
        public int Id { get; set; }

        [Required]
        public string MemberId { get; set; } = "";

        [Required]
        public int TransactionId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        [Required]
        public FineStatus Status { get; set; }

        [ForeignKey("TransactionId")]
        public virtual BorrowingTransaction? BorrowingTransaction { get; set; }
    }
}