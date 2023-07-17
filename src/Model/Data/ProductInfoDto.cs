namespace src.Model.Data;

public class ProductInfoDto{
  public string? ReferenceNumber {get; set;}
  public string? SalesOrderNumber {get; set;}
  public List<IFormFile>? Media {get; set;}
  public int ProductId {get; set;}
  public string? StoreName {get; set;}
  public string? Variation {get; set;}
  public DateTime Date {get; set;}
  public string? ProductDescription {get; set;}
}