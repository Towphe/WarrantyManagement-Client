using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using src.Model.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using src.Model.Data;
using src.Service;
using Amazon.SimpleEmail;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleEmail.Model;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

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
  [Route("user")]
  public async Task<IActionResult> GetUserInfo(){
    ViewData["Title"] = "User Info";
    return View();
  }
  [Route("product")]
  public async Task<IActionResult> GetProductInfo(UserInfoDto userInfo){
    // implement email validation

    // implement phone validation
    
    TempData.Put<UserInfoDto>("userInfo", userInfo);
    List<Product> products = await _dbContext.Products.ToListAsync();
    List<Merchant> merchants = await _dbContext.Merchants.ToListAsync();
    ViewBag.ProductCount = products.Count;
    ViewBag.Merchants = merchants;
    ViewData["Title"] = "Product Info";
    return View(products);
  }
  [Route("review")]
  public async Task<IActionResult> Review(ProductInfoDto productInfo){
    TempData.Put<string>("productName", _dbContext.Products.Find(productInfo.ProductId).Name);
    TempData.Put<string>("tempId", IDGenerator.GenerateID("ENT"));
    return View(productInfo);
  }
  [Route("finish")]
  public async Task<IActionResult> Finish(UserInfoDto userInfo, ProductInfoDto productInfo, string TempId, string MediaJSON){
    Entry entry = new Entry(){
      Id = TempId,
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
    Console.WriteLine(MediaJSON);
    await _dbContext.Entries.AddAsync(entry);
    await _dbContext.SaveChangesAsync();
    ViewData["Title"] = "Request Submitted";
    return View(userInfo);
  }
  // implement for media upload:
  [Route("media-upload/{entryId}")]
  [HttpPost]
  //[Consumes("image/png", "image/jpg")]
  public async Task<HttpStatusCode> UploadMedia([FromRoute] string entryId){
    // decode image base 64 -> upload to s3
    var client = new AmazonS3Client();
    Console.WriteLine($"Id: {entryId}");
    string bucketName = "warranty-media";
    int mediaCount = 1;
    string bodyStr = "";
    var req = HttpContext.Request;
    req.EnableBuffering();
    using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true)){
      bodyStr = await reader.ReadToEndAsync();
    }
    req.Body.Position = 0;
    List<string>? mediaURIsRaw = System.Text.Json.JsonSerializer.Deserialize<List<string>>(bodyStr);
    List<string>? mediaURIs = new List<string>();
    foreach(string mediaURI in mediaURIsRaw){
      mediaURIs.Add(mediaURI.Split(',')[1]);
    }
    // convert base64 string to img
    foreach(string mediaURI in mediaURIs){
      byte[] bytes = Convert.FromBase64String(mediaURI);
      using (MemoryStream ms = new MemoryStream(bytes)){
        var objectRequest = new PutObjectRequest(){
          BucketName = bucketName,
          Key = $"{entryId}/{mediaCount}",
          InputStream= ToStream(Image.FromStream(ms), ImageFormat.Jpeg)
        };
        var response = await client.PutObjectAsync(objectRequest);  
      }
      
      mediaCount += 1;
    }
    return HttpStatusCode.OK;
  }
  private static Stream ToStream(Image image, ImageFormat format){
    var stream = new System.IO.MemoryStream();
    image.Save(stream,format);
    stream.Position = 0;
    return stream;
  }
}