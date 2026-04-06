using AutoServiceCatalog.DAL.Entities;
using AutoServiceCatalog.DAL.QueryParametrs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Specefication
{
    public class ServiceSpecification : Specification<Service>
    {
        public ServiceSpecification(PartQueryParameters parameters)
        {
            Criteria = s =>
                (string.IsNullOrEmpty(parameters.Search) ||
                    s.Name.Contains(parameters.Search)) &&
                (!parameters.CategoryId.HasValue ||
                    s.CategoryId == parameters.CategoryId.Value);

            // Include залежностей
            Includes.Add(s => s.Category);
            Includes.Add(s => s.ServiceDetail);
            Includes.Add(s => s.ServiceSuppliers);

            // Сортування
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                OrderBy = parameters.SortBy switch
                {
                    "name" => q => q.OrderBy(s => s.Name),
                    "price" => q => q.OrderBy(s => s.Price),
                    _ => OrderBy
                };

                if (parameters.Desc)
                {
                    OrderByDescending = parameters.SortBy switch
                    {
                        "name" => q => q.OrderByDescending(s => s.Name),
                        "price" => q => q.OrderByDescending(s => s.Price),
                        _ => OrderByDescending
                    };
                }
            }

            Skip = (parameters.PageNumber - 1) * parameters.PageSize;
            Take = parameters.PageSize;
        }
    }
}
