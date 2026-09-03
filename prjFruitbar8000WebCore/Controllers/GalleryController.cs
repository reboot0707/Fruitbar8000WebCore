using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjFruitbar8000WebCore.Models;
using prjFruitbar8000WebCore.Models.Services;
using prjFruitbar8000WebCore.Models.ViewModels;

namespace prjFruitbar8000WebCore.Controllers
{
    public class GalleryController : Controller
    {
        private readonly FruitBarDbContext _context;
        public GalleryController(FruitBarDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(List));
        }

        // GET: QueryController
        public async Task<IActionResult> List()
        {
            // 修改前作邏輯, 參考某音樂平台, 先用歌曲為單位列表
            List<GalleryListViewModel> queryList = new List<GalleryListViewModel>();

            queryList = new GalleryDataAccess().List(queryList, _context);

            return View(queryList);
        }

        public async Task<IActionResult> Create()
        {
            GallerySongViewModel nsvm = await new GalleryDataAccess().GetCreate(_context);
            return View(nsvm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(GallerySongViewModel nsvmSent)
        {
            await new GalleryDataAccess().PostCreate(nsvmSent, _context);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return RedirectToAction(nameof(List));
            }
            var editSong = await _context.TSongs
                .Where(x => x.FSongId == id)
                .Include(x => x.TArtistsSongs)
                .Include(x => x.TSongsAlbums)
                .FirstOrDefaultAsync();
            if (editSong is null)
            {
                return RedirectToAction(nameof(List));
            }
            GallerySongViewModel infoEditSong = await new GalleryDataAccess().GetEdit(editSong, _context);
            return View(infoEditSong);
        }

        // FIXED: 修正差集同步邏輯, 目前邏輯會踩到中介表 FK 不能為空值的坑
        [HttpPost]
        public async Task<IActionResult> Edit(GallerySongViewModel gsvm)
        {
            if (gsvm is null)
            {
                return RedirectToAction(nameof(List));
            }
            // FIXED: 此查詢必須一併載入 TArtistsSongs 與 TSongsAlbums。導覽集合雖已初始化但未代表資料庫內容，
            // 因此 PostEdit() 的 existRelationInList 會把既有關聯誤判為不存在，最後 INSERT 時撞上複合唯一索引。
            var tobeUpdate = _context.TSongs
                .Include(x => x.TArtistsSongs)
                .Include(x => x.TSongsAlbums)
                .FirstOrDefault(x => x.FSongId == gsvm.id);
            if (tobeUpdate is null)
            {
                return View(gsvm);
            }
            bool isSuccess = await new GalleryDataAccess().PostEdit(gsvm, tobeUpdate, _context);
            if (isSuccess)
            {
                return RedirectToAction(nameof(List));
            }
            return View(gsvm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return RedirectToAction(nameof(List));
            }
            await new GalleryDataAccess().Delete(id, _context);
            return RedirectToAction(nameof(List));
        }
    }
}
