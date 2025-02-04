﻿using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using WebbShop.Model;

namespace WebbShop
{
    internal class AdminPage
    {

        public static async Task WriteAdminPage()
        {
            while (DataTracker.GetIsAdmin() == true)
            {
                Console.Clear();
                List<string> AdminBox = new List<string>();
                List<string> options = new List<string>();

                for (int i = 0; i < 15; i++)
                {
                    AdminBox.Add("                                                                                            ");
                }

                options.Add("[1] Change Start Page");
                options.Add("[2] Add new products");
                options.Add("[3] Purchase History");
                options.Add("[4] Handle Products");
                options.Add("[5] Querys");
                options.Add("[ESC] Logg Out");

                var AdminWindow = new Window("", 5, 2, AdminBox);
                AdminWindow.Draw();

                var AdminOptions = new Window("Options", 6, 4, options);
                AdminOptions.Draw();

                ConsoleKeyInfo keyInfo = Console.ReadKey();
                switch (keyInfo.Key)
                {
                    case ConsoleKey.D1:
                        {
                            List<string> options2 = new List<string>();

                            Console.Clear();

                            AdminWindow.Draw();

                            options2.Add("[1] Change Product 1");
                            options2.Add("[2] Change Product 2");
                            options2.Add("[3] Change Product 3");
                            options2.Add("[4] Change Product 4");
                            options2.Add("[B]ack");
                            var optionBox = new Window("Options", 6, 4, options2);
                            AdminOptions.Draw();


                            Helpers.Product1();
                            Helpers.Product2();
                            Helpers.Product3();
                            Helpers.Product4();


                            ConsoleKeyInfo keyInfo2 = Console.ReadKey();

                            if (keyInfo2.Key == ConsoleKey.D1)
                            {
                                DataTracker.SetChangeProduct(0);
                            }
                            else if (keyInfo2.Key == ConsoleKey.D2)
                            {
                                DataTracker.SetChangeProduct(1);
                            }
                            else if (keyInfo2.Key == ConsoleKey.D3)
                            {
                                DataTracker.SetChangeProduct(2);

                            }
                            else if (keyInfo2.Key == ConsoleKey.D4)
                            {
                                DataTracker.SetChangeProduct(3);
                            }
                            else if (keyInfo2.Key == ConsoleKey.B)
                            {
                                break;
                            }
                            Console.Clear();
                            DataTracker.SetRunPage(true);
                            AdminWindow.Draw();
                            WriteAllPages.WriteOutPages();

                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            Console.Clear();
                            AdminWindow.Draw();
                            List<string> newGroupId = new List<string>();
                            newGroupId.Add("Would you like to create a new GroupId or add a new product in a existence GroupId.");
                            newGroupId.Add("");
                            newGroupId.Add("    [C]reate new GroupId & create new product          [A]dd product to GroupId    ");

                            var groupIdCheckBox = new Window("", 6, 7, newGroupId);
                            groupIdCheckBox.Draw();


                            ConsoleKeyInfo key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.C)
                            {
                                AdminTools.AddNewProduct();
                            }

                            else if (key.Key == ConsoleKey.A)
                            {
                                AdminTools.AddNewCopy();
                            }
                            break;
                        }

                    case ConsoleKey.D3:
                        {
                            DataTracker.SetRunPage(true);
                            ReceiptPages.WriteAllReceipt();
                            break;
                        }
                    case ConsoleKey.D4:
                        {
                            ChangeProductInfomation();
                            break;
                        }
                    case ConsoleKey.D5:
                        {
                            Console.Clear();
                            AdminWindow.Draw();
                            options.Clear();

                            using (var myDb = new MyDbContext())
                            {
                                var startDate = DateTime.Now.AddMonths(-1);
                                var endDate = DateTime.Now;
                                var totalSales = myDb.shopingCart
                                    .Where(o => o.DateWhenBought >= startDate && o.DateWhenBought <= endDate)
                                    .Sum(o => o.Antal);


                                var totalProductsInStock = myDb.stocks
                                                .Sum(p => p.StockCount);

                                var top5bought = myDb.shopingCart
                                                .Where(o => o.CompletedPurchase == true)
                                                .Take(5).Join(myDb.products, cartItem => cartItem.ProductId,
                                                product => product.Id,
                                                (cartItem, product) => new
                                                {
                                                    productName = product.ProductName,
                                                    QuantityBought = cartItem.Antal

                                                }).ToList();

                                foreach (var product in top5bought)
                                {
                                    options.Add("Top products solds " + product.productName + " " + product.QuantityBought);
                                }


                                options.Add("The amount of products sold in a month " + totalSales);
                                options.Add("");
                                options.Add("The amount of products in stock: " + totalProductsInStock);

                                options.Add("");

                                options.Add("[1] Remove GuestUsers and Carts (every 24 Hours)");
                                options.Add("[B]ack");

                                ConsoleKeyInfo key = Console.ReadKey(true);

                                if (key.Key == ConsoleKey.D1)
                                {
                                    var oneDayAgo = DateTime.Now.AddDays(-1);

                                    var usersToDelete = myDb.users
                                        .Where(u => u.Name.Contains("GuestUser") &&
                                                    u.Age <= oneDayAgo && // User is at least 24 hours old
                                                    myDb.shopingCart.Any(sc => sc.CompletedPurchase == false))
                                        .ToList();


                                    var cartsToDelete = myDb.shopingCart
                                                    .Where(c => c.CompletedPurchase == false &&
                                                     myDb.users.Any(a => a.Age <= oneDayAgo))
                                                    .ToList();

                                    myDb.users.RemoveRange(usersToDelete);
                                    myDb.shopingCart.RemoveRange(cartsToDelete);
                                    myDb.SaveChanges();
                                }
                            }




                            break;
                        }
                    case ConsoleKey.Escape:
                        {
                            DataTracker.SetIsAdmin(false);
                            DataTracker.SetUserId(0);
                            break;
                        }
                }


            }




        }




        public static void ChangeProductInfomation()
        {
            List<string> windows = new List<string>();
            var AdminWindow = new Window("", 5, 2, windows);
            DataTracker.SetAddProduct(true);
            string connectionString = DataTracker.GetConnectionString();
            DataTracker.SetRunPage(true);

            WriteAllPages.WriteOutPages();
            int pointer = 0;

            using (var myDb = new MyDbContext())
            {
                var everyProduct = myDb.products.Where(i => i.ProductGroup == DataTracker.GetProductId()).ToList();
                var productInfo = everyProduct.FirstOrDefault();

                int productGroup = productInfo.ProductGroup;
                //---------------------------------------------------------
                var color = myDb.colors
                        .Select(c => new { c.Id, c.Name })
                        .ToList();

                var result = productInfo.ColorId.Join(
                        color,
                        id => id,
                        color => color.Id,
                        (id, color) => color.Name
                        ).ToList(); //lista är viktigt för rotera!!
                //---------------------------------------------------------

                var everySize = everyProduct.Select(i => i.Size).ToList();

                //----------------------------------------------

                string colorText = "Available colors: ";
                string lastColor = result.Last();

                string sizeText = "Available Sizes: ";
                string lastSize = everySize.Last();


                foreach (var sizeGroup in everySize)
                {
                    if (sizeGroup != lastSize)
                    {
                        sizeText += sizeGroup + ", ";
                    }
                    else
                    {
                        sizeText += sizeGroup;
                    }
                }
                foreach (var colors in result)
                {
                    if (colors != lastColor)
                    {
                        colorText += colors + ", ";
                    }
                    else
                    {
                        colorText += colors;
                    }
                }

                var brandName = (from p in myDb.products
                                 join b in myDb.brands
                                 on p.Brand equals b.Id
                                 where p.Id == DataTracker.GetProductId()
                                 select b.Name)
                                 .FirstOrDefault();

                string[] infoTitle = { "Product Name", "Gender", "Description", "Environment Friendly", "Product Price" };

                while (DataTracker.GetAddProduct() == true)
                {
                    Console.Clear();
                    windows.Add("                            ");
                    //---------------------------------------------
                    if (pointer == 0) // productName
                    {
                        windows.Add("Product Name: " + productInfo.ProductName + " <-");  //name, brand, category, Gender, description, enviroment, price, CanBeBought
                    }
                    else
                    {
                        windows.Add("Product Name: " + productInfo.ProductName);
                    }
                    if (pointer == 1) //Product Brand
                    {
                        windows.Add("Product Brand: " + brandName + " <-");
                    }
                    else
                    {
                        windows.Add("Product Brand: " + brandName);
                    }
                    if (pointer == 2) //Product Category
                    {
                        windows.Add("Product Category: " + productInfo.Category + " <-");
                    }
                    else
                    {
                        windows.Add("Product Category: " + productInfo.Category);
                    }
                    if (pointer == 3) //Product Gender
                    {
                        windows.Add("Product Gender: " + productInfo.Gender + " <-");
                    }
                    else
                    {
                        windows.Add("Product Gender: " + productInfo.Gender);
                    }
                    if (pointer == 4) //Product Size
                    {
                        windows.Add(sizeText + " <-");
                    }
                    else
                    {
                        windows.Add(sizeText);
                    }
                    if (pointer == 5) //Product Colors
                    {
                        windows.Add(colorText + " <-");
                    }
                    else
                    {
                        windows.Add(colorText);
                    }
                    if (pointer == 6) // product describtion
                    {
                        windows.Add("Product Description: " + productInfo.Description + " <-");
                    }
                    else
                    {
                        windows.Add("Product Description: " + productInfo.Description);
                    }
                    if (pointer == 7) // product Enviroment
                    {
                        windows.Add("Product Enviroment: " + productInfo.EnviromentFriendly + " <-");
                    }
                    else
                    {
                        windows.Add("Product Enviroment: " + productInfo.EnviromentFriendly);
                    }
                    if (pointer == 8) // product Price
                    {
                        windows.Add("Product Price: " + productInfo.Price + " <-");
                    }
                    else
                    {
                        windows.Add("Product Price: " + productInfo.Price);
                    }
                    if (pointer == 9) // product Can be bought
                    {
                        windows.Add("Product Can be bought: " + productInfo.CanBeBought + " <-");
                    }
                    else
                    {
                        windows.Add("Product Can be bought: " + productInfo.CanBeBought);
                    }
                    windows.Add("[Q] Back");
                    AdminWindow.Draw();
                    windows.Clear();

                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (pointer == 9)
                        {
                            pointer = 0;
                        }
                        else
                        {
                            pointer = (pointer + 1) % 10;
                        }

                    }
                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (pointer == 0)
                        {
                            pointer = 9;
                        }
                        else
                        {
                            pointer = (pointer - 1) % 10;
                        }

                    }
                    else if (key.Key == ConsoleKey.E)
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            AdminWindow = new Window("", 10, 40, windows);
                            switch (pointer)
                            {
                                case 0: //name,
                                    {

                                        string changeTo = AdminTools.EnterValue(infoTitle[0]);
                                        string query = "UPDATE products SET ProductName = @changeTo WHERE ProductGroup = @productGroup";
                                        int nameUpdate = conn.Execute(query, new { changeTo, productGroup });
                                        break;
                                    }
                                case 1: //brand
                                    {
                                        int changeTo = AdminTools.ChooseBrand();

                                        string query = "UPDATE products SET Brand = @changeTo WHERE ProductGroup = @productGroup";
                                        int brandUpdate = conn.Execute(query, new { changeTo, productGroup });
                                        break;
                                    }
                                case 2: // category
                                    {
                                        string changeTo = AdminTools.ChooseCategory();
                                        Console.Clear();
                                        string query = "UPDATE products SET Category = @changeTo WHERE ProductGroup = @productGroup";
                                        int rowsAffected = conn.Execute(query, new { changeTo, productGroup });
                                        break;


                                    }
                                case 3: //gender
                                    {

                                        string changeTo = AdminTools.EnterValue(infoTitle[1]);
                                        string query = "UPDATE products SET Gender = @changeTo WHERE ProductGroup = @productGroup";
                                        int rowsAffected = conn.Execute(query, new { changeTo, productGroup });

                                        break;
                                    }
                                case 4: // size
                                    {
                                        Console.Clear();

                                        AdminTools.RemoveSize(productGroup);
                                        break;
                                    }
                                case 5://color.. får inte att fungera med array.......
                                    {
                                     

                                        break;
                                    }
                                case 6: //describtion
                                    {
                                        string changeTo = AdminTools.EnterValue(infoTitle[2]);
                                        string query = "UPDATE products SET Description = @ChangeTo WHERE ProductGroup = @productGroup";
                                        int rowsAffected = conn.Execute(query, new { changeTo, productGroup });
                                        break;
                                    }
                                case 7: //envirnoment
                                    {
                                        bool changeTo = AdminTools.EnterBoolValue(infoTitle[3]);


                                        string query = "UPDATE products SET EnviromentFriendly = @ChangeTo WHERE ProductGroup = @productGroup";
                                        int rowsAffected = conn.Execute(query, new { changeTo, productGroup });

                                        break;
                                    }
                                case 8://price
                                    {
                                        int price = AdminTools.EnterIntValue(infoTitle[4]);


                                        string query = "UPDATE products SET Price = @price WHERE ProductGroup = @productGroup";
                                        int rowsAffected = conn.Execute(query, new { price, productGroup });

                                        break;
                                    }
                                case 9: //can be bought or not
                                    {
                                        bool changeTo = true;
                                        Console.Clear();
                                        windows.Clear();
                                        windows.Add("The current status is " + productInfo.CanBeBought.ToString());
                                        windows.Add("Would you like to change the product group or one size?");
                                        windows.Add("            [G]roup      [S]ize   [Q] Back");
                                        AdminWindow = new Window("", 10, 40, windows);
                                        AdminWindow.Draw();

                                        key = Console.ReadKey();

                                        if (key.Key == ConsoleKey.G)
                                        {
                                            changeTo = !productInfo.CanBeBought;
                                            string query = "UPDATE products SET CanBeBought = @changeTo WHERE ProductGroup = @productGroup";
                                            int rowsAffected = conn.Execute(query, new { changeTo, productGroup });
                                        }
                                        else if (key.Key == ConsoleKey.S)
                                        {
                                            pointer = 0;
                                            while (true)
                                            {

                                                windows.Clear();
                                                Console.Clear();
                                                for (int i = 0; i < everySize.Count; i++)
                                                {
                                                    var sizes = everySize[i];
                                                    if (pointer == i)
                                                    {
                                                        windows.Add(sizes + productInfo.CanBeBought + " <-");
                                                    }
                                                    else
                                                    {
                                                        windows.Add(sizes);
                                                    }
                                                }
                                                windows.Add("[E} Pick what size to change to " + !productInfo.CanBeBought);
                                                windows.Add("[B] Back");
                                                AdminWindow.Draw();
                                                windows.Clear();

                                                key = Console.ReadKey(true);
                                                {
                                                    if (key.Key == ConsoleKey.UpArrow)
                                                    {
                                                        if (pointer == 0)
                                                        {
                                                            pointer = everySize.Count;
                                                        }
                                                        else
                                                        {
                                                            pointer--;
                                                        }

                                                    }
                                                    if (key.Key == ConsoleKey.DownArrow)
                                                    {
                                                        if (pointer == everySize.Count)
                                                        {
                                                            pointer = 0;
                                                        }
                                                        else
                                                        {
                                                            pointer++;
                                                        }

                                                    }
                                                    else if (key.Key == ConsoleKey.E)
                                                    {
                                                        string size = everySize[pointer];

                                                        changeTo = !productInfo.CanBeBought;
                                                        string query = "UPDATE products SET CanBeBought = @changeTo WHERE Size = @size and ProductGroup = @productGroup";
                                                        int rowsAffected = conn.Execute(query, new { changeTo, size, productGroup });
                                                    }
                                                    else if (key.Key == ConsoleKey.B)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    }
                            }

                        }


                    }

                    else if (key.Key == ConsoleKey.Q)
                    {
                        break;
                    }
                }
            }
        }






        public static void pointer5(int productGroup)
        {
            int[] colorId = AdminTools.NewColors();
            string connectionString = DataTracker.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = $"UPDATE products SET color = @{colorId} WHERE ProductGroup IN @{productGroup}";
                int rowsAffected = conn.Execute(query, new { colorId, productGroup });
            }
        }
    }
}


