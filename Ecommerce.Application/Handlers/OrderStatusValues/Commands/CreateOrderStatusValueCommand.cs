﻿using AutoMapper;
using Ecommerce.Application.Common;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using MediatR;

namespace Ecommerce.Application.Handlers.OrderStatusValues.Commands;

public class CreateOrderStatusValueCommand : IRequest<Response<string>>
{
    public string StatusValue { get; set; }
    public string Description { get; set; }
}
public class CreateOrderStatusValueCommandHandler : IRequestHandler<CreateOrderStatusValueCommand, Response<string>>
{
    private readonly IDataContext _db;
    private readonly IMapper _mapper;
    public CreateOrderStatusValueCommandHandler(IDataContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<Response<string>> Handle(CreateOrderStatusValueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var orderStatusValue = _mapper.Map<OrderStatusValue>(request);
            var addOrderStatusValue = await _db.OrderStatusValues.AddAsync(orderStatusValue);
            await _db.SaveChangesAsync(cancellationToken);
            return Response<string>.Success(orderStatusValue.StatusValue, "Successfully created");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Response<string>.Fail("Failed to add item!");
        }
    }
}
