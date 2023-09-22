namespace Ecommerce.Application.Dto;

public class CustomerDto
{
    public long Id { get; set; }
    public string? UserFirstName { get; set; }
    public string? UserLastName { get; set; }
    public string? UserFullName { get; set; }
    public string? UserProfilePicture { get; set; }
    public string? UserPhoneNumber { get; set; }
    public string? UserEmail { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? UserGender { get; set; }
    public DateTime? CreatedDate { get; set; }
}
