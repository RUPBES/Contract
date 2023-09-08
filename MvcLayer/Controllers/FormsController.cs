using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Mvc;
using MvcLayer.Models;

namespace MvcLayer.Controllers
{
    public class FormsController : Controller
    {
        private readonly IFormService _formService;
        private readonly IFileService _fileService;
        private readonly IScopeWorkService _scopeWork;
        private readonly IMapper _mapper;

        public FormsController(IFormService formService, IMapper mapper, IFileService fileService, IScopeWorkService scopeWork)
        {
            _formService = formService;
            _mapper = mapper;
            _fileService = fileService;
            _scopeWork = scopeWork;
        }

        public ActionResult Index()
        {
            return View(_mapper.Map<IEnumerable<FormViewModel>>(_formService.GetAll()));
        }

        public IActionResult GetByContractId(int contractId)
        {
            return View(_mapper.Map<IEnumerable<FormViewModel>>(_formService.Find(x => x.ContractId == contractId)));
        }

        public IActionResult ChoosePeriod(int contractId, bool isOwnForces)
        {
            if (contractId > 0)
            {
                // по объему работ, берем начало и окончание периода
                var period = _scopeWork.GetDatePeriodLastOrMainScopeWork(contractId);
                
                var periodChoose = new PeriodChooseViewModel
                {
                    ContractId = contractId,
                    IsOwnForces = isOwnForces,
                    PeriodStart = period.Value.Item1,
                    PeriodEnd = period.Value.Item2,
                };

                DateTime startDate = period.Value.Item1;

                if (period is null)
                {
                    return RedirectToAction("Details", "Contracts", new { id = contractId });
                }

                // определяем, есть уже формы собственными силами (флаг IsOwnForces = true)

                var formExist = _formService
                    .Find(x => x.ContractId == contractId && x.IsOwnForces == isOwnForces);

                if (formExist.Count() > 0)
                {
                    //если уже заполнена форма по какой-то дате, берем эту дату и по ней обрезаем список
                    //с начала дат, остальное отправляем на вью
                    var lastDate = formExist.LastOrDefault().Period;

                    if (startDate < lastDate)
                    {
                        startDate = (DateTime)lastDate;
                    }
                    else if(startDate == lastDate)
                    {
                        startDate = startDate.AddMonths(1);
                    }


                    if (startDate <= period?.Item2)
                    {
                        //заполняем период в виде списка, для выбора месяца по которому будет заполняться форма с3 - а
                        while (startDate <= period?.Item2)
                        {
                            periodChoose.ListDates.Add(startDate);
                            startDate = startDate.AddMonths(1);
                        }
                        return View(periodChoose);
                    }
                    else
                    {
                        return RedirectToAction("Details", "Contracts", new { id = contractId });
                    }
                }
                else
                {

                    while (startDate <= period?.Item2)
                    {
                        periodChoose.ListDates.Add(startDate);
                        startDate = startDate.AddMonths(1);
                    }
                    return View(periodChoose);
                }

            }
            return View();
        }

        public ActionResult CreateForm(PeriodChooseViewModel model/*int contractId, DateTime? date = null*/)
        {

            return View("AddForm", new FormViewModel { Period = model.ChoosePeriod, ContractId = model.ContractId, IsOwnForces = model.IsOwnForces });
        }

        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormViewModel formViewModel)
        {
            try
            {
                int fileId = (int)_fileService.Create(formViewModel.FilesEntity, FolderEnum.Form3C);
                int formId = (int)_formService.Create(_mapper.Map<FormDTO>(formViewModel));
                _formService.AddFile(formId, fileId);

                return RedirectToAction(nameof(GetByContractId), new { contractId = formViewModel.ContractId});
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            return View(_mapper.Map<FormViewModel>(_formService.GetById(id)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormViewModel formViewModel)
        {
            if (formViewModel is not null)
            {
                try
                {
                    _formService.Update(_mapper.Map<FormDTO>(formViewModel));
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
                foreach (var item in _fileService.GetFilesOfEntity(id, FolderEnum.Form3C))
                {
                    _fileService.Delete(item.Id);
                }

                _formService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}