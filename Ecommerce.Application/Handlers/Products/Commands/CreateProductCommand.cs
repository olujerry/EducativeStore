using AutoMapper;
using Ecommerce.Application.Common;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using MediatR;

namespace Ecommerce.Application.Handlers.Products.Commands;

public class CreateProductCommand : IRequest<Response<string>>
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? VariableTheme { get; set; }
    public int CategoryId { get; set; }
}
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Response<string>>
{
    private readonly IDataContext _db;
    private readonly IMapper _mapper;
    public CreateProductCommandHandler(IDataContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<Response<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = _mapper.Map<Product>(request);
            var addproduct = await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync(cancellationToken);
            return Response<string>.Success(product.Name, "Successfully added the product");
        }
        //catch (ValidationException e)
        //{
        //    Console.WriteLine(e);
        //    return Response<string>.Fail("Failed to add the product");
        //}
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Response<string>.Fail("Failed to add the product");
        }
    }
}
