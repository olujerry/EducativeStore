using AutoMapper;
using Ecommerce.Application.Common;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using MediatR;

namespace Ecommerce.Application.Handlers.DeliveryMethods.Commands;

public class CreateDeliveryMethodCommand : IRequest<Response<string>>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}
public class CreateDeliveryMethodCommandHandler : IRequestHandler<CreateDeliveryMethodCommand, Response<string>>
{
    private readonly IDataContext _db;
    private readonly IMapper _mapper;
    public CreateDeliveryMethodCommandHandler(IDataContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<Response<string>> Handle(CreateDeliveryMethodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var DeliveryMethod = _mapper.Map<DeliveryMethod>(request);
            var addDeliveryMethod = await _db.DeliveryMethods.AddAsync(DeliveryMethod);
            await _db.SaveChangesAsync(cancellationToken);
            return Response<string>.Success(DeliveryMethod.Name, "Successfully created");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Response<string>.Fail("Failed to add item!");
        }
    }
}
