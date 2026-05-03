using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public enum ReservationStatus
    {
        Waiting,
        ReadyForPickup,
        Cancelled
    }

    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public string MemberId { get; set; } = "";

        [Required]
        public int BookId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public ReservationStatus Status { get; set; }

        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }
    }
}