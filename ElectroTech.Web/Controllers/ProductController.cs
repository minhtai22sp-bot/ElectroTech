using ElectroTech.Application.Features.Products.Queries;
using ElectroTech.Web.Abstractions;
using Entities.ViewModel;
using Library;
using Microsoft.AspNetCore.Mvc;

namespace ElectroTech.Web.Controllers;

public class ProductsController : BaseController<ProductsController>
{

    public async Task<IActionResult> Index(ProductSearch model)
    {
        model.pageSize = 12;
        model.currentPage = model.currentPage > 0 ? model.currentPage : 1;
        model.skip = (model.currentPage - 1) * model.pageSize;

        var response = await _mediator.Send(new GetAllPaginatedListQuery
        {
            model = model
        });

        if (response.Succeeded)
            return View(response.Data);

       
        return View(new PaginatedList<ProductIndexModel>(
            new List<ProductIndexModel>(), // items
            0,                            // count
            1,                            // currentPage
            model.pageSize                // pageSize
        ));
    }


    public async Task<IActionResult> Detail(int id)
    {
        var response = await _mediator.Send(new GetIdProductQuery
        {
            Id = id
        });

        if (!response.Succeeded || response.Data is null)
            return NotFound();

        return View("Details", response.Data);
    }
}