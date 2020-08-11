using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerHost.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public virtual DbSet<KeyValue> Values { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
	}

	public class KeyValue
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Key { get; set; }

		public string Value { get; set; }
	}
}
