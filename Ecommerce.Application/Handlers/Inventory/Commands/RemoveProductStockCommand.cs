using AutoMapper;
using Ecommerce.Application.Common;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using MediatR;

namespace Ecommerce.Application.Handlers.Inventory.Commands;

public class RemoveProductStockCommand : IRequest<Response<string>>
{
    public long VariantId { get; set; }
    public string Description { get; set; }
    public int Qty { get; set; }
}
public class RemoveProductStockCommandHandler : IRequestHandler<RemoveProductStockCommand, Response<string>>
{
    private readonly IDataContext _db;
    private readonly IMapper _mapper;
    public RemoveProductStockCommandHandler(IDataContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<Response<string>> Handle(RemoveProductStockCommand request, CancellationToken cancellationToken)
    {
        var getItem = await _db.Variants.FindAsync(request.VariantId);

        using var transaction = _db.BeginTransaction();
        try
        {
            getItem.Qty -= request.Qty;

            _db.Variants.Update(getItem);

            Stock stock = new Stock()
            {
                VariantId = getItem.Id,
                StockInputType = StockInputType.Deduction,
                Qty = request.Qty,
                Description = request.Description,
            };

            _db.Stocks.Add(stock);
            await _db.SaveChangesAsync();
            transaction.Commit();

            return Response<string>.Success(getItem.Qty.ToString(), "Successfully updated the category");
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            return Response<string>.Fail("Failed to add the category");
        }

    }
}
