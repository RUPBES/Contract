using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using System.Diagnostics.Contracts;

namespace MvcLayer.Controllers
{
    public class AmendmentsController : Controller
    {
        private readonly IAmendmentService _amendment;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public AmendmentsController(IAmendmentService amendment, IMapper mapper, IFileService fileService)
        {
            _amendment = amendment;
            _mapper = mapper;
            _fileService = fileService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(_amendment.GetAll()));
        }

        [HttpGet]
        public ActionResult GetByContractId(int id, int returnContractId = 0)
        {
            return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(_amendment.Find(x => x.ContractId == id)));
        }

        public ActionResult Create(int contractId, int returnContractId = 0)
        {
            ViewData["contractId"] = contractId;
            ViewData["returnContractId"] = returnContractId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AmendmentViewModel amendment, int returnContractId = 0)
        {
            try
            {
                int amendId = (int)_amendment.Create(_mapper.Map<AmendmentDTO>(amendment));
                int fileId = (int)_fileService.Create(amendment.FilesEntity, FolderEnum.Amendment, amendId);

                _amendment.AddFile(amendId, fileId);

                return RedirectToAction(nameof(GetByContractId), new { id = amendment.ContractId, returnContractId = returnContractId });
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id, int? contractId = null, int returnContractId = 0)
        {
            ViewBag.contractId = contractId;
            ViewBag.returnContractId = returnContractId;
            return View(_mapper.Map<AmendmentViewModel>(_amendment.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AmendmentViewModel amendment, int returnContractId = 0)
        {
            if (amendment is not null)
            {
                try
                {
                    _amendment.Update(_mapper.Map<AmendmentDTO>(amendment));
                }
                catch
                {
                    return View();
                }
            }
            if (amendment.ContractId is not null && amendment.ContractId > 0)
            {
                return RedirectToAction(nameof(GetByContractId), new { id = amendment.ContractId, returnContractId = returnContractId });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public ActionResult Delete(int id, int? contractId = null)
        {
            try
            {
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Amendment))
                {
                    _fileService.Delete(item.Id);
                }

                _amendment.Delete(id);
                if (contractId is not null && contractId > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { id = contractId });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                return View();
            }
        }
    }
}