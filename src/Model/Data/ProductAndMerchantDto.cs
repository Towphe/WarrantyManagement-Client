using src.Model.Repo;

namespace src.Model.Data;

public class ProductAndMerchantDto{
  public List<Merchant> Merchants {get; set;}
  public List<Product> Products {get; set;}
}