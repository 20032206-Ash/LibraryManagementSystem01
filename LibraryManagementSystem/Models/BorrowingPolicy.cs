using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BorrowingPolicy
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 60)]
        public int LoanDurationDays { get; set; }

        [Required]
        [Range(0, 10)]
        public int MaxRenewals { get; set; }

        [Required]
        [Range(1, 20)]
        public int MaxBooksPerMember { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal FinePerDay { get; set; }
    }
}