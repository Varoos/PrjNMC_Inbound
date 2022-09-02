using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrjNMC_Inbound
{
    public class ClsSalesInvoice
    {
        public class SalesInvoice
        {
            public Header Header { get; set; }
            public List<Body> Body { get; set; }
        }

        public class Header
        {
            public string CustomerCountry { get; set; }
            public string CustomerName { get; set; }
            public string CurrencyName { get; set; }
            public string HotelMaster { get; set; }
            public string ConfirmationNo { get; set; }
            public string SupplierCode { get; set; }
            public string HotelCity { get; set; }
            public string HotelCountry { get; set; }
            public string ExchangeRate { get; set; }
            public string HotelPhoneNumber { get; set; }
            public string SupplierConfirmationNumber { get; set; }
            public string ClientConfirmationNumber { get; set; }
            public string AgencyVATNumber { get; set; }
            public string AgencyAddress { get; set; }
            public string Status { get; set; }
        }
        public class Body
        {
            public string Item { get; set; }
            public string NoOfPerson { get; set; }
            public string Quantity { get; set; }
            public string Rate { get; set; }
            public string ServiceCharges { get; set; }
            public string MunicipalityFee { get; set; }
            public string VAT { get; set; }
            public string Gross { get; set; }
            public string CheckInDate { get; set; }
            public string CheckOutDate { get; set; }
            public string BookingDate { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string CountryCode { get; set; }
            public string City { get; set; }
            public string StreetAddr { get; set; }
            public string PostCode { get; set; }
            public string PhoneNo { get; set; }
            public string MobilePhone { get; set; }
            public string MealPlan { get; set; }
            public string NoofNights { get; set; }
            public string Adults { get; set; }
            public string Children { get; set; }
            public string NationalityOfGuest { get; set; }
        }

      
        public class SalesResponse
        {
            public int iStatus { get; set; }
            public string sMessage { get; set; }
            public string sVouchrNo { get; set; }
        }

    }
}