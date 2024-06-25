using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Repositories
{
    internal class FormC3Repository : IRepository<FormC3a>
    {
        private readonly ContractsContext _context;
        public FormC3Repository(ContractsContext context)
        {
            _context = context;
        }

        public void Create(FormC3a entity)
        {
            if (entity is not null)
            {
                _context.FormC3as.Add(entity);
            }
        }

        public void Delete(int id, int? secondId = null)
        {
            FormC3a form = _context.FormC3as.Find(id);

            if (form is not null)
            {
                _context.FormC3as.Remove(form);
            }
        }

        public IEnumerable<FormC3a> Find(Func<FormC3a, bool> predicate)
        {
            return _context.FormC3as.Where(predicate).ToList();
        }

        public IEnumerable<FormC3a> Find(Func<FormC3a, bool> where, Func<FormC3a, FormC3a> select)
        {
            return _context.FormC3as.Where(where).Select(select).ToList();
        }        

        public IEnumerable<FormC3a> GetAll()
        {
            return _context.FormC3as.ToList();
        }

        public FormC3a GetById(int id, int? secondId = null)
        {
            if (id > 0)
            {
                return _context.FormC3as.Find(id);
            }
            else
            {
                return null;
            }
        }

        public void Update(FormC3a entity)
        {
            if (entity is not null)
            {
                var form = _context.FormC3as.Find(entity.Id);

                if (form is not null)
                {                    
                    form.DateSigning = entity.DateSigning;
                    form.SmrContractCost = entity.SmrContractCost ?? 0;
                    form.SmrCost = entity.SmrCost ?? 0;
                    form.PnrCost = entity.PnrCost ?? 0;
                    form.EquipmentCost = entity.EquipmentCost ?? 0;
                    form.OtherExpensesCost = entity.OtherExpensesCost ?? 0;
                    form.AdditionalCost = entity.AdditionalCost ?? 0;
                    form.MaterialCost = entity.MaterialCost??0;
                    form.GenServiceCost = entity.GenServiceCost ?? 0;
                    
                    form.OffsetCurrentPrepayment = entity.OffsetCurrentPrepayment??0;
                    form.OffsetTargetPrepayment = entity.OffsetTargetPrepayment??0;

                    form.IsExemptFromVAT = entity.IsExemptFromVAT ?? false;
                    form.MaterialClientCost = entity.MaterialClientCost ?? 0;
                    form.EquipmentClientCost = entity.EquipmentClientCost;
                    form.CostToConstructionIndustryFund = entity.CostToConstructionIndustryFund;
                    form.СostStatisticReportOfContractor = entity.СostStatisticReportOfContractor ?? 0;
                    form.SmrNdsCost = entity.SmrNdsCost ?? 0;
                    form.AdditionalNdsCost = entity.AdditionalNdsCost ?? 0;

                    _context.FormC3as.Update(form);
                }
            }
        }
    }
}

