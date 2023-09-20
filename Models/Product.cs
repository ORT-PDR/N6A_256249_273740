using System;
using System.Drawing;

namespace Models
{
	public class Product
	{
		public Guid id;
		public string name;
		public string description;
		public int stock;
		public double price;
		public string imagePath;
		public string creator;
		public List<Review> reviews;
	}
}
