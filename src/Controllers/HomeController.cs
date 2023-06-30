using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using src.Model.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using src.Model.Data;
using src.Service;

[Controller]
public class HomeController : Controller{
  public HomeController(WarrantyrepoContext dbCtx){
    _dbContext = dbCtx;
  }
  private WarrantyrepoContext _dbContext {get; set;}
  public IActionResult Index(){
    ViewData["Title"] = "Warranty";
    return View();
  }
  [Route("product")]
  public async Task<IActionResult> GetProductInfo(){
    // maybe implement in Dapper
    List<Product> products = await _dbContext.Products.ToListAsync();
    List<Merchant> merchants = await _dbContext.Merchants.ToListAsync();
    ViewBag.ProductCount = products.Count;
    ViewBag.Merchants = merchants;
    ViewData["Title"] = "Product Info";
    return View(products);
  }
  [Route("user")]
  public async Task<IActionResult> GetUserInfo(ProductInfoDto productInfo){
    TempData.Put<ProductInfoDto>("product", productInfo);
    ViewData["Title"] = "User Info";
    return View();
  }
  [Route("finish")]
  public async Task<IActionResult> Finish(UserInfoDto userInfoDto){
    ProductInfoDto productInfo = TempData.Get<ProductInfoDto>("product");
    Entry entry = new Entry(){
      Id = IDGenerator.GenerateID("ENT"),
      FirstName = userInfoDto.FirstName,
      LastName = userInfoDto.LastName,
      ContactNumber = userInfoDto.ContactNumber,
      Email = userInfoDto.Email,
      ProductId = productInfo.ProductId,
      ReferenceNum = productInfo.ReferenceNumber,
      SalesOrderNum = productInfo.SalesOrderNumber,
      Variation = productInfo.Variation,
      Store = productInfo.StoreName,
      DatePurchased = DateOnly.FromDateTime(productInfo.Date),
      DateAdded = DateOnly.FromDateTime(DateTime.Now)
    };
    await _dbContext.Entries.AddAsync(entry);
    await _dbContext.SaveChangesAsync();
    ViewData["Title"] = "Request Submitted";
    return View(userInfoDto);
  }
}