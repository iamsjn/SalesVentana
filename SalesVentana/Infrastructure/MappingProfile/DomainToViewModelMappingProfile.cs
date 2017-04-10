using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using SalesVentana.BO;

namespace SalesVentana
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public override string ProfileName
        {
            get
            {
                return "DomainToViewModelMappings";
            }
        }

        protected override void Configure()
        {
        }
    }
}