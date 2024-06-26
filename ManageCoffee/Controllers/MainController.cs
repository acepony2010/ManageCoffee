using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ManageCoffee.DAO;
using ManageCoffee.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ManageCoffee.Controllers
{
    public class MainController : Controller
    {
        private ManageCoffeeContext dbContext;

        public MainController()
        {
            dbContext = new ManageCoffeeContext();
        }
        public IActionResult Index()
        {
            return View();
        }

        public object Load()
        {
            return Json(new
            {
                products = ProductDAO.Instance.GetProductList(),
                tables = TableDAO.Instance.GetTableList()
            });
        }

        public object Create(IFormCollection request)
        {
            foreach (var field in request)
            {
                System.Console.WriteLine($"{field.Key}: {field.Value}");
            }
            Order order = new Order();
            User user = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
            order.UserId = 1;
            order.Status = 0;
            order.Note = request["Note"];
            if (request["table_id"] != "")
            {
                order.TableId = int.Parse(request["table_id"]);
            }
            order.TotalPrice = int.Parse(request["total_price"]);
            dbContext.Orders.Add(order);
            dbContext.SaveChanges();    
            for (int i = 0; i < request["quantity[]"].Count(); i++)
            {
                Detail detail = new Detail();
                detail.OrderId = order.OrderId;
                detail.ProductId = int.Parse(request["id[]"][i]);
                detail.Quantity = int.Parse(request["quantity[]"][i]);
                detail.Price = int.Parse(request["price[]"][i]);
                dbContext.Add(detail);
                dbContext.SaveChanges();
            }
            var table = dbContext.Tables.FirstOrDefault(t => t.TableId == order.TableId);
            if (table != null)
            {
                table.Status = 1;
                dbContext.SaveChanges();
            }
            return Json(new
            {
                msg = "Đã tạo đơn hàng thành công",
                status = "success",
            });
        }

        [HttpPost]
        public object Edit(IFormCollection request)
        {
            Order order = OrderDAO.Instance.GetOrderByID(int.Parse(request["id"]));
            User user = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
            System.Console.WriteLine(order.TableId);
            order.UserId = 1;
            order.TotalPrice = int.Parse(request["total_price"]);
            dbContext.Orders.Update(order);
            dbContext.SaveChanges();
            order.RemoveDetails();
            //Tạo detail
            System.Console.WriteLine(request["quantity[]"]+ " danh sách số lg");
            for (int i = 0; i < request["quantity[]"].Count(); i++)
            {
                Detail detail = new Detail();
                detail.OrderId = order.OrderId;
                detail.ProductId = int.Parse(request["id[]"][i]);
                detail.Quantity = int.Parse(request["quantity[]"][i]);
                detail.Price = int.Parse(request["price[]"][i]);
                System.Console.WriteLine("Đây là product ID: "+int.Parse(request["id[]"][i]));
                DetailDAO.Instance.AddNew(detail);
            }

            return Json(new
            {
                msg = "Đã cập nhật đơn hàng thành công",
                status = "success",
            });
        }

        public object GetOrderByTableId(int id)
        {
            Order order = dbContext.Orders.FirstOrDefault(o => o.TableId == id && o.Status == 0);
            if (order != null)
            {
                return Json(new
                {
                    order = order,
                    details = order.GetDetail(),
                    table = order.GetTable(),
                });
            } 
            else
            {
                return BadRequest();
            }
        }
    }
}