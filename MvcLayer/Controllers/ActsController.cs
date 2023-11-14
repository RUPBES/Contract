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
    public class ActsController : Controller
    {
        private readonly IActService _actService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ActsController(IActService actService, IMapper mapper, IFileService fileService)
        {
            _actService = actService;
            _mapper = mapper;
            _fileService = fileService;
        }

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<ActViewModel>>(_actService.GetAll()));
        }

        public IActionResult GetByContractId(int id)
        {
            ViewData["contractId"] = id;
            return View(_mapper.Map<IEnumerable<ActViewModel>>(_actService.Find(x => x.ContractId == id)));
        }

        public ActionResult Create(int contractId)
        {
            ViewData["contractId"] = contractId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ActViewModel actViewModel)
        {
            try
            {
                int fileId = (int)_fileService.Create(actViewModel.FilesEntity, FolderEnum.Acts);
                int actId = (int)_actService.Create(_mapper.Map<ActDTO>(actViewModel));
                _actService.AddFile(actId, fileId);
                
                //если запрос пришел с детальной инфы по договору, тогда редиректим туда же
                if (actViewModel.ContractId is not null &&  actViewModel.ContractId > 0)
                {
                    return RedirectToAction(nameof(GetByContractId), new { id = actViewModel.ContractId });
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

        public ActionResult Edit(int id, int? contractId = null)
        {
            ViewBag.contractId = contractId;
            return View(_mapper.Map<ActViewModel>(_actService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ActViewModel act)
        {
            if (act is not null)
            {
                try
                {
                    _actService.Update(_mapper.Map<ActDTO>(act));
                }
                catch
                {
                    return View();
                }
            }
            if (act?.ContractId is not null && act.ContractId > 0)
            {
                return RedirectToAction(nameof(GetByContractId), new { id = act.ContractId });
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
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Acts))
                {
                    _fileService.Delete(item.Id);
                }

                _actService.Delete(id);

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