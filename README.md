# Expression Helper

Expression helper, translates your written rules into a dynamic query.

# Usage

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    
        public Product(int id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }
    }

Let's assume you have a product model.

    var products = new List<Product>
    {
        new Product(1, "Keyboard", 500),
        new Product(2, "Mouse", 700),
        new Product(3, "Ram", 2000),
        new Product(4, "Speaker", 1000),
        new Product(5, "Monitor", 3000)
    };

Let's create a filtering model to generate dynamic queries for your product listings.

    List<FilterQuery> rules = new List<FilterQuery>()
    {
        new FilterQuery()
        {
            Column = "Name",
            Condition = "Keyboard",
            Relation = RuleRelation.Contains,
            Statement = "Or"
        },
        new FilterQuery()
        {
            Column = "Name",
            Condition = "Mouse",
            Relation = RuleRelation.Contains,
            Statement = "Or"
        },
        new FilterQuery()
        {
            Column = "Price",
            Condition = "1000",
            Relation = RuleRelation.LessThan,
            Statement = "And"
        }
    };

Let's send this model to our function that generates dynamic queries.

    var filter = ExpressionHelper.BuildDynamicFilter<Product>(rules);
The result of the "filter" variable is:

    x => x.Name.Contains("Keyboard") || x.Name.Contains("Mouse") && x.Price <= 1000
    
Summary:

    var filter = ExpressionHelper.BuildDynamicFilter<Product>(rules);
    var results = queryable.Where(filter).ToList();
Result:

    [
	    {
		    "id":  1,
		    "name":  "Keyboard",
		    "price":  500
	    },
	    {
		    "id":  2,
		    "name":  "Mouse",
		    "price":  700
	    }
    ]

