﻿namespace eCommerceWebAPI.ModelFromDB
{
    public class Favorite
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}