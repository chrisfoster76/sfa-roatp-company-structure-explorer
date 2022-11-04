using System;
using System.Collections.Generic;
using System.Text;

namespace RoatpCompanyStructureExplorer.Models
{

    public class OfficersResponse
    {
        public string etag { get; set; }
        public List<Item> items { get; set; }
        public string kind { get; set; }
        public int active_count { get; set; }
        public int items_per_page { get; set; }
        public Links links { get; set; }
        public int total_results { get; set; }
        public int inactive_count { get; set; }
        public int start_index { get; set; }
        public int resigned_count { get; set; }

        public class Address
        {
            public string country { get; set; }
            public string address_line_1 { get; set; }
            public string locality { get; set; }
            public string premises { get; set; }
            public string postal_code { get; set; }
            public string region { get; set; }
        }

        public class DateOfBirth
        {
            public int year { get; set; }
            public int month { get; set; }
        }

        public class Item
        {
            public string occupation { get; set; }
            public string nationality { get; set; }
            public Address address { get; set; }
            public string country_of_residence { get; set; }
            public Links links { get; set; }
            public string name { get; set; }
            public string officer_role { get; set; }
            public string appointed_on { get; set; }
            public DateOfBirth date_of_birth { get; set; }
            public string resigned_on { get; set; }
        }

        public class Links
        {
            public Officer officer { get; set; }
            public string self { get; set; }
        }

        public class Officer
        {
            public string appointments { get; set; }
        }
    }



}
