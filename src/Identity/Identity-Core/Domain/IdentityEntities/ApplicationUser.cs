using Microsoft.AspNetCore.Identity;

namespace Identity_Core.Domain.IdentityEntities
{
 public class ApplicationUser : IdentityUser<Guid>
 {
  public string? PersonName { get; set; }
  
  public string? AvatarPath { get; set; }

 }
}
