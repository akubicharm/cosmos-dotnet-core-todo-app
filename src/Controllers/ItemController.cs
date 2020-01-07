namespace todo.Controllers
{
    using System;
    using System.Threading.Tasks;
    using todo.Services;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Microsoft.Extensions.Logging;

    public class ItemController : Controller
    {

        private readonly ICosmosDbService _cosmosDbService;

        private readonly ILogger _logger;

        public ItemController(ICosmosDbService cosmosDbService, ILogger<ItemController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _cosmosDbService.GetItemsAsync("SELECT * FROM c"));
        }

        [ActionName("ForceError")]
        public async Task<IActionResult> ForceError() {
            _logger.LogError("# DEBUG MESSAGE : FORCE ERROR CALLED");

            Item item = await _cosmosDbService.GetItemAsync(null);
            return RedirectToAction("Index");
        }

        [ActionName("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("Id,Name,Description,Completed")] Item item)
        {

            if (string.Equals(item.Name?.TrimEnd(), "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Cannn register item with title {item.Name}.", nameof(item.Name));
            }
            if (ModelState.IsValid)
            {
                item.Id = Guid.NewGuid().ToString();
                await _cosmosDbService.AddItemAsync(item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("Id,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                await _cosmosDbService.UpdateItemAsync(item.Id, item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Item item = await _cosmosDbService.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Item item = await _cosmosDbService.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind("Id")] string id)
        {
            await _cosmosDbService.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            _logger.LogWarning("# DEBUG MESSAGE# Details called");
            return View(await _cosmosDbService.GetItemAsync(id));
        }
    }
}