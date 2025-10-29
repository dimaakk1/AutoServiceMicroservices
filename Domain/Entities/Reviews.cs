using Domain.Common;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Review : BaseEntity
    {
        public int CustomerId { get; private set; }
        public int OrderId { get; private set; }
        public Rating Rating { get; private set; }
        public string Comment { get; private set; }

        private Review() { }

        public Review(int customerId, int orderId, Rating rating, string comment)
        {
            if (customerId <= 0)
                throw new ArgumentException("CustomerId must be greater than 0.");
            if (orderId <= 0)
                throw new ArgumentException("OrderId must be greater than 0.");
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment cannot be empty.");

            CustomerId = customerId;
            OrderId = orderId;
            Rating = rating;
            Comment = comment;
        }
    }
}
