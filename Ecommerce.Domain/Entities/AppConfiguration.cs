using Ecommerce.Domain.Common;
using System.Xml.Linq;

namespace Ecommerce.Domain.Entities;
public class AppConfiguration : BaseEntity
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }

    //public AppConfiguration(string? key, string? value)
    //{
    //    Key = key ?? throw new ArgumentNullException(nameof(key), "Key cannot be null.");
    //    Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
    //}
}