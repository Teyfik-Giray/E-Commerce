using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace deneme
{
    public class ProductManagementViewModel
    {
        public List<urunler> Products { get; set; }
        public List<kategoriler> Categories { get; set; }
        public List<stoklar> Stoklar{get;set;}
        public List<talepler> Talepler { get; set; }
    }
}