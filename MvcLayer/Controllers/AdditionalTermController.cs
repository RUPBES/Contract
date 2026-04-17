using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.ContractServices;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MvcLayer.Controllers
{
    public class AdditionalTermController : Controller
    {
        private readonly IAdditionalTermService _additionalTermService;
        private readonly IAmendmentService _amendmentService;
        private readonly IFileService _fileService;
        private readonly IFormService _formService;
        private readonly IMapper _mapper;
        private readonly IHttpContextUserProvider _httpHelper;

        public AdditionalTermController(IAdditionalTermService additionalTermService, IMapper mapper,
            IAmendmentService amendmentService, IHttpContextUserProvider httpHelper, IFormService formService, IFileService fileService)
        {
            _additionalTermService = additionalTermService;
            _mapper = mapper;
            _amendmentService = amendmentService;
            _httpHelper = httpHelper;
            _fileService = fileService;
            _formService = formService;
        }


        [HttpGet]
        public ActionResult GetByContractId(int id, int returnContractId = 0)
        {
            ViewData["contractId"] = id;
            ViewData["returnContractId"] = returnContractId;
            return View(_mapper.Map<IEnumerable<AdditionalTermViewModel>>(_additionalTermService.Find(x => x.ContractId == id).OrderByDescending(x => x.Date)));
        }

        //[HttpGet]
        //[Route("/archive/Amendments")]
        //public ActionResult GetArchByContractId(int id, int returnContractId = 0)
        //{
        //    ViewData["contractId"] = id;
        //    ViewData["returnContractId"] = returnContractId;

        //    return View(_mapper.Map<IEnumerable<AmendmentViewModel>>(
        //        _amendment
        //            .Find(x => x.ContractId == id, useArchiveData: true)
        //            .OrderByDescending(x => x.Date)
        //        ));
        //}

        [Authorize(Policy = "EditPolicy")]
        public ActionResult Edit(int id, int? contractId = null, int returnContractId = 0)
        {
            ViewBag.contractId = contractId;
            ViewBag.returnContractId = returnContractId;
            return View(_mapper.Map<AdditionalTermViewModel>(_additionalTermService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditPolicy")]
        public ActionResult Edit(AdditionalTermViewModel additionalTerm, int returnContractId = 0)
        {
            if (additionalTerm is not null)
            {
                try
                {
                    _additionalTermService.Update(_mapper.Map<AdditionalTermDTO>(additionalTerm));
                    NotificationHelper.SetNotification(TempData, "Согласование срока исполнения обязательств обновлено", NotificationType.Info);
                }
                catch
                {
                    NotificationHelper.SetNotification(TempData, "Ошибка обновления согласования", NotificationType.Error);
                    return View();
                }
            }

            if (additionalTerm.ContractId is not null && additionalTerm.ContractId > 0)
            {
                return RedirectToAction(nameof(GetByContractId), new { id = additionalTerm.ContractId, returnContractId = returnContractId });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Policy = "DeletePolicy")]
        public ActionResult Delete(int id, int? contractId = null)
        {
            try
            {
                foreach (var item in _fileService.GetAttachedFiles(id, Folder.AdditionalTerms))
                {
                    _fileService.Delete(item.Id);
                }


                var additionalTerm = _additionalTermService.GetById(id);
                var amendment = _amendmentService?.Find(x => x.ContractId == additionalTerm.ContractId)?.LastOrDefault()?.DateEndWork;

                if ( additionalTerm.DueDate.HasValue && amendment.HasValue && DateComparer.IsLessYearAndMonth(amendment, additionalTerm.DueDate))
                {
                    var formsRemove = _formService.Find(x => x.ContractId == additionalTerm.ContractId
                        && !DateComparer.IsLessOrSameYearAndMonth(x.Period, amendment)
                        && x.IsOwnForces is false);

                    foreach (var form in formsRemove)
                    {
                        _formService.Delete(form.Id);
                    }
                }

                _additionalTermService.Delete(id);
                NotificationHelper.SetNotification(TempData, "Согласование срока исполнения обязательств удалено", NotificationType.Info);

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
                NotificationHelper.SetNotification(TempData, "Ошибка удаления согласования срока исполнения обязательств", NotificationType.Error);
                return View();
            }
        }
    }
}
