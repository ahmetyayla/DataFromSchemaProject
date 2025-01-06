# Varlıklarımız
```
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public List<Review> Reviews { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Review
{
    public int Id { get; set; }
    public string Comment { get; set; }
    public int Rating { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
}
```
# Örnek veri
```
        var category = new Category { Id = 1, Name = "Electronics" };
        context.Categories.Add(category);
        context.Products.Add(new Product
        {
            Id = 1,
            Name = "Laptop",
            Category = category,
            Reviews = new List<Review>
            {
                new Review { Id = 1, Comment = "Great!" },
                new Review { Id = 2, Comment = "Good value for money." }
            }
        });
        await context.SaveChangesAsync();

        // JSON formatındaki kolon seçim tanımı
        string jsonColumns = @"
        {
            ""Product"": [""Id"", ""Name"", ""Reviews:[Id,Comment,Product.Name]"", ""Category.Name""]
        }";
```
# Kullanımı
```
    List<Product> productList=context.Products.All();
    List<ExpandoObject> dataList= productList.CreateDynamicObjectByStringSchema(jsonColumns);
veya
    Product selectedProduct=context.Products.getById(5);
    ExpandoObject data= selectedProduct.CreateDynamicObjectByStringSchema(jsonColumns);
```        

