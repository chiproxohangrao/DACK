using DACK.Helpers;
using DACK.Models;
using DACK.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ShoppingCartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IProductRepository _productRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    public ShoppingCartController(IProductRepository productRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _productRepository = productRepository;
        _userManager= userManager;
        _context= context;

    }
    public async Task<IActionResult> AddToCart(int productId, int quantity)
    {
        // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
        Product product = await GetProductFromDatabaseAsync(productId);
        var cartItem = new CartItem
        {
            ProductId = productId,
            Name = product.Name,
            Price = product.Price,
            Quantity = quantity
        };
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        cart.AddItem(cartItem);
        HttpContext.Session.SetObjectAsJson("Cart", cart);
        return RedirectToAction("Index");
    }
    [Authorize]
    [HttpGet]
    public IActionResult Checkout()
    {
        return View(new Order());
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Checkout(Order order)
    {
        var cart =
        HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart == null || !cart.Items.Any())
        {
            // Xử lý giỏ hàng trống...
            return RedirectToAction("Index");
        }
        var user = await _userManager.GetUserAsync(User);
        order.UserId = user.Id;
        order.OrderDate = DateTime.UtcNow;
        order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        order.OrderDetails = cart.Items.Select(i => new OrderDetail
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList();
        //Lưu đơn hàng
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("Cart");
        return View("OrderCompleted", order.Id); // Trang xác nhận hoàn  thành đơn hàng
}

public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        return View(cart);
    }
    // Các actions khác...
    private async Task<Product> GetProductFromDatabaseAsync(int productId)
    {
        // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm

        return await _productRepository.GetByIdAsync(productId);

    }
    [HttpPost]
    public IActionResult RemoveFromCart(int productId)
    {
        // Lấy giỏ hàng từ Session
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

        // Kiểm tra xem giỏ hàng có tồn tại không
        if (cart != null)
        {
            // Tìm sản phẩm cần xóa trong giỏ hàng dựa vào productId được truyền từ form
            var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            // Nếu tìm thấy sản phẩm, xóa nó khỏi giỏ hàng
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                // Lưu lại giỏ hàng vào Session
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
        }

        // Chuyển hướng người dùng trở lại trang giỏ hàng sau khi xóa sản phẩm
        return RedirectToAction("Index");
    }
    [Authorize]
    public IActionResult PaymentCallBack()
    {
        return View();
    }
}
