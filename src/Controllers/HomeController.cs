using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using src.Model.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using src.Model.Data;
using src.Service;
using Amazon.S3;
using Amazon.S3.Model;

[Controller]
public class HomeController : Controller{
  public HomeController(WarrantyrepoContext dbCtx){
    _dbContext = dbCtx;
  }
  private string _directory = @"C:\ApplicationData\WarrantyManagement\Temp\Uploads";
  private WarrantyrepoContext _dbContext {get; set;}
  public IActionResult Index(){
    ViewData["Title"] = "Warranty";
    return View();
  }
  [Route("user")]
  public async Task<IActionResult> GetUserInfo(){
    ViewData["Title"] = "User Info";
    return View();
  }
  [Route("product")]
  public async Task<IActionResult> GetProductInfo(UserInfoDto userInfo){
    // maybe implement in Dapper
    TempData.Put<UserInfoDto>("userInfo", userInfo);
    List<Product> products = await _dbContext.Products.ToListAsync();
    List<Merchant> merchants = await _dbContext.Merchants.ToListAsync();
    ViewBag.ProductCount = products.Count;
    ViewBag.Merchants = merchants;
    ViewData["Title"] = "Product Info";
    return View(products);
  }
  [Route("finish")]
  public async Task<IActionResult> Finish(ProductInfoDto productInfo){
    UserInfoDto userInfo = TempData.Get<UserInfoDto>("userInfo");
    string entryId = IDGenerator.GenerateID("ENT");
    Entry entry = new Entry(){
      Id = entryId,
      FirstName = userInfo.FirstName,
      LastName = userInfo.LastName,
      ContactNumber = userInfo.ContactNumber,  
      Email = userInfo.Email,
      ProductId = productInfo.ProductId,
      ReferenceNum = productInfo.ReferenceNumber,
      SalesOrderNum = productInfo.SalesOrderNumber,
      Variation = productInfo.Variation,
      Store = productInfo.StoreName,
      Status = "UNRESOLVED",
      Description = productInfo.ProductDescription,
      DatePurchased = DateOnly.FromDateTime(productInfo.Date),
      DateAdded = DateOnly.FromDateTime(DateTime.Now)
    };
    // save media here

    var client = new AmazonS3Client();
    string bucketName = "warranty-media";
    int mediaCount = 1;
    foreach(FormFile file in productInfo.Media){
      var objectRequest = new PutObjectRequest(){
        BucketName = bucketName,
        Key = $"{entryId}/{mediaCount}",
        InputStream= file.OpenReadStream()
      };
      var response = await client.PutObjectAsync(objectRequest);  
      mediaCount += 1;
    }
    await _dbContext.Entries.AddAsync(entry);
    await _dbContext.SaveChangesAsync();
    ViewData["Title"] = "Request Submitted";
    return View(userInfo);
  }
}