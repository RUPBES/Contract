using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class CommissionActsController : Controller
    {
        private readonly ICommissionActService _commissionActService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public CommissionActsController(ICommissionActService commissionActService, IMapper mapper, IFileService fileService)
        {
            _commissionActService = commissionActService;
            _mapper = mapper;
            _fileService = fileService;
        }

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<CommissionActViewModel>>(_commissionActService.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<CommissionActViewModel>>(_commissionActService.Find(x => x.ContractId == contractId)));
        }

        public ActionResult Create(int id)
        {
            ViewData["id"] = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CommissionActViewModel commissionAct)
        {
            try
            {
                int fileId = (int)_fileService.Create(commissionAct.FilesEntity, FolderEnum.CommissionActs);
                int commissionActId = (int)_commissionActService.Create(_mapper.Map<CommissionActDTO>(commissionAct));
                _commissionActService.AddFile(commissionActId, fileId);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            return View(_mapper.Map<CommissionActViewModel>(_commissionActService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CommissionActViewModel commissionAct)
        {
            if (commissionAct is not null)
            {
                try
                {
                    _commissionActService.Update(_mapper.Map<CommissionActDTO>(commissionAct));
                }
                catch
                {
                    return View();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(int id)
        {
            try
            {
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.CommissionActs))
                {
                    _fileService.Delete(item.Id);
                }

                _commissionActService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}