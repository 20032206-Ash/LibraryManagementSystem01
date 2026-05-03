using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public enum BorrowingStatus
    {
        Active,
        Returned,
        Overdue,
        Reserved
    }

    public class BorrowingTransaction
    {
        public int Id { get; set; }

        [Required]
        public string MemberId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [Required]
        public BorrowingStatus Status { get; set; }

        public int RenewalCount { get; set; }

        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }
    }
}